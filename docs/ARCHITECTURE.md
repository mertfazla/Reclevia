# Architecture and Structure

## Decisions

Use a **modular monolith**: one application with named business areas and one database. Organize use cases as small feature folders. Ordinary method calls and a shared transaction keep financial workflows understandable.

Start with two production projects: `Reclevia.Core` contains business rules; `Reclevia.Api` contains HTTP endpoints, persistence, adapters, and use-case orchestration. Core does not depend on EF Core or ASP.NET Core. Api depends on Core. Add test projects beside them.

One DbContext spans the business modules; configurations and tables remain grouped by module/schema. This is a deliberate coupling: invoice/payment/ledger updates can commit together. A use case owns the transaction and calls module services; it does not scatter cross-module writes across controllers.

## Technologies and why

| Choice | Purpose / when introduced |
| --- | --- |
| .NET 10 LTS / C# 14 | Supported foundation; use familiar language features. [Support policy](https://dotnet.microsoft.com/en-us/platform/support/policy). |
| ASP.NET Core controllers + built-in DI | Explicit HTTP endpoints and dependency wiring. |
| EF Core 10 + Npgsql 10 provider | Queries, mapping, migrations, transactions; inspect generated SQL. [Provider release](https://www.npgsql.org/efcore/release-notes/10.0.html). |
| Windows PostgreSQL + pgAdmin | Existing local database server and a visual SQL/data tool. Record installed major version; use the same supported major in CI. |
| ASP.NET Core Identity + secure cookies | Account/password handling; tenant roles remain application membership policies. [Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0). |
| Built-in validation + Problem Details | Small request validation and consistent HTTP errors; domain rules stay in Core. |
| OpenAPI + Scalar | Document and try the API during development. Restrict interactive documentation outside development. |
| BackgroundService + PostgreSQL inbox/outbox | Durable asynchronous work without introducing a broker. |
| Razor Pages + plain CSS | Small same-origin finance UI, added after business flows work; no separate JavaScript stack. |
| xUnit + WebApplicationFactory + real PostgreSQL | Fast rule tests and meaningful HTTP/database/concurrency tests. |
| ILogger + OpenTelemetry | Structured logs first; traces and metrics when complete workflows exist. |
| Git + GitHub Actions + Docker | Version history, repeatable checks, and final application packaging. Local PostgreSQL stays on Windows. |

Pin exact SDK and package versions at setup; use compatible supported 10.x releases. A runtime patch number is not an SDK version. No initial Redis, RabbitMQ, Kafka, MediatR, generic repository, microservices, Kubernetes, or event sourcing: add a tool only for a demonstrated requirement. An immutable journal alone is not event sourcing.

## Planned repository tree

```text
reclevia/
|-- README.md
|-- ROADMAP.md
|-- LICENSE
|-- docs/                         # Domain, architecture, diagrams, setup, GitHub, sources
|-- src/
|   |-- Reclevia.Core/
|   |   |-- Common/                # Money and small shared business primitives
|   |   |-- Tenancy/
|   |   |-- Receivables/           # Customers, invoices, credit notes
|   |   |-- Payments/              # Attempts, allocations, refunds
|   |   |-- Ledger/                # Accounts, journals, postings
|   |   |-- Reconciliation/        # Batches, statements, exceptions
|   |   `-- SaaSBilling/           # Our plans, subscriptions, entitlements
|   `-- Reclevia.Api/
|       |-- Features/              # Same business areas; one folder per use case
|       |   `-- Receivables/IssueInvoice/
|       |       |-- Endpoint.cs
|       |       |-- Request.cs
|       |       `-- Handler.cs
|       |-- Persistence/           # DbContext, module configurations, migrations
|       |-- Identity/              # Authentication, tenant context, authorization
|       |-- Integrations/          # PaymentProvider and local simulator adapter
|       |-- Background/            # Inbox/outbox dispatch, retries, subscriptions
|       |-- Pages/                 # Razor Pages added later
|       `-- Program.cs
|-- tests/
|   |-- Reclevia.UnitTests/
|   `-- Reclevia.IntegrationTests/
|-- samples/                       # Synthetic provider/bank fixtures and demo steps
|-- .github/workflows/ci.yml
|-- .editorconfig
|-- .gitignore
|-- global.json
|-- Directory.Packages.props
|-- Dockerfile
`-- Reclevia.slnx                  # .sln is also supported
```

Create folders only when their roadmap step needs them. A handler is an ordinary use-case class, not a mediator framework. Pages and controllers invoke the same use cases. Reports initially live beside the business area they query.

## Transactions and asynchronous boundaries

- **Local transaction:** business state, financial journal, audit, idempotency result, and outbox commit together. On failure all roll back.
- **External request:** commit the attempt and outbox first; a worker calls the provider afterward with a stable operation key. HTTP and PostgreSQL cannot share an atomic transaction.
- **Incoming webhook:** verify raw-body signature/time window, resolve the provider account, insert a unique inbox event, commit, then acknowledge. Process the event later. Dedupe both event IDs and financial source IDs. [Webhook behavior](https://docs.stripe.com/webhooks).
- **Recovery:** claim jobs with expiring leases; retry transient failures with capped backoff; expose exhausted attempts for review/replay. Check the provider after unknown outcomes. Delivery is at least once; stored uniqueness makes business effects repeat-safe.
- **Concurrency:** optimistic version checks protect draft edits. Short PostgreSQL row locks protect invoice allocation, numbering counters, and refund limits. Acquire related locks in a fixed order; retry an entire transaction only when safe. [Row locks](https://www.postgresql.org/docs/current/explicit-locking.html).

Use tenant-scoped unique keys for numbers, requests, and external references. Use composite foreign keys including TenantId to prevent cross-tenant relationships. EF query filters help reads; they do not authorize requests or validate writes. Explicit tenant predicates are required in SQL, jobs, and exports.

## Small HTTP contract

Use `/api/v1/tenants/{tenantId}/...`; validate membership before accessing data. Core resources: `customers`, `invoices`, `payment-attempts`, `refunds`, `settlement-imports`, `bank-imports`, `reconciliation-cases`, `reports`, and `subscription`. Invoice actions include `/{id}/issue` and `/{id}/credit-notes`.

Money-changing POSTs require `Idempotency-Key`, scoped by tenant and operation with a normalized request hash. Keep results for the v1 dataset lifetime. Return 201 for created resources, 202 plus a status URL for asynchronous work, 400 for invalid input, 401/403 for authentication/permission errors, 404 for missing or other-tenant records, and 409 for conflicting state/key reuse. Lists have bounded pagination. Errors use Problem Details with a stable business code and trace ID.

Cookie-authenticated writes require an antiforgery token. `/webhooks/provider` uses signature authentication and provider-account mapping instead. Separate SaaS-billing provider references from tenant collection references.

# Sequential Roadmap

Complete steps in order. Each step includes its own tests and documentation updates. Later verification steps combine the features; they do not postpone correctness work.

For every step: **explain the business problem -> write examples -> implement the smallest change -> test failures -> explain the result -> commit.**

| Step | Build or study | Done when |
| --- | --- | --- |
| 01 | Read DOMAIN; draw the tenant, customer, provider, and bank. Explain invoice, collection, fee, payout, and reconciliation. | You explain the EUR 1,000 example without technical vocabulary. |
| 02 | Review scope and diagrams. Write examples for partial payment, duplicate notification, failed payment, refund, and missing deposit. Confirm the architecture decisions. | Every example has an expected balance and outcome. |
| 03 | Follow GITHUB to publish the documentation baseline as `reclevia`. | The README links and Mermaid diagrams work on GitHub; no feature is marked implemented. |
| 04 | Prepare Visual Studio Community 2026 with ASP.NET and web development, .NET 10 SDK, pgAdmin, and existing PostgreSQL. Record actual versions and connection details without credentials. | You can inspect PostgreSQL and run the SDK; existing databases remain intact. |
| 05 | Follow [Visual Studio setup](docs/VISUAL-STUDIO-SETUP.md): create the solution, physical folders, four projects, references, SDK/package pins, formatting rules, and local settings. | A health endpoint runs; build and first smoke test pass. |
| 06 | Add EF Core/Npgsql, one DbContext, schema conventions, first migration, and separate development/test databases. Add CI build/test with PostgreSQL. | A fresh test database migrates successfully; CI passes. |
| 07 | Add Identity, secure cookie login/logout, antiforgery protection, Tenant and Membership. Add Owner, Finance, and Viewer policies; audit membership changes. | Two tenants cannot read or mutate each other's records; Viewer cannot write. |
| 08 | Add business customers, Money, currency checks, UTC event times, DateOnly due dates, validation, and Problem Details. | Invalid amounts, cross-tenant references, and duplicate customer codes are rejected. |
| 09 | Build accounts, journal entries, postings, immutable corrections, and ledger queries. | Unbalanced or mixed-currency entries fail; posting twice for one business event is impossible. |
| 10 | Build invoice lines, draft editing, issue, unique numbering, and due dates. Issue the receivable journal in the same transaction. | An issued invoice is immutable and creates exactly one balanced journal. |
| 11 | Add stored API idempotency results, invoice row locks, optimistic draft version checks, audit events, and a transactional outbox. | Concurrent issue requests produce one invoice effect; changed payload with the same key conflicts. |
| 12 | Build a local provider simulator with persistent operation IDs, signed events, status lookup, and programmable failures. Define the PaymentProvider interface. | Simulator tests reproduce success, failure, timeout, duplicates, and reordered events. |
| 13 | Build the durable inbox and background dispatcher: leases, retries/backoff, attempt limits, failure queue, and manual replay. | A crash/restart loses no committed work; duplicate delivery creates no duplicate effect. |
| 14 | Add payment initiation, durable dispatch, provider confirmation, partial allocation, and overpayment suspense. Commit payment, allocation, journal, audit, and outbox together. | Partial payments update outstanding correctly; a timed-out request stays unresolved until checked; duplicate confirmations have one effect. |
| 15 | Add credit notes and full/partial refunds, including reserved pending refund amounts and permission checks. | Concurrent refunds cannot exceed the captured amount; failed refunds remain owed to the customer. |
| 16 | Import provider settlement batches and fees. Record provider payout status separately from bank receipt. | Gross payments minus refunds and fees explain a payout; clearing remains nonzero until supported settlement evidence exists. |
| 17 | Import bank-statement CSV, reconcile by stable references/currency/amount, and manage exceptions and adjustments. | Reimport is harmless; missing, ambiguous, and mismatched rows require an explained resolution. |
| 18 | Add receivables aging, clearing/bank summaries, refund liabilities, audit search, and CSV export. | Report totals agree with the underlying ledger and sample fixtures; exports remain tenant-scoped. |
| 19 | Add Reclevia subscription plans, monthly billing cycles, provider events, renewal retries, grace periods, cancellation, and entitlements. | Duplicate renewal events do not double-bill; expiry blocks new business writes but preserves login, recovery, read/export, and financial event processing. |
| 20 | Add Razor Pages for customers, invoices, payment history, reconciliation exceptions, reports, membership, and subscriptions. | A finance user completes the core flow in the browser; permissions and CSRF protection apply everywhere. |
| 21 | Complete integrated failure drills: parallel requests, reordered events, unknown outcomes, authorization failures, and process restarts. | The required scenarios in DEVELOPMENT pass on real PostgreSQL with deterministic assertions. |
| 22 | Add structured logs, correlation IDs, metrics/traces, job-age alerts, readiness checks, and documented recovery. Test backups/restores and safe migration deployment. | You can trace a payment end to end and restore the demonstration into a separate database. |
| 23 | Package the app with Docker; keep Windows PostgreSQL for everyday development. Verify a clean local install and optionally a private demo host. | Another engineer can follow the final setup instructions; CI checks formatting, build, migrations, tests, and package creation. |
| 24 | Record the demo, update implemented status/setup/screenshots, review secrets and sample data, check MIT/third-party notices, and tag `v1.0.0`. | The README accurately describes a reproducible release and its limits. |

## Later domain tracks — outside version 1

Study in order: **25** disputes/chargebacks and evidence deadlines; **26** tax documents and credit-note rules for one jurisdiction; **27** currencies, FX, and rounding; **28** real provider onboarding, hosted checkout, PCI scope, KYB/KYC, AML, and country availability; **29** bank feeds and accounting-system exports; **30** scale only after measuring a bottleneck.

Each track starts with current official requirements, worked money examples, and revised acceptance criteria. Version 1 does not establish readiness to handle real funds.

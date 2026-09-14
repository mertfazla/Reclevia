# Official Reading Map

References checked on **2026-09-14**. These explain technology/provider behavior; application scope, accounting examples, roles, and subscription policy are documented design decisions. The architecture is specific to this receivables workflow.

| Read when | Source | Question to answer |
| --- | --- | --- |
| Setup | [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy) | Why choose .NET 10 LTS? It is supported through November 14, 2028. |
| Persistence | [Npgsql EF Core 10 release notes](https://www.npgsql.org/efcore/release-notes/10.0.html) | Which provider generation fits EF Core 10? |
| PostgreSQL setup | [PostgreSQL version policy](https://www.postgresql.org/support/versioning/) | Is my installed major version still supported? |
| Authentication | [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-10.0) | Which account-management responsibilities can the framework handle? |
| Concurrency | [PostgreSQL row locks](https://www.postgresql.org/docs/current/explicit-locking.html) | How do simultaneous writes coordinate without losing money? |
| API retries | [Stripe idempotent requests](https://docs.stripe.com/api/idempotent_requests) | Why must a retried external request reuse its operation key? |
| Provider events | [Stripe webhooks](https://docs.stripe.com/webhooks) | Why verify signatures and expect duplicates or reordered events? |
| Settlement | [Payout reconciliation](https://docs.stripe.com/payouts/reconciliation) | Which transactions make up a payout? |
| Bank matching | [Bank reconciliation](https://docs.stripe.com/bank-reconciliation) | Why compare a provider payout with an independent bank deposit? |
| SaaS billing | [Subscription lifecycle](https://docs.stripe.com/billing/subscriptions/overview) | How do renewals, failed payments, and application access relate? |
| Repository license | [MIT License](https://choosealicense.com/licenses/mit/) | Which permissions, conditions, and limitations apply? |
| GitHub publication | [Create a repository in Visual Studio](https://learn.microsoft.com/en-us/visualstudio/version-control/git-create-repository) | How do we publish the existing repository root? |

Stripe is a reference for realistic asynchronous behavior, not a required account, SDK, or live integration. The local simulator must document its own smaller contract. Recheck current provider/country requirements before any later real integration.

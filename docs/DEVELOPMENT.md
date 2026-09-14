# Development Guide

## Local setup, when roadmap step 04 begins

1. Use **Visual Studio Community 2026** with **ASP.NET and web development**, Git, and the .NET 10 SDK. Follow the [Visual Studio setup guide](VISUAL-STUDIO-SETUP.md) for solution creation, physical paths, project references, and the first verification checkpoint.
2. In **pgAdmin**, inspect the existing Windows PostgreSQL server: run `SELECT version();` and confirm host/port. Do not assume the default port or replace the installation. Check that its major version is supported.
3. Create dedicated `reclevia_dev` and `reclevia_test` databases with project-specific roles. Use a separate migration owner and a runtime role with only needed data privileges. The runtime role must not be a superuser or schema owner.
4. Store `ConnectionStrings:Reclevia` and simulator signing secrets in **.NET User Secrets**; use protected environment configuration for deployments. Commit only placeholders. Never put credentials in tracked settings, screenshots, or fixtures.
5. At steps 05–06, create the planned solution, configure local HTTPS, run the API, apply a reviewed migration to the dedicated database, and inspect tables with pgAdmin. Record exact successful commands/URLs in this guide once the application exists.
6. Use a separate test database connection. Reset only that database's test data; guard against accidentally using a development/production connection. CI uses a disposable PostgreSQL service of the verified local major version.

There is no runnable application yet; setup/run commands dependent on future files are intentionally added after verification. Docker is introduced for packaging/CI, not as a replacement for your local database.

## One feature at a time

Write the business rule and one worked example before code. Put intrinsic rules in Core, workflow coordination in a handler, and HTTP translation in the endpoint. Keep methods small and names explicit. Learn the generated SQL and transaction boundary instead of hiding them behind additional layers.

Keep implementation status accurate: specifications describe intended behavior; passing verification establishes implemented behavior.

## Minimum verification set

| Scenario | Required result |
| --- | --- |
| EUR 1,000 invoice; payments 400 + 600; fee 20 | Outstanding 0; bank 980; clearing 0 after bank match. |
| Two tenants request the same record ID | Only the owning tenant can access it; writes and exports are isolated. |
| Same request/key twice; changed payload under same key | Same saved response; changed payload gets 409. |
| Same provider event repeated or events reordered | One financial effect; completed state does not regress. |
| Concurrent partial collections exceed remaining debt | Allocate only outstanding; recognize excess as a customer liability and open an exception. |
| Provider timeout followed by success; process restarts | No false failure or duplicate collection; durable work resumes. |
| Unpaid, paid, and partially paid credit notes | Correct receivable reduction/refund liability and balanced journals. |
| Concurrent refunds and a failed refund | Captured amount never exceeded; failed payout leaves the customer liability intact. |
| Refund after provider payout | Negative clearing remains visible until a later offset or bank debit. |
| Reimported/malformed/ambiguous bank rows | No duplicate journal; reject invalid input or open an explained exception. |
| Duplicate renewal; expired grace period | One billing effect; correct feature restriction; financial recovery continues. |
| Restore backup into a separate database | Balances, source references, and pending jobs survive restoration. |

Also test checked money arithmetic, impossible journal entries, authorization/CSRF, database constraints, and UTC/time-zone boundaries. Use a controllable clock; avoid sleep-based assertions. Use real PostgreSQL for transaction/concurrency tests rather than EF's in-memory provider.

## Feature completion checklist

- [ ] Business rule and vocabulary are understandable without reading code.
- [ ] Success, meaningful failure, permissions, and duplicate/concurrency cases pass.
- [ ] Financial changes preserve the ledger rules and atomic transaction boundary.
- [ ] Migrations, API documentation, logs, and relevant examples reflect the feature.
- [ ] Code is formatted, CI passes, and the commit explains its business purpose.

For import/export, bound file size and row count; validate currency/reference fields and neutralize spreadsheet-formula cells in text exports. Log IDs and outcomes rather than sensitive payloads. Store only necessary synthetic fixture data.

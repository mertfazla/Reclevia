# Reclevia

**B2B receivables and payment reconciliation.**

Reclevia brings invoices, collections, provider fees, and bank settlements into a single financial workflow. It is designed to help finance teams track outstanding balances, explain differences, and trace each amount back to its source.

**Status:** pre-implementation. This repository currently contains the product specification, architecture, and implementation roadmap.

## Scope

- **Receivables:** business customers, invoices, payment terms, partial payments, and credit notes.
- **Payments:** payment attempts, confirmations, refunds, and repeat-safe processing.
- **Financial records:** a double-entry subledger with immutable journals and traceable corrections.
- **Reconciliation:** provider settlement imports, fee tracking, bank matching, and exception resolution.
- **Tenant administration:** organization isolation, role-based access, audit history, and subscription entitlements.

The initial release will use EUR, a simulated payment provider, and imported bank statements. Live fund transfers, tax compliance, and foreign exchange are outside its scope.

## Architecture

A modular monolith built with **C# and .NET 10**, **ASP.NET Core**, **EF Core**, and **PostgreSQL**. Business rules are separated from HTTP and persistence concerns. Financial changes share a database transaction; durable inbox/outbox processing handles asynchronous provider interactions.

Supporting components: ASP.NET Core Identity, OpenAPI/Scalar, Razor Pages, xUnit, OpenTelemetry, and GitHub Actions.

## Documentation

| Document | Contents |
| --- | --- |
| [Domain](docs/DOMAIN.md) | Business terminology, accounting examples, and financial rules. |
| [Architecture](docs/ARCHITECTURE.md) | Technical decisions, repository structure, and API conventions. |
| [Diagrams](docs/DIAGRAMS.md) | System context, data relationships, and transaction lifecycles. |
| [Roadmap](ROADMAP.md) | Ordered implementation milestones and acceptance criteria. |
| [Visual Studio setup](docs/VISUAL-STUDIO-SETUP.md) | Solution creation in Visual Studio Community 2026. |
| [Development](docs/DEVELOPMENT.md) | PostgreSQL configuration, verification, and contribution standards. |
| [Repository workflow](docs/GITHUB.md) | GitHub setup, CI, and release requirements. |
| [References](docs/SOURCES.md) | Official technical and domain documentation. |

## License

[MIT](LICENSE) — Copyright (c) 2026 Mert Fazla.

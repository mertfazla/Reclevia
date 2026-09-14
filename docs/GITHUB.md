# Repository Workflow

**Repository:** `reclevia`  
**Description:** B2B receivables and payment reconciliation with .NET and PostgreSQL.  
**Topics:** `dotnet`, `csharp`, `postgresql`, `fintech`, `b2b-saas`, `reconciliation`, `modular-monolith`

## Initial publication

1. Keep `README.md`, `ROADMAP.md`, `LICENSE`, and `docs/` at the repository root. If starting before the solution exists, use **File > Open > Folder** in Visual Studio; later follow the setup guide and keep one repository root.
2. Add `.gitignore`: exclude `bin/`, `obj/`, `.vs/`, `.env`, local overrides, test results, coverage, and database backups.
3. Select **Git > Create Git Repository**. Choose GitHub, set the repository name to `reclevia`, confirm the local root, and select visibility. Inspect the files before **Create and Push**. If Git already exists, use **Git Changes** and the existing remote rather than initializing again.
4. Use `main` as the default branch. Verify README links, the recognized MIT license, and Mermaid rendering on GitHub.
5. Create ordered issues from roadmap steps: Foundation (01–08), Receivables (09–11), Payments (12–15), Reconciliation (16–18), and Delivery (19–24).

The documentation package does not publish a repository. If the solution is created first, publish this same root afterward; do not create a second nested repository.

## Changes and CI

Use a short branch per feature, such as `feat/issue-invoice`. Keep commits focused. A PR explains the business problem, resulting behavior, and verification; link its roadmap issue.

At step 06, add restore/build, unit and integration tests, and migration checks against a clean PostgreSQL service. Add formatting and packaging checks as those capabilities appear. Keep deployment credentials in protected secrets. Enable available dependency alerts and branch checks.

## Release requirements

- README status, setup instructions, diagrams, and examples match implemented behavior.
- Checks pass; fixtures are synthetic; tracked files and history contain no secrets or personal data.
- Preserve the root MIT license and applicable third-party notices. Add contribution and security-reporting guidance before accepting external contributions.
- Include a reproducible demo, release notes, limitations, and a `v1.0.0` tag.

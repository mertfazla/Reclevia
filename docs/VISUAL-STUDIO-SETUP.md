# Visual Studio Community 2026 Setup

This guide expands roadmap steps 04–05. Complete it once; do not repeat the same setup when returning to the roadmap. Menu wording can vary slightly by Visual Studio update.

## 1. Prepare the tools

1. Open **Visual Studio Installer**, find **Community 2026**, and select **Modify**.
2. Ensure **ASP.NET and web development** and the **.NET 10 SDK** are installed. Skip installation if they are already present.
3. Open Visual Studio. The project framework selector must offer **.NET 10.0**. Use the stable SDK, not a preview.

## 2. Create the solution and physical folders

4. Select **Create a new project**, search **Blank Solution**, and select it.
5. Set **Solution name** to `Reclevia`; choose the parent directory where the repository should live. Select **Create**.
6. Open the solution directory in File Explorer. This directory is `<repo>` below. It should contain `Reclevia.slnx` or `Reclevia.sln`; keep the format Visual Studio creates.
7. Copy `README.md`, `ROADMAP.md`, `LICENSE`, and `docs/` from this package into `<repo>`. Create physical `src/` and `tests/` directories there. If the documentation repository already exists, keep its `.git` directory at this root and check for accidental nesting before continuing.

## 3. Add the projects

8. Right-click the solution -> **Add > New Project**. Add the projects below. For each wizard, use the indicated **Location** as the parent directory; verify that Visual Studio appends the project name only once.

| Template | Project name | Location | Framework |
| --- | --- | --- | --- |
| ASP.NET Core Web API | `Reclevia.Api` | `<repo>\src` | .NET 10.0 |
| Class Library (C#) | `Reclevia.Core` | `<repo>\src` | .NET 10.0 |
| xUnit Test Project (C#) | `Reclevia.UnitTests` | `<repo>\tests` | .NET 10.0 |
| xUnit Test Project (C#) | `Reclevia.IntegrationTests` | `<repo>\tests` | .NET 10.0 |

For the Web API's **Additional information**, select:

| Setting | Value |
| --- | --- |
| Authentication type | None; Identity is introduced at roadmap step 07. |
| Configure for HTTPS | On |
| Enable OpenAPI support | On |
| Use controllers | On |
| Enable container support | Off |
| Aspire orchestration, if offered | Off |
| Do not use top-level statements, if offered | Leave unchecked |

Choose **Class Library**, not **Class Library (.NET Framework)**. If xUnit is unavailable, finish Api/Core first; add the xUnit template/test SDK before completing this checkpoint. Do not substitute a different framework silently.

9. Optionally create `src` and `tests` **Solution Folders** and drag the projects into them. These are visual groups; they do not create or move physical directories.

## 4. Add project references

10. For each row below, right-click the source project's **Dependencies > Add Project Reference**, select the target project, and confirm.

| Source | Reference |
| --- | --- |
| Reclevia.Api | Reclevia.Core |
| Reclevia.UnitTests | Reclevia.Core |
| Reclevia.IntegrationTests | Reclevia.Api |

Core must have no reference to Api. Test projects are never referenced by production projects.

## 5. Verify the scaffold

11. Right-click **Reclevia.Api > Set as Startup Project**. Choose its **https** launch profile.
12. Select **Build > Build Solution**. Run with **Ctrl+F5** and trust the local development certificate when prompted.
13. Using the actual HTTPS port from the output, open `/openapi/v1.json`. It should return the generated API document. If the template includes `/weatherforecast`, verify that endpoint too. A 404 at `/` is normal before a home page exists; OpenAPI support does not automatically install Scalar or Swagger UI.
14. Open **Test > Test Explorer** and run the generated tests. Template tests only verify test discovery; replace them with meaningful checks as features are added.
15. Check the physical layout:

```text
Reclevia/
|-- Reclevia.slnx                 # Or Reclevia.sln
|-- README.md
|-- ROADMAP.md
|-- LICENSE
|-- docs/
|-- src/
|   |-- Reclevia.Api/Reclevia.Api.csproj
|   `-- Reclevia.Core/Reclevia.Core.csproj
`-- tests/
    |-- Reclevia.UnitTests/Reclevia.UnitTests.csproj
    `-- Reclevia.IntegrationTests/Reclevia.IntegrationTests.csproj
```

## 6. Finish roadmap step 05

16. Add `.gitignore` and `.editorconfig` at the repository root. Ignore build output, `.vs/`, credentials, local overrides, and test results.
17. From **View > Terminal**, inspect `dotnet --version` at the solution root and pin that installed .NET 10 SDK in `global.json`. Centralize compatible package versions in `Directory.Packages.props`; migrate existing project package versions consistently before rebuilding.
18. Add a `/health` endpoint and a meaningful HTTP smoke test. Configure `Microsoft.AspNetCore.Mvc.Testing` in IntegrationTests when introducing `WebApplicationFactory`. Remove generated WeatherForecast/Class1/template-test files after replacements pass.
19. Rebuild, run the meaningful tests, and commit the scaffold through **Git Changes**. Follow GITHUB if the repository has not been initialized yet.

**Checkpoint:** a correctly located four-project solution, valid dependency direction, SDK/package configuration, a running HTTPS API, and passing smoke verification. Business features and database migrations begin afterward. Scalar setup can follow once OpenAPI is verified.

**Next:** roadmap step 06 adds EF Core/Npgsql, reviewed migrations, dedicated PostgreSQL databases/roles, and CI. Do not create tables manually before defining the first model and migration.

Sources: [Visual Studio solutions](https://learn.microsoft.com/en-us/visualstudio/ide/creating-solutions-and-projects), [ASP.NET Core Web API setup](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-10.0), and [Visual Studio 2026 release notes](https://learn.microsoft.com/en-us/visualstudio/releases/2026/release-notes). Project names, paths, and architecture settings above are repository decisions.

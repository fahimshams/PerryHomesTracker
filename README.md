# PerryHomesTracker

ASP.NET Core MVC application for tracking Perry Homes inventory: property addresses, pipeline **stages**, **people** (primary contacts), and optional **purchase** details (one record per home).

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** for local development (including [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) or LocalDB). Integration and smoke tests do not require a shared database for the default test paths.

## Quick start

1. Clone the repository and open a terminal at the solution root.

2. Configure the connection string. The committed `appsettings.json` leaves `ConnectionStrings:DefaultConnection` empty. Use **User Secrets** (the web project defines a `UserSecretsId`), `appsettings.Development.json` (not committed if it contains secrets), or environment variables:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PerryHomesTracker;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
   }
   ```

3. Restore, build, and run the web app:

   ```bash
   dotnet restore PerryHomesTracker.sln
   dotnet build PerryHomesTracker.sln
   dotnet run --project PerryHomesTracker.csproj
   ```

   With the default **http** profile, the site listens on [http://localhost:5138](http://localhost:5138).

In **Development**, the app applies pending EF Core migrations on startup (`Database.Migrate()`), so the database is created or updated automatically when SQL Server is reachable.

## Tests

Run the full test project:

```bash
dotnet test Tests/PerryHomesTracker.Tests/PerryHomesTracker.Tests.csproj
```

- **Unit** tests live under `Tests/PerryHomesTracker.Tests/Unit/` and use mocks.
- **Integration** tests use `WebApplicationFactory` with the `Testing` environment and an in-memory database.
- **Smoke** tests under `Tests/PerryHomesTracker.Tests/Smoke/` expect configuration in `appsettings.Smoke.json` (copied to test output) for a deployed or local base URL.

## Solution layout

| Path | Description |
|------|-------------|
| `PerryHomesTracker.csproj` | Web application (MVC, Razor, EF Core). |
| `Data/` | `PerryHomesDbContext`, `IPerryHomesDbContext`, and relationship configuration. |
| `Models/` | `Home`, `Stage`, `Person`, `PurchaseInfo`. |
| `Controllers/` | CRUD for homes, stages, people, and purchase info; `HomeController` for site pages. |
| `Migrations/` | EF Core SQL Server migrations. |
| `Tests/PerryHomesTracker.Tests/` | xUnit, Moq, MVC testing packages. |
| `infra/` | Terraform configuration for Azure Linux App Service (dev / staging / production). |
| `Tools/` | Auxiliary console projects (not referenced by the main web app compile). |

For deeper architectural notes (conventions, delete rules, DI details), see [Claude.md](Claude.md).

## Azure infrastructure

Terraform files are under `infra/`. From that directory, after Azure CLI login and provider init:

```bash
terraform init
terraform plan
terraform apply
```

Defaults target an existing resource group and provision Linux App Service plans and .NET 8 web apps for dev, staging, and production. See `infra/variables.tf` for overrides.

## Entity Framework migrations (reference)

Install the EF CLI once if you do not have it: `dotnet tool install --global dotnet-ef`.

If you add model changes and need a new migration from the repository root:

```bash
dotnet ef migrations add YourMigrationName --project PerryHomesTracker.csproj --startup-project PerryHomesTracker.csproj
```

Ensure `DefaultConnection` points at a valid SQL Server instance when generating migrations.

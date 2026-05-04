# PerryHomesTracker — context for Claude

## Purpose

ASP.NET Core MVC app to track **Perry Homes** properties: addresses, pipeline **stages**, **people** (primary contacts), and optional **purchase** details (one row per home).

## Stack

- **.NET 8** (`net8.0`), nullable reference types, implicit usings
- **ASP.NET Core MVC** — conventional routing `{controller=Home}/{action=Index}/{id?}`
- **Entity Framework Core 8** — SQL Server in normal runs; **InMemory** when environment is `Testing`
- **SQL Server** — connection string `ConnectionStrings:DefaultConnection` (empty in committed `appsettings.json`; use User Secrets / environment / `appsettings.Development.json` locally)
- **Bootstrap + Razor** — CRUD views with tag helpers and validation

## Repository layout

| Area | Path | Notes |
|------|------|--------|
| Web app | `PerryHomesTracker.csproj` | Excludes `Tests/**` and `Tools/**` from compile |
| EF data | `Data/PerryHomesDbContext.cs`, `IPerryHomesDbContext.cs` | Relations configured in `OnModelCreating` |
| Migrations | `Migrations/` | EF Core migrations for SQL Server |
| Tests | `Tests/PerryHomesTracker.Tests/` | xUnit, Moq, `WebApplicationFactory`, MockQueryable |
| Infra | `infra/` | Terraform (`azurerm`) — Linux App Service plans/apps for dev / staging / production |
| Tools | `Tools/PerryHomesMcp/`, `Tools/PrReviewAgent/` | Separate small utilities; not part of the main web compile |

## Domain model

- **Stage** — name, description, sort order; has many **Homes**. Delete blocked if any homes reference the stage (redirect back to delete view with `TempData` error).
- **Home** — address (line1/2, city, state, zip), optional community/plan names, required **StageId**, optional **PrimaryContactId**; optional 1:1 **PurchaseInfo**.
- **Person** — contact fields (name, email, phone, role); can be primary contact on many homes.
- **PurchaseInfo** — contract/closing dates, purchase price, notes; **HomeId** unique (one purchase record per home).

**EF delete behaviors (in `PerryHomesDbContext`):** Home → Stage **Restrict**; Home → Primary contact **SetNull**; PurchaseInfo → Home **Cascade**; unique index on `PurchaseInfo.HomeId`.

## Controllers

| Controller | Role |
|------------|------|
| `HomeController` | Site home, privacy, error (no DB) |
| `HomesController` | Home CRUD; `[Bind(...)]` allow-lists; lookups via `ViewBag` for stages and people |
| `StagesController` | Stage CRUD + guarded delete |
| `PeopleController` | Person CRUD |
| `PurchaseInfosController` | PurchaseInfo CRUD; homes without purchase info available for create |

**Conventions:** Async EF (`*Async`, `SaveChangesAsync`). POST actions use `[ValidateAntiForgeryToken]`. Details/Edit/Delete: missing id or entity → `NotFound()`. `HomesController` and `StagesController` depend on **`IPerryHomesDbContext`**; `PeopleController` and `PurchaseInfosController` inject **`PerryHomesDbContext`** directly.

## Startup (`Program.cs`)

- Registers `PerryHomesDbContext` + scoped `IPerryHomesDbContext`.
- **Development:** runs `Database.Migrate()` on startup.
- **Testing:** unique InMemory database name per run (see factory below).
- Non-development uses exception handler to `/Home/Error`.

## Tests

- **Unit** — `Tests/PerryHomesTracker.Tests/Unit/` — controllers with mocked `IPerryHomesDbContext` / Moq + MockQueryable.
- **Integration** — `Tests/PerryHomesTracker.Tests/Integration/` — `PerryHomesWebApplicationFactory` sets `Testing` environment; antiforgery helpers where needed.
- **Smoke** — `Tests/PerryHomesTracker.Tests/Smoke/` — HTTP checks against a configured base URL (`appsettings.Smoke.json` copied to output).

**Naming (project rule):** test methods use underscores, pattern `Method_ControllerName_[Scenario]_ExpectedResult` with concrete outcomes (status codes, result types, redirects).

## AI / editor rules

- `.cursor/rules/perry-homes.mdc` — stack and MVC/EF/view conventions for this app.
- `.cursor/rules/testing.mdc` — test naming and minimum coverage expectations for new controllers.

## Infrastructure (Terraform)

- Existing resource group (default name `mtc-resources`); optional `location` variable, else RG’s region.
- Three Linux App Service plans + `.NET 8` Linux web apps: **dev** (F1), **staging** (B1), **production** (B1); name prefix variable default `perry-homes`.

---

When changing persistence or URLs, keep migrations, connection strings, and Terraform names in sync with deployment targets.

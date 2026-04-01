# Expense Tracker — API (`expense-app-backend`)

ASP.NET Core HTTP API for the sibling **expense-app** Flutter repository (clone separately). Hosts **Swagger UI** for exploration and will expose **sync** + **Entra**-validated routes in later phases.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (see `ExpenseTracker.Api.csproj` target framework).
- Optional: **Docker** for **SQL Server** when schema work starts (Phase 5.3 in the Flutter repo docs).

## Build and run

From this repository root:

```bash
dotnet build ExpenseTracker.sln -c Release
dotnet run --project ExpenseTracker.Api
```

- **Swagger UI:** open **http://localhost:5057/swagger** (HTTP profile; see `Properties/launchSettings.json` for ports).
- **OpenAPI JSON:** `http://localhost:5057/swagger/v1/swagger.json`

### Try from the terminal

```bash
curl http://localhost:5057/api/hello
curl http://localhost:5057/api/health
```

## Endpoints (v0)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/hello` | Sample JSON `{ "message": "Hello, world!" }` |
| GET | `/api/health` | Liveness JSON for probes |

**CORS** is permissive for **local development** only; restrict before any shared or production deployment.

## Flutter client

In the **expense-app** repo, point the app at this base URL (no trailing slash):

```bash
flutter run -d chrome --dart-define=AZURE_API_BASE_URL=http://localhost:5057
```

See `lib/application/cloud_backend_env.dart` in **expense-app**.

## Layout

- `ExpenseTracker.sln` — solution
- `ExpenseTracker.Api/` — web host (Kestrel), Swagger, minimal APIs

## Optional: SQL Server locally (Phase 5.3+)

No database is required for the **v0** health/hello endpoints. For **schema work** and sync APIs, use **SQL Server** on the same machine (Azure SQL–compatible T-SQL).

**Docker (example)** — pick your own strong `MSSQL_SA_PASSWORD`; do not commit it:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong!Passw0rd>" -p 1433:1433 --name sql-expense -d mcr.microsoft.com/mssql/server:2022-latest
```

Create an empty database (e.g. `ExpenseTracker`) with SQL Server tools or Azure Data Studio. **Connection string:** keep it out of git. For ASP.NET Core, use [User secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) from `ExpenseTracker.Api`:

```bash
cd ExpenseTracker.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ExpenseTracker;User Id=sa;Password=<YourStrong!Passw0rd>;TrustServerCertificate=True"
```

**LocalDB** is an alternative on Windows; same rule — store the connection string in user secrets or a gitignored override, not in committed files.

## Docs (product / phases)

Phase checklist, **sync contract** (`05-sync-spec.md`), and Azure strategy live in the **expense-app** repo under `docs/` (e.g. `05-implementation-phase-5-plan.md`, `05-azure-hosting-strategy.md`). **Next backend milestone:** Phase **5.3** — versioned migrations and schema aligned with that spec, tested on local SQL Server **before** any billable **Azure SQL**.

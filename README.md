# Expense Tracker — API (`expense-app-backend`)

ASP.NET Core HTTP API for the sibling **expense-app** Flutter repository (clone separately). Hosts **Swagger UI** for exploration and will expose **sync** + **Entra**-validated routes in later phases.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (see `ExpenseTracker.Api.csproj` target framework).
- For **Phase 5.3+** (schema / sync): a local **SQL Server** engine. This project documents **SQL Server Express** on Windows as the default choice; **Docker** or **LocalDB** are alternatives (see below).

## Build and run

From this repository root:

```bash
dotnet build ExpenseTracker.sln -c Release
dotnet run --project ExpenseTracker.Api
```

### Configuration (secrets)

Loaded in order: `appsettings.json`, `appsettings.{Environment}.json`, optional **`appsettings.local.json`** (gitignored — copy from `ExpenseTracker.Api/appsettings.local.example.json`), ASP.NET Core **user secrets** (Development), then **environment variables** (nested keys use `__`, e.g. `ConnectionStrings__DefaultConnection`, `DevData__ExposeEndpoints`, `DevData__SharedSecret`).

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

See `lib/application/cloud_backend_env.dart`. For **Phase 5.b** dev-only book helpers (`reset`, `seed-taxonomy`, `seed-demo`), use `lib/data/remote/dev_backend_api_client.dart`. If the API sets `DevData:RequireSharedSecret`, pass the same value from Flutter:

```bash
flutter run --dart-define=AZURE_API_BASE_URL=http://localhost:5057 --dart-define=DEV_DATA_SECRET=your-secret
```

## Dev-only book endpoints (Phase 5.b)

Enabled when `DevData:ExposeEndpoints` is **true** (default in `appsettings.Development.json`). All accept JSON `{ "userId": "<tenant id>" }`.

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/dev/books/reset` | Delete all book rows for the user |
| POST | `/api/dev/books/seed-taxonomy` | Insert default expense + income taxonomy |
| POST | `/api/dev/books/seed-demo` | Reset, then taxonomy + sample data |

Optional header `X-Dev-Data-Secret` when `DevData:RequireSharedSecret` and `DevData:SharedSecret` are set.

## Layout

- `ExpenseTracker.sln` — solution
- `ExpenseTracker.Api/` — web host (Kestrel), Swagger, minimal APIs, **EF Core** startup (create database + migrate)
- `ExpenseTracker.Infrastructure/` — **EF Core** `DbContext`, entities, migrations, taxonomy/demo seed services (Phase **5.b**)

## Local database (Phase 5.3+)

No database is required for the **v0** health/hello endpoints. For **schema work** and sync APIs, use **SQL Server** on the same machine. T-SQL is **Azure SQL–compatible** enough for local development before you provision **Azure SQL** in the cloud.

**Chosen setup for this product (Windows):** [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) (e.g. 2025 Express via the web installer `SQL*-SSEI-Expr.exe`). **Developer edition** is also fine for non-production dev if you prefer a fuller feature set.

### Step-by-step: SQL Server Express + empty database + API connection

1. **Install Express**  
   Run the downloaded installer. Use **Basic** or **Custom** and note the **instance name**. The default named instance is often `SQLEXPRESS`, so the server address is `localhost\SQLEXPRESS` (or `.\SQLEXPRESS`). A **default** instance would be `localhost` only.

2. **Authentication**  
   Prefer **Windows Authentication** for local dev (simplest). If you enable **mixed mode** and use `sa`, use a strong password and never commit it.

3. **Optional GUI**  
   Install [SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms) or [Azure Data Studio](https://learn.microsoft.com/sql/azure-data-studio/download-azure-data-studio) to run SQL and inspect objects.

4. **Start the SQL Server service**  
   `services.msc` → ensure **SQL Server (SQLEXPRESS)** (or your instance name) is **Running**.

5. **Database creation (optional)**  
   If `ConnectionStrings:DefaultConnection` is set, the API **creates the database** on the server when it is missing and applies **EF Core** migrations on startup. You can still create `ExpenseTracker` manually in SSMS if you prefer.

6. **Connection string (Windows auth — typical for Express on same PC)**  
   Replace the instance name if yours is not `SQLEXPRESS`:

   ```text
   Server=localhost\SQLEXPRESS;Database=ExpenseTracker;Trusted_Connection=True;TrustServerCertificate=True
   ```

   If you use **SQL login** instead:

   ```text
   Server=localhost\SQLEXPRESS;Database=ExpenseTracker;User Id=sa;Password=<your-password>;TrustServerCertificate=True
   ```

7. **Store the connection string for the API (not in git)**  
   Either copy `appsettings.local.example.json` to **`appsettings.local.json`** in `ExpenseTracker.Api/` and edit `ConnectionStrings:DefaultConnection`, **or** use user secrets from `ExpenseTracker.Api`:

   ```bash
   cd ExpenseTracker.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=ExpenseTracker;Trusted_Connection=True;TrustServerCertificate=True"
   ```

   Adjust the value to match step 6. See [ASP.NET Core user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).

8. **Run the API**  
   `dotnet run --project ExpenseTracker.Api` applies pending **EF Core** migrations after ensuring the catalog exists. No separate migrate console is required for local dev.

9. **New migrations (schema changes)**  
   From the repo root (with the same startup project for design-time):

   ```bash
   dotnet ef migrations add <Name> --project ExpenseTracker.Infrastructure --startup-project ExpenseTracker.Api --output-dir Data/Migrations
   ```

10. **Firewall**  
   For **localhost-only** access, extra firewall rules are usually unnecessary. If you connect from another machine or container, open TCP **1433** (or the port configured for your instance) as required.

### Alternatives

**Docker (SQL Server Linux container)** — pick your own strong `MSSQL_SA_PASSWORD`; do not commit it:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong!Passw0rd>" -p 1433:1433 --name sql-expense -d mcr.microsoft.com/mssql/server:2022-latest
```

Example user secret for Docker SA login:

```bash
cd ExpenseTracker.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ExpenseTracker;User Id=sa;Password=<YourStrong!Passw0rd>;TrustServerCertificate=True"
```

The API can create `ExpenseTracker` automatically when the connection string points at that catalog name.

**LocalDB** — lightweight on Windows; connection string like `Server=(localdb)\\MSSQLLocalDB;Database=ExpenseTracker;Trusted_Connection=True;TrustServerCertificate=True`. Same rule: **secrets in user secrets**, not in committed files.

## Docs (product / phases)

Phase checklist, **sync contract** (`05-sync-spec.md`), and Azure strategy live in the **expense-app** repo under `docs/` (e.g. `05-implementation-phase-5-plan.md`, `05-azure-hosting-strategy.md`). **Next backend milestone:** Phase **5.4** — sync **REST** contract and API implementation against this schema **before** any billable **Azure SQL**.

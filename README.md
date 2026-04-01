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

## Docs (product / phases)

Phase checklist and Azure strategy live in the **expense-app** repo under `docs/` (e.g. `05-implementation-phase-5-plan.md`, `05-azure-hosting-strategy.md`).

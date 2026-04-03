# Backend testing strategy

This document defines how we test **expense-app-backend** and how new work should include tests.

## Stack

| Area | Technology |
|------|------------|
| Runtime | .NET 10 (`net10.0`) |
| Unit / integration runner | **xUnit** |
| Assertions | **FluentAssertions** |
| Isolation (unit) | **Moq** |
| HTTP + host | **Microsoft.AspNetCore.Mvc.Testing** (`WebApplicationFactory<Program>`) |
| Real SQL Server in tests | **Testcontainers.MsSql** |
| DB reset between tests | **Respawn** (truncate + reseed identity; ignores migrations history and seeded roles) |

**Out of scope for the current suite (add when needed):** Pact.NET (consumer contracts), SpecFlow/Gherkin, k6/NBomber load tests, BenchmarkDotNet (unless profiling a hot path).

## Project layout

| Project | Purpose |
|---------|---------|
| `tests/ExpenseTracker.UnitTests` | Fast tests: services, helpers, JWT options parsing, blocklist—**no** SQL, **no** full host. |
| `tests/ExpenseTracker.IntegrationTests` | SQL Server container + API factory: HTTP endpoints, Identity, DB persistence. |

Future optional projects (only if the product needs them): `ExpenseTracker.ContractTests`, `ExpenseTracker.FunctionalTests`.

## Traits (categories)

Use xUnit `[Trait("Category", "...")]` so CI and local runs can filter:

| Trait | When |
|-------|------|
| `Unit` | Pure unit tests |
| `Integration` | Any test that uses the integration fixture or Docker |
| `Integration.Database` | Direct DB / `UserManager` / `RoleManager` via DI |
| `Integration.Api` | HTTP calls through `HttpClient` |
| `E2E` | Longer user-journey style flows (still in IntegrationTests project today) |

**Naming:** `MethodName_Scenario_ExpectedBehavior` (e.g. `Register_WithInvalidEmail_ReturnsBadRequest`).

**Style:** Arrange–Act–Assert with blank lines; no `Thread.Sleep`; avoid conditional logic in tests.

## Integration fixture

- **`IntegrationHostFixture`** (`tests/ExpenseTracker.IntegrationTests/Fixtures/`): starts one **MSSQL** Testcontainer per test collection, builds **`ExpenseTrackerApiFactory`**, applies migrations on first client creation (same as production startup), configures **Respawn** with `TablesToIgnore`: `__EFMigrationsHistory`, `roles`, `role_claims` (seeded roles survive resets; user data is wiped).
- **`[Collection("Integration")]`** + **`DisableParallelization = true`**: one DB per run; avoids cross-test interference.
- Each test should call **`await host.ResetDatabaseAsync()`** at the start unless a future scenario explicitly needs accumulated state.

## Configuration

- Integration tests use environment **`Integration`** and in-memory configuration from **`ExpenseTrackerApiFactory`** (connection string from the container, long JWT signing key, `InitialAdmin:Enabled=false`, dev endpoints enabled where tests need them).
- Do **not** commit secrets; tests never rely on `appsettings.local.json`.

## Local commands

From repository root:

```bash
# Fast — no Docker
dotnet test tests/ExpenseTracker.UnitTests --filter "Category=Unit"

# Full integration — Docker must be running (Testcontainers pulls SQL Server image on first run)
dotnet test tests/ExpenseTracker.IntegrationTests --filter "Category=Integration"

# All tests in the solution
dotnet test ExpenseTracker.sln
```

## CI

See **`.github/workflows/tests.yml`**: unit job without Docker; integration job with Docker and Testcontainers.

## Definition of done for new features

1. **Unit tests** for new pure logic (validators, services, token helpers, mappers).
2. **Integration tests** for new HTTP routes (status codes, JSON shape, auth rules) and for non-trivial EF queries or transactions.
3. Traits and naming follow this document.
4. If a change alters **Respawn** assumptions (new tables that must survive reset, or custom schemas), update **`IntegrationHostFixture`** and this doc.

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
| DB reset between tests | **Respawn** + post-step (see below) |
| Coverage gate (CI) | **coverlet.msbuild** (merge unit + integration, line threshold) |

**Out of scope for the current suite (add when needed):** Pact.NET (consumer contracts), SpecFlow/Gherkin, k6/NBomber load tests, BenchmarkDotNet (unless profiling a hot path).

## Project layout

| Project | Purpose |
|---------|---------|
| `tests/ExpenseTracker.UnitTests` | Fast tests: JWT, blocklist, `DevBookDataService.ValidateUserId`, `DevBookRequestValidation`—**no** SQL, **no** full host. |
| `tests/ExpenseTracker.IntegrationTests` | SQL Server container + API factory: health/auth, **admin users** (`/api/users`, bootstrap), **DevBook** (`/api/dev/books/*`), Identity + DB checks. |

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

- **`IntegrationHostFixture`** (`tests/ExpenseTracker.IntegrationTests/Fixtures/`): starts one **MSSQL** Testcontainer per test collection, builds **`ExpenseTrackerApiFactory`**, applies migrations on first client creation (same as production startup), configures **Respawn** with `TablesToIgnore`: `__EFMigrationsHistory`, `roles`, `role_claims` (seeded roles survive resets).
- After **Respawn**, the fixture **removes all Identity users** so bootstrap / SuperAdmin tests start from a known state: for each user id, **`DevBookDataService.ResetUserBookAsync`** (clears FKs from book tables), then **`UserManager.DeleteAsync`**. Without the book reset, deletes can fail on FKs if Respawn ordering leaves rows pointing at `users`.
- **`[Collection("Integration")]`** + **`DisableParallelization = true`**: one DB per run; avoids cross-test interference.
- Each test should call **`await host.ResetDatabaseAsync()`** at the start unless a future scenario explicitly needs accumulated state.

## Configuration

- Integration tests use environment **`Integration`** and in-memory configuration from **`ExpenseTrackerApiFactory`** (connection string from the container, long JWT signing key, `InitialAdmin:Enabled=false`, **`Setup:BootstrapToken`** set to the value in **`IntegrationTestConstants`** for bootstrap tests, dev endpoints on, `DevData:RequireSharedSecret=false` by default).
- **`ExpenseTrackerApiFactory`** accepts optional **`configurationOverrides`** (e.g. `DevData:RequireSharedSecret` + `DevData:SharedSecret`) for scenarios like DevBook shared-secret checks.
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

### Coverage (optional local / same as CI)

Use an **absolute** `CoverletOutput` path so merge finds the first run’s file (coverlet resolves paths relative to the test project directory otherwise).

**PowerShell** (repository root):

```powershell
New-Item -ItemType Directory -Force -Path coverage | Out-Null
$cov = Join-Path (Get-Location) "coverage/coverage.json"
dotnet test tests/ExpenseTracker.UnitTests/ExpenseTracker.UnitTests.csproj -c Release --filter "Category=Unit" `
  /p:CollectCoverage=true /p:CoverletOutput="$cov" /p:CoverletOutputFormat=json /p:ExcludeByFile="**/Migrations/**"
dotnet test tests/ExpenseTracker.IntegrationTests/ExpenseTracker.IntegrationTests.csproj -c Release --filter "Category=Integration" `
  /p:CollectCoverage=true /p:MergeWith="$cov" /p:CoverletOutput="$cov" /p:CoverletOutputFormat=json `
  /p:ThresholdType=line /p:Threshold=82 /p:ExcludeByFile="**/Migrations/**"
```

CI runs the same sequence with **`${{ github.workspace }}/coverage/coverage.json`**. Migrations are excluded from the coverage denominator via **`ExcludeByFile`**. The current gate is **82%** merged **line** coverage (Api + Infrastructure).

## CI

See **`.github/workflows/tests.yml`**: single job (Docker on the runner), unit tests then integration tests, merged coverage with **line threshold 82%**.

## Definition of done for new features

1. **Unit tests** for new pure logic (validators, services, token helpers, mappers).
2. **Integration tests** for new HTTP routes (status codes, JSON shape, auth rules) and for non-trivial EF queries or transactions.
3. Traits and naming follow this document.
4. If a change alters **Respawn** or **post-reset** assumptions (new tables that must survive reset, or custom schemas), update **`IntegrationHostFixture`** and this doc.
5. If CI coverage falls below the threshold, add tests or raise the threshold only with team agreement.

---
description: Expense Tracker ASP.NET Core API — scope, stack, and boundaries vs Flutter repo
globs: "**/*.cs,**/*.csproj,**/*.sln,**/appsettings*.json,**/launchSettings.json,**/README.md"
alwaysApply: true
---

# Expense Tracker — Backend (this repository)

## Scope

- This workspace root is **`expense-app-backend`** only. The Flutter app lives in the separate **`expense-app`** repository.
- Do **not** assume Dart/Flutter sources exist here. Cross-repo links are documentation-only.

## Stack

- **C#**, **ASP.NET Core**, **minimal APIs**, **Swashbuckle** (Swagger + Swagger UI).
- **Azure-aligned:** later **Azure SQL**, **Microsoft Entra External ID** JWT validation, deploy to Azure App Service or Functions as chosen in product docs.

## Conventions

- **English** for code, identifiers, and API contracts.
- **Tests:** follow **`docs/TESTING.md`**; new features include **unit** and/or **integration** tests as appropriate (see **`.cursor/rules/expense-backend-testing.md`**).
- **No secrets** in source: connection strings and keys via **environment**, **user secrets**, or **Azure Key Vault** in hosted environments.
- Keep **CORS** locked down outside local development.
- Prefer **versioned SQL migrations** in this repo when the database layer is added.

## Product docs

Authoritative phase checklist and environment model: **`expense-app`** repo → `docs/05-implementation-phase-5-plan.md`, `docs/05-azure-hosting-strategy.md`, `docs/05-sync-spec.md`.

## Architecture detail

Layering, boundaries, performance/data rules, integrations, environments, and comment discipline: **`expense-backend-architecture.md`** (applies when working on `*.cs` / project files; `globs` in that file).

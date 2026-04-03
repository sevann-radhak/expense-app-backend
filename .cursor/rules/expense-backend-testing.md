---
description: Require tests alongside backend changes; points to TESTING.md and test project conventions
globs: "**/*.cs,**/tests/**/*"
alwaysApply: false
---

# Expense Tracker — Backend testing (agent / contributor checklist)

## When this applies

Whenever you add or change **API endpoints**, **EF models/migrations**, **auth**, **services**, or **host configuration** in **`expense-app-backend`**.

## Rules

1. Read **`docs/TESTING.md`** for traits, fixtures, and commands.
2. **Unit tests** (`tests/ExpenseTracker.UnitTests`): pure logic, mocks for `DbContext` / `HttpClient` / external services—no Docker.
3. **Integration tests** (`tests/ExpenseTracker.IntegrationTests`): use **`IntegrationHostFixture`** + `[Collection("Integration")]`, call **`ResetDatabaseAsync()`** at test start, assert HTTP and/or DB state.
4. Use **`[Trait("Category", "Unit")]`** or **`Integration` / `Integration.Api` / `Integration.Database` / `E2E`** as documented.
5. Prefer **`MethodName_Scenario_ExpectedBehavior`** and FluentAssertions **Arrange / Act / Assert** layout.
6. Do **not** add sleeps for timing; rely on async APIs and Testcontainers health.
7. If you add tables that must **not** be truncated on reset, update **`IntegrationHostFixture`** Respawn **`TablesToIgnore`** and **`docs/TESTING.md`**.

## PR expectation

The same change set should include tests that would fail if the new behavior regressed, unless the team explicitly defers (note in PR why).

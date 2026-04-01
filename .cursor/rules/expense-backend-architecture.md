---
description: Backend architecture — layers, boundaries, Azure SQL, Entra, performance/data rules, comments discipline
globs: "**/*.{cs,csproj,sln}"
alwaysApply: false
---

# Expense Tracker — Backend architecture (implementation)

**Repository:** `expense-app-backend`. **Product / sync contract:** sibling **`expense-app`** → `docs/PROJECT_MASTER_PLAN.md`, `docs/05-sync-spec.md`, `docs/05-implementation-phase-5-plan.md`, `docs/05-azure-hosting-strategy.md`.

### Related Cursor rules (map)

| Rule file | Use |
|-----------|-----|
| **`expense-backend-project.md`** | Repo scope, stack summary, links to Flutter docs. |
| **This file** | Architecture, layering, API/data/security/testing expectations for C# work. |
| **`expense-app`** `.cursor/rules/expense-app-architecture.md` | Flutter-side clean architecture; keep HTTP/JSON contracts aligned with sync spec. |

**Per-phase checklists** live in **`expense-app`** `docs/05-implementation-phase-5-plan.md` (and related phase docs).

---

## 1) Architecture reference

- **Source of truth** for *product* scope and sync semantics: **`expense-app`** docs above, especially **`05-sync-spec.md`**.
- **Source of truth** for *this* service shape: **this rule** + **`expense-backend-project.md`**, implemented consistently in code.
- If implementation **conflicts** with these rules or the sync spec, **call it out explicitly** (PR description or short note in repo docs)—do not silently drift.

---

## 2) Domain boundaries and ownership

- **Single bounded context (v1):** **Expense Tracker API** — authoritative **server copy of one book per authenticated user** (`user_id` from Entra JWT, not from the client body).
- **Single database** for this service: **Azure SQL** (one logical app DB). No second service mutating the same tables outside this API.
- **Cross-repo boundary:** the **Flutter** app is the client; integration is **HTTPS + JSON** only. No shared database with the mobile/web app.
- **v1 sync model:** **HTTP** request/response (snapshot-first per sync spec). **Event buses, outbox, and multi-service choreography** are **out of scope** until an explicit **ADR** and phase plan say otherwise.

---

## 3) Tech stack (baseline)

- **Runtime:** .NET (target framework in `ExpenseTracker.Api.csproj`; prefer current LTS or STS aligned with Azure hosting).
- **Database:** **Azure SQL**; access via one chosen stack (**EF Core**, **Dapper**, or ADO.NET) and **versioned migrations** in this repo.
- **Auth:** **Microsoft Entra External ID** (or agreed Entra app registration) — validate **JWT** (issuer, audience, lifetime); map **`oid` / `sub`** → internal **`user_id`**.
- **API surface:** ASP.NET Core **minimal APIs** (or controllers if introduced later); **Swagger / OpenAPI** (Swashbuckle) stays accurate for public routes.
- **Observability (hosted):** **Application Insights** + structured logs; traces for dependency calls when enabled.
- **Resilience (outbound HTTP only):** if you add **HttpClient** calls (e.g. metadata, future providers), use **timeouts**, bounded **retries**, and optional **Polly** policies—never unbounded waits.

---

## 3.1) Repositories and deliverables

| Repo | Deliverable |
|------|-------------|
| **`expense-app-backend` (this)** | API, OpenAPI, health/hello, SQL migrations, tests, container or publish profile as needed for Azure. |
| **`expense-app`** | Flutter web + mobile, Drift, `AZURE_API_BASE_URL`, MSAL when auth lands. |

---

## 4) Performance and payload expectations

Targets are **guidelines** for a **small user base**; tighten if usage grows.

- **List-style endpoints** (when added): **paginate** (sensible defaults, e.g. default **50**, max **200** per page); avoid unbounded scans.
- **Snapshot / full-book** endpoints: document expected **maximum payload** and prefer **compression** at the host if payloads grow; if a book can exceed **~1 MB**, define **chunking or versioning** in the API doc and sync spec—not ad-hoc truncation.
- **Primary keyed reads** (by `user_id` + id): aim for **sub-second** DB time under normal load; investigate if consistently worse.
- Avoid **correlated subqueries** and **N+1** patterns on paths that will run for every sync or list request; prefer explicit joins or batched queries.
- **Full-text / search** (if added): prefer prefix or indexed search over leading-`%` wildcards on large tables.

---

## 5) Data access rules

- **Parameterized SQL** only; never build SQL by concatenating user-controlled strings.
- **Pagination** mandatory for any **list** endpoint that can return many rows.
- **Stored procedures** optional for complex reporting later—not required for v1 CRUD/sync if kept simple in application code.
- Schema and **FK order** respect bootstrap order in **`expense-app`** `docs/05-sync-spec.md` §3.

---

## 6) Integration rules

- **Entra / OpenID** metadata and token validation: configure via framework packages; keys from metadata, not hardcoded secrets.
- **Future external APIs** (e.g. FX): isolate behind **small adapter** types; **timeouts + retries + circuit breaker** for outbound calls; **secrets** in Key Vault / env—not committed JSON.

---

## 7) Event-driven standards

- **Not applicable for v1** sync MVP (HTTP-only). If you introduce **async processing or messages**, document an **ADR**, align with **`expense-app`** phase plans, and add **idempotent consumers**, **outbox** (if used), and **correlation ids** as part of that design—do not add a message bus “by default.”

---

## 8) Environments and deployment

- **Names** (see **`expense-app`** `docs/05-azure-hosting-strategy.md`): **Local** (dev PC) → **Azure Dev** → **Azure Prod**.
- **CI/CD:** gate merges on **`dotnet build`** and **tests**; add security/analysis jobs when the pipeline exists; **no production deploy** of secrets from repo files.
- **Infrastructure:** prefer **IaC** (Bicep/Terraform) or documented repeatable steps before relying on manual portal-only prod setup.
- **Secrets:** **Azure Key Vault** or environment configuration in Azure; local **user secrets** for development.

---

## 9) API and schema evolution

- Prefer **incremental** schema migrations and **versioned** public API paths or contracts when breaking JSON (**e.g. `/api/v1/...`**).
- **Replace** old behavior with **documented** deprecation when possible; avoid undeclared breaking changes for the Flutter client.
- Large cutovers should include **rollback** and **monitoring** steps in the implementation plan or PR—not only code.

---

## 10) Layering (clean / ports style)

Keep **dependencies pointing inward** (application/domain do not depend on `HttpContext` or concrete ADO types in core logic).

| Layer | Responsibility |
|-------|----------------|
| **API (host)** | Endpoints, Swagger, middleware, HTTP status → Problem Details. Thin. |
| **Application** | Use cases (get/put book, validation, orchestration); depends on abstractions. |
| **Domain** (optional / grow as needed) | Pure invariants: versioning, conflict helpers — no SQL types. |
| **Infrastructure** | Azure SQL, JWT wiring, repository implementations. |

**Pragmatic layout:** folders under `ExpenseTracker.Api` such as `Endpoints/`, `Application/`, `Infrastructure/`, `Domain/` until a separate project is justified.

### Product alignment (book model)

- **Dates:** business fields **calendar-only**; JSON **`YYYY-MM-DD`** where applicable.
- **Money:** **`decimal`** or integer minor units with documented scale—**not** `double` for money.
- **Cards:** **metadata only** — no PAN, CVV, PIN.
- **Tenancy:** all book data keyed by **`user_id`** from the token.

### HTTP conventions

- **Problem Details** (`RFC 7807`) and **stable error codes** for sync conflicts and validation.
- **Idempotency** for PUT/replace as specified in the sync plan.
- **CORS:** open only in local dev; **explicit origins** in production.

### Testing

- **Unit** tests for application/domain without DB.
- **Integration** tests with **local SQL** or **Testcontainers** + **`WebApplicationFactory`** when valuable.
- Always cover: **user A cannot read user B’s data**; **optimistic concurrency** if snapshot versioning is used.

### Cross-repo contract

- JSON shapes stay compatible with **`expense-app`** `BookBackupSnapshot` / DTOs; coordinate **breaking** changes across both repos and bump **contract version** fields when required.

---

## 11) Code comments and inline documentation

- Prefer **clear names and structure** over comments. No noise: no comments for obvious code, **no commented-out code**, no ticket or user-story IDs in source (use commits, PRs, and repo docs).
- Do **not** label integrations in comments with vague external names (“legacy API”, “old stack”); describe **behavior, invariants, or security/data cautions** when a comment is truly needed.
- Use comments **sparingly** for: non-obvious invariants, security or data-handling warnings, performance contracts, or compliance boundaries. **XML doc** on **public** API types is optional—favor **accurate OpenAPI** over duplicating Swagger in comments.
- Historical comparisons, migration narratives, and analysis belong in **repository documentation** (`README.md` or a future `docs/` folder in **this** repo), not scattered in application code.

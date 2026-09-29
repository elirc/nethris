# Key Flows

## Flow: Login And Current User

**Why it matters:** Authentication is the first trust boundary. It turns an email/password into a JWT whose claims drive API authorization and UI visibility.

**Open first:** [client/src/App.tsx](../../../client/src/App.tsx#L71-L87) - form submit and session setup.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | [App.tsx](../../../client/src/App.tsx#L71-L87) | Reads form data and calls `login`. | email/password strings | UI defaults are demo credentials. |
| 2 | Client API | [api.ts](../../../client/src/api.ts#L12-L35) | Posts to `/api/auth/login`; falls back to demo users on fetch failure. | `Session` | Possible risk: fallback can mask API outage during development. |
| 3 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83) | Loads active user including employee and verifies password. | `LoginRequest` -> `LoginResponse` | No lockout/rate limiting present. |
| 4 | Security | [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L21-L38) | Emits claims for user id, email, role, employee id, employee name. | JWT | Claims must stay aligned with authorization helpers. |

**Validation & authorization:** Password verification is in [PasswordService.cs](../../../server/Nethris.Api/Security/PasswordService.cs#L18-L30). Token validation parameters are configured in [Program.cs](../../../server/Nethris.Api/Program.cs#L32-L45).

**Persistence & side effects:** User lookup uses EF in [Program.cs](../../../server/Nethris.Api/Program.cs#L72-L74). No audit log is written for login.

**Tests:** No direct coverage found; no test project exists in the inventory.

**Juniors miss:** Authentication is not the same as authorization.

**Seniors notice:** The demo fallback in [api.ts](../../../client/src/api.ts#L24-L35) is useful for local exploration but should be explicitly disabled or surfaced in production builds.

**Drill:** Change the email to mixed case in your head and trace why API lookup still works.

**Self-grade:** Basic: can name files. Solid: can explain claims. Strong: can propose rate limiting, lockout, and test coverage.

## Flow: Directory Search And Profile

**Why it matters:** Read flows teach query boundaries, DTO shaping, and client-side derived state without side effects.

**Open first:** [client/src/App.tsx](../../../client/src/App.tsx#L43-L51) - client filtering.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Client API | [api.ts](../../../client/src/api.ts#L42-L49) | Loads employees with departments and roles in snapshot. | `Employee[]` | Multiple requests happen in parallel. |
| 2 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L117-L147) | Optional search and department filters are applied to `IQueryable`. | query -> `EmployeeResponse[]` | `ToLower().Contains` can limit index use. |
| 3 | EF | [Program.cs](../../../server/Nethris.Api/Program.cs#L119-L124) | Eager loads department, role, manager, direct reports. | entity graph | Includes can grow payload and query cost. |
| 4 | UI | [App.tsx](../../../client/src/App.tsx#L228-L246) | Renders rows and selected profile. | local `Snapshot` | Client filtering can diverge from API filtering rules. |
| 5 | UI | [App.tsx](../../../client/src/App.tsx#L413-L435) | Profile panel computes direct reports from loaded employees. | employee + employees | Depends on full employee list being present. |

**Validation & authorization:** Endpoint requires any authenticated user in [Program.cs](../../../server/Nethris.Api/Program.cs#L147-L147).

**Persistence & side effects:** Read-only EF query. Mapping helper is [Program.cs](../../../server/Nethris.Api/Program.cs#L412-L432).

**Tests:** No direct coverage found.

**Drill:** Add a trace row for a department filter and identify where it is enforced server-side vs client-side.

## Flow: Org Chart

**Why it matters:** Recursive structures teach ownership of hierarchy, cycle risk, and server-side shaping.

**Open first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L255-L270) - org chart endpoint.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L261) | Loads all employees with department and job role, no tracking. | flat employees | Unbounded query for large orgs. |
| 2 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L263-L267) | Finds roots where `ManagerId` is null. | roots | Orphaned/cyclic managers need tests. |
| 3 | API helper | [Program.cs](../../../server/Nethris.Api/Program.cs#L434-L449) | Recursively builds `OrgNodeResponse`. | tree | Cycle would recurse indefinitely. |
| 4 | UI | [App.tsx](../../../client/src/App.tsx#L250-L264) | Renders forest of org nodes. | `OrgNode[]` | Horizontal overflow handled by CSS. |
| 5 | UI helper | [App.tsx](../../../client/src/App.tsx#L438-L459) | Recursively renders reports. | tree | Deep orgs may be hard to navigate. |

**Validation & authorization:** Any authenticated user can load org chart in [Program.cs](../../../server/Nethris.Api/Program.cs#L270-L270).

**Persistence & side effects:** Manager relationship is configured with restrict delete in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L23-L26).

**Tests:** No direct coverage found.

**Drill:** Write the invariant "no employee can eventually manage themselves" and decide where to enforce it.

## Flow: PTO Request

**Why it matters:** This is the cleanest write flow from React form to EF persistence and audit log.

**Open first:** [client/src/App.tsx](../../../client/src/App.tsx#L89-L121) - submit handler.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | [App.tsx](../../../client/src/App.tsx#L275-L293) | Renders date, hours, and reason form. | form fields | Browser validation only catches basic required/min. |
| 2 | UI | [App.tsx](../../../client/src/App.tsx#L89-L121) | Builds payload, calls `createPto`, updates local snapshot. | `CreatePto` | Optimistic local audit entry can differ from API. |
| 3 | Client API | [api.ts](../../../client/src/api.ts#L54-L77) | Posts to `/api/pto` or creates demo request. | payload -> `PtoRequest` | Demo path bypasses server validation. |
| 4 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L295-L323) | Validates dates/hours, uses claim employee id, saves request and audit. | `CreatePtoRequest` -> `PtoResponse` | Does not check PTO balance. |
| 5 | EF | [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L3-L15) | Default status starts at manager review. | entity | Date overlap is not checked. |

**Validation & authorization:** Date and hours validation is [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303); authenticated user required in [Program.cs](../../../server/Nethris.Api/Program.cs#L323-L323).

**Persistence & side effects:** Request and audit save together in [Program.cs](../../../server/Nethris.Api/Program.cs#L317-L319).

**Tests:** No direct coverage found.

**Juniors miss:** The client does not decide who the request is for; the API uses `employeeId` from claims in [Program.cs](../../../server/Nethris.Api/Program.cs#L305-L309).

**Seniors notice:** No transaction is explicit, but one `SaveChangesAsync` will wrap changes in EF's default transaction for relational providers.

## Flow: PTO Approval

**Why it matters:** Approval is an authorization and state-machine exercise.

**Open first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385) - decision endpoint.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | [App.tsx](../../../client/src/App.tsx#L65-L69) | Builds approval queue by role and status. | `PtoRequest[]` | UI filtering is not a security boundary. |
| 2 | UI | [App.tsx](../../../client/src/App.tsx#L300-L324) | Renders approve/reject buttons. | queue item | No comment input yet. |
| 3 | Client API | [api.ts](../../../client/src/api.ts#L79-L88) | Posts decision to `/api/pto/{id}/decision`. | `DecidePtoRequest` | Demo path returns minimal shape. |
| 4 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L337-L353) | Rejects closed requests, checks manager owns direct report, blocks manager HR-stage approval. | request state + claims | Tests should freeze these rules. |
| 5 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L355-L381) | Adds approval, transitions status, deducts PTO on HR/Admin approval, audits. | `PtoApproval` | Balance can go negative. |

**Validation & authorization:** Policy allows Manager/Hr/Admin in [Program.cs](../../../server/Nethris.Api/Program.cs#L28-L30) and [Program.cs](../../../server/Nethris.Api/Program.cs#L385-L385); resource authorization is inside the endpoint at [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353).

**Persistence & side effects:** Approval relationship is configured in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L67-L71).

**Tests:** No direct coverage found.

**Drill:** Write a table with states `PendingManager`, `PendingHr`, `Approved`, `Rejected` and allowed actors.

## Flow: Audit Logs

**Why it matters:** Audit logs are an accountability surface. They are also a security and operations signal.

**Open first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L481-L490) - audit helper.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L196-L197) | Employee create writes audit with `Created`. | `AuditLog` | No old/new diff. |
| 2 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L248-L249) | Employee update writes audit with `Updated`. | `AuditLog` | No field-level details. |
| 3 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L318-L319) | PTO request writes audit. | `AuditLog` | No correlation id. |
| 4 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381) | PTO decision writes audit. | `AuditLog` | Audit save fails with main transaction. |
| 5 | UI | [App.tsx](../../../client/src/App.tsx#L327-L352) | HR/Admin renders audit list. | `AuditLog[]` | Mojibake at [App.tsx](../../../client/src/App.tsx#L344-L344). |

**Validation & authorization:** Audit endpoint requires PeopleOps in [Program.cs](../../../server/Nethris.Api/Program.cs#L387-L395).

**Persistence & side effects:** Audit entity fields are [AuditLog.cs](../../../server/Nethris.Api/Domain/AuditLog.cs#L3-L12).

**Tests:** No direct coverage found.

## Flow: Startup And Seed Data

**Why it matters:** Startup establishes dependency graph, config, migrations/creation behavior, and initial data.

**Open first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L12-L56) - service registration and seed call.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L14-L15) | Registers SQLite DbContext. | scoped DbContext | Connection string is local file. |
| 2 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L25-L30) | Registers token service and auth policies. | DI services | Policies are role-only. |
| 3 | API | [Program.cs](../../../server/Nethris.Api/Program.cs#L52-L56) | Creates scope and seeds database before serving. | service scope | Startup can fail if database setup fails. |
| 4 | Data | [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L15) | Ensures database exists and exits if employees exist. | SQLite DB | `EnsureCreatedAsync` conflicts with migrations later. |
| 5 | Data | [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L79-L85) | Saves entities, then sets department leads. | two saves | Two-phase save handles circular references. |

**Validation & authorization:** Not request-time auth; this is startup composition.

**Persistence & side effects:** Creates database file and seed rows.

**Tests:** No direct coverage found.

## Flow: Missing Background/Async Worker

**Why it matters:** The request asked for background/async learning. This repo has async request handlers but no `IHostedService`, `BackgroundService`, channel, queue, webhook, retry policy, or `IHttpClientFactory` usage found in inventory.

**Open first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L395) - async endpoint handlers.

**Trace:** There is no background flow to trace. Use this as a design kata: "send PTO approval notifications reliably." Anchor proposed integration points to [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381) and [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L17-L27).

**Drill:** Design an outbox table and hosted service. Label all code as illustrative fake code until implemented.


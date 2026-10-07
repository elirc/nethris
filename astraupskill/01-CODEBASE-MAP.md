# Read the system from the outside inward

The browser and API are two programs connected by HTTP. SQLite is the persisted source of truth. The packaged API serves both the static browser assets and the API routes from one loopback origin. During development, Vite serves the browser on port 5173 and proxies /api to the API on port 5187.

## The reading path

| File | Responsibility | Question to answer |
|---|---|---|
| [main.tsx](../client/src/main.tsx) | Mounts the active React application | Which component really runs? |
| [Workshop.tsx](../client/src/Workshop.tsx) | Screens, drafts, reference selectors, recovery states | What is stored locally and what is fetched? |
| [workshopApi.ts](../client/src/workshopApi.ts) | HTTP requests, operation snapshots, ETag construction | Which bytes must remain identical during a retry? |
| [Program.cs](../server/Nethris.Api/Program.cs) | Hosting, loopback boundaries, authentication, errors | Which middleware runs before a route? |
| [HrisApi.cs](../server/Nethris.Api/HrisApi.cs) | Dispatch, transaction ownership, receipts, projections | Where does commit happen? |
| [HrisAuth.cs](../server/Nethris.Api/Security/HrisAuth.cs) | Bootstrap, sessions, current roles, account administration | Why are role claims not trusted indefinitely? |
| [HrisPeople.cs](../server/Nethris.Api/Domain/HrisPeople.cs) | Employee and reference-record CRUD | Which relationships prevent deletion? |
| [HrisPto.cs](../server/Nethris.Api/Domain/HrisPto.cs) | Time-off validation and transitions | Who may perform each transition? |
| [HrisDbContext.cs](../server/Nethris.Api/Data/HrisDbContext.cs) | EF mappings, constraints and concurrency | How are decimals and timestamps stored? |
| [HrisDatabase.cs](../server/Nethris.Api/Data/HrisDatabase.cs) | Database identity and independent contexts | What happens if the file is unknown? |

Read [WorkshopContracts.cs](../server/Nethris.Api/Contracts/WorkshopContracts.cs) alongside the domain methods. An input DTO describes fields accepted from a client. A view DTO describes fields intentionally exposed to that client. An EF entity describes persisted state and navigation properties. Those are three different contracts, even when their property names overlap.

## Relationships to draw

An Employee belongs to one Department and one JobRole. Employee.ManagerId points to another Employee. Department.LeadEmployeeId also points to an Employee, with an extra domain rule requiring departmental membership. UserAccount references one Employee and carries its security role. AuthSession references UserAccount. PTO references its requester and the manager captured when the request was submitted. Each approval references the PTO request and its approver.

A plain foreign key guarantees that a referenced employee exists. It does not guarantee that the employee is the correct department lead, that a manager chain is acyclic, or that an approver has the right current role. Locate both the database constraints and the application checks before claiming a relationship is safe.

## Trace a directory read

The screen requests an employee page. Authentication validates the signed token, and HrisAuth.Require loads the matching unexpired server session plus the current user and employee. The query filters and orders employees before Skip and Take. After retrieval, the view mapper decides whether the caller may see each balance. The response includes items, total, offset and limit.

This sequence matters. Filtering only the first twelve rows in React would hide matches on later pages. Returning the entire EF object graph could expose account details or create serialization cycles. Filtering confidential fields only in React would still leak them through the network response.

## Trace a department write

The form captures a draft and creates an Operation containing the method, path, serialized body, operation key and optional ETag. HrisApi opens an immediate SQLite transaction, checks the current account, checks for a matching receipt, parses the input and calls HrisPeople.SaveDepartment. The domain method validates the name, uniqueness and lead relationship, then saves the entity. The API adds an audit entry and receipt, saves again, commits, and only then returns the result.

Two SaveChanges calls still belong to one transaction. A failure inserting the receipt rolls back the earlier department insert. The HTTP regression suite deliberately injects a temporary trigger into its private test database to prove that behavior.

## Historical source

The original App, demo API, default-password seed and old documentation are retained as text snapshots. Current compatibility entry points direct readers to the active workshop. When reviewing changes, compare exact originals with current files. Do not run a historical setup command against the new database or assume an old screenshot describes current permissions.


## The dispatcher we wrote, and the ASP.NET Core you will write at work

Every `/api/...` request reaches one route, registered at `server/Nethris.Api/Program.cs:60`:

```csharp
app.MapMethods("/api/{**path}", ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"], (HttpContext http, HrisApi api) => api.Handle(http));
```

After the 2026-09 fix pass the dispatcher is split across four partial files (line counts checked 2026-10-06):

| File | Lines | What it owns |
|---|---|---|
| `HrisApi.cs` | 146 | `Handle` (39-83): path splitting (`ReadRoute`, 85-89), one transaction per request (51-55, deferred for GET, immediate for writes), setup/login (92-124), the role gate `MinimumRole` (127-145) |
| `HrisApi.Reads.cs` | 211 | the GET `switch (parts[0])` (37 onward); `offset`/`limit` parsing at 28-29; malformed ids become 404 at 31-34 |
| `HrisApi.Mutations.cs` | 212 | `Idempotency-Key` check (22-25), fingerprint, receipt replay (`Idempotency-Replayed`, 84), write dispatch |
| `HrisRequest.cs` | 99 | body limits and media type (47-80), `If-Match` parsing with the 428 (34-45), JSON responses |

**Read it for the lesson, not for the shape.** It puts every cross-cutting concern in one place so you can see them in one screen, which helps while you are learning what those concerns *are*. It is not how ASP.NET Core codebases are usually organised. It gives up routing-table introspection, per-endpoint OpenAPI metadata, model binding, per-route authorization metadata, and the ability to call one handler from a test without building an `HttpContext`.

> **Status of the "MapGroup conversion" (checked 2026-10-06).** The 2026-09 review left converting this dispatcher to route groups as an open item. It has **not** been done: `grep -rn MapGroup server` finds nothing. The code blocks below are a design sketch for you to try in a private branch. None of the types they mention (`TransactionFilter`, `IdempotencyFilter`, `IfMatchFilter`, `PageQuery`, an `"HrWrite"` policy) exist in this repository.

**Route groups replace the path splitting.**

```csharp
// Sketch, not repository code.
var api = app.MapGroup("/api").AddEndpointFilter<TransactionFilter>();

var departments = api.MapGroup("/departments");               // reads: any signed-in role
departments.MapGet("/", ListDepartments);                      // binds [AsParameters] PageQuery (offset, limit)
departments.MapGet("/{id:guid}", GetDepartment);
departments.MapPost("/", CreateDepartment)
           .RequireAuthorization("HrWrite").AddEndpointFilter<IdempotencyFilter>();
departments.MapPut("/{id:guid}", UpdateDepartment)
           .RequireAuthorization("HrWrite").AddEndpointFilter<IdempotencyFilter>().AddEndpointFilter<IfMatchFilter>();
```

Three details are easy to get wrong when you translate the funnel:

- **Put the write policy on the write endpoints, not on the group.** `MinimumRole` (`HrisApi.cs:139-142`) demands HR only when `route.IsWrite` and the kind is `employees`, `departments` or `job-roles`. Reads of those kinds need only Employee (line 144). Putting `.RequireAuthorization("HrWrite")` on the `departments` group would also lock Employees out of `GET /api/departments`. Nothing in xUnit would catch that, because `ApiScenarioTests` reads departments only with the admin token. For `job-roles`, `scripts/check_api.py:49` reads the list with the employee token (`page('job-roles', et)`), so the Python suite would fail.
- **A claims-only policy would weaken authorization.** `HrisAuth.Require` (`Security/HrisAuth.cs:19-30`) loads the server session row and the *current* account on every request, then compares `user.Role` from the database. A stock `RequireRole`/`RequireClaim` policy reads the role claim baked into the JWT, so a demoted or deactivated account would keep its old power until the token expires. A faithful conversion needs a custom authorization handler (or an endpoint filter) that still calls `HrisAuth.Require`.
- **`{id:guid}` matches today's 404.** `HrisApi.Reads.cs:31-34` turns a non-GUID id into `HrisError.Missing()`. A route constraint that fails to match also produces 404, so this part is a true like-for-like.

**Endpoint filters replace the top of `Handle`.** A filter is ASP.NET Core's per-endpoint middleware: it runs around the handler and can short-circuit.

```csharp
// Sketch, not repository code.
public sealed class IfMatchFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        if (!http.Request.Headers.TryGetValue("If-Match", out var tag) || tag.Count == 0)
            return Results.Problem(statusCode: 428, title: "Send the record's strong ETag in If-Match.");

        http.Items["If-Match"] = tag.ToString();
        return await next(context);
    }
}
```

This sketch is weaker than the real check. `HrisRequest.Expected` (`HrisRequest.cs:34-45`) also requires the tag to start with `"<route id>-`, to end with a quote and to carry a positive version. It answers 428 when any of those fail. To keep that behaviour, the filter (or the handler) must parse the tag against the route id. The same pattern can hold an `IdempotencyFilter` (validate the UUID key, fingerprint, replay or continue) and a `TransactionFilter` (open the transaction, `await next`, commit on success). Because each filter is attached per endpoint, the route table shows which endpoints have it.

**Typed handlers replace the `switch`/`if` ladders.**

```csharp
// Sketch, not repository code.
private static async Task<Results<Ok<DepartmentView>, NotFound>> GetDepartment(
    Guid id, HrisDbContext db, ClaimsPrincipal user, CancellationToken ct)
{
    var row = await db.Departments.AsNoTracking().SingleOrDefaultAsync(d => d.Id == id, ct);
    return row is null ? TypedResults.NotFound() : TypedResults.Ok(HrisPeople.View(row));
}
```

The handler takes injected parameters instead of digging them out of `HttpContext`, and its return type names both outcomes. You can call it from a test with a `DbContext` and no web server.

**Why the delivered code is not simply wrong.** With a single funnel you cannot forget the transaction, the receipt or the ETag on a new endpoint, because every endpoint goes through the funnel. The idiomatic version regains that safety only through *filters attached deliberately*: stronger when you attach them, weaker when you forget. The funnel has its own omission risk. A brand-new resource kind falls through to `return SecurityRole.Employee` (`HrisApi.cs:144`) unless someone edits `MinimumRole`.

If you attempt the conversion, run **both** suites before and after. `ApiScenarioTests` (15 HTTP cases) exercises receipts, `If-Match`, 428/409 and rollback only on `departments`. Its role refusals cover department writes, audit logs and accounts. Employees, job roles and the PTO workflow are covered only by `scripts/check_api.py`. A green `dotnet test` alone does not prove that every endpoint kept its filters.

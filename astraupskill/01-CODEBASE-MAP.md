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

`server/Nethris.Api/HrisApi.cs` is 194 lines, and every `/api/...` request reaches it through a single route:

```csharp
app.MapMethods("/api/{**path}", ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"],
    (HttpContext http, HrisApi api) => api.Handle(http));
```

`Handle` then splits the path itself, decides the required role from the first segment, opens a transaction, enforces the idempotency key, checks `If-Match`, dispatches on `(method, path)` and serializes the response. **Read it for the lesson, not for the shape.** It puts every cross-cutting concern in one place so you can see them in one screen - which is genuinely useful when you are learning what those concerns *are* - but it is not how an ASP.NET Core codebase is organised, and copying it into a job would be a mistake. Concretely, it gives up routing-table introspection, per-endpoint OpenAPI metadata, model binding, per-route authorization policies, and the ability to unit-test one handler without constructing an `HttpContext`.

Here is the same set of concerns expressed idiomatically. Work through the correspondence line by line; the point is that nothing is lost, only relocated.

**Route groups replace the path splitting.**

```csharp
var api = app.MapGroup("/api").AddEndpointFilter<TransactionFilter>();

var departments = api.MapGroup("/departments").RequireAuthorization("HrWrite");
departments.MapGet("/",       (HrisDbContext db, [AsParameters] PageQuery page) => HrisPeople.List(db, page));
departments.MapGet("/{id:guid}", GetDepartment);
departments.MapPost("/",      CreateDepartment).AddEndpointFilter<IdempotencyFilter>();
departments.MapPut("/{id:guid}", UpdateDepartment).AddEndpointFilter<IdempotencyFilter>()
                                                 .AddEndpointFilter<IfMatchFilter>();
```

- `{id:guid}` does the parsing and the 404-on-malformed-id that `Guid.TryParse` does by hand today.
- `[AsParameters] PageQuery page` binds and validates `limit`/`cursor` once, instead of re-reading the query string in each branch.
- `RequireAuthorization("HrWrite")` replaces the `minimum = parts.FirstOrDefault() is ... ? SecurityRole.Hr : ...` expression. The policy is declared once next to the other policies, and a new route cannot forget it by omission the way a new `if` branch can.

**Endpoint filters replace the top of `Handle`.** A filter is ASP.NET Core's per-endpoint middleware: it runs around the handler and can short-circuit.

```csharp
public sealed class IfMatchFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        if (!http.Request.Headers.TryGetValue("If-Match", out var tag) || tag.Count == 0)
            return Results.Problem(statusCode: 428, title: "Send the record's strong ETag in If-Match.");

        http.Items["If-Match"] = tag.ToString();
        return await next(context);   // the handler compares it with the loaded row's version
    }
}
```

The same shape holds an `IdempotencyFilter` (parse the key, hash the fingerprint, replay a stored receipt or continue) and a `TransactionFilter` (open the transaction, `await next`, commit on success). Each is independently unit-testable, each is attached where it applies, and each appears in the route table so you can see which endpoints are covered.

**Typed handlers replace the `if (method == ... && path == ...)` ladder.**

```csharp
private static async Task<Results<Ok<DepartmentView>, NotFound>> GetDepartment(
    Guid id, HrisDbContext db, ClaimsPrincipal user, CancellationToken ct)
{
    var row = await db.Departments.AsNoTracking().SingleOrDefaultAsync(d => d.Id == id, ct);
    return row is null ? TypedResults.NotFound() : TypedResults.Ok(HrisPeople.View(row));
}
```

One method, one statement per line, parameters injected rather than dug out of `HttpContext`, and a return type that states both outcomes. It can be called directly from a test with a `DbContext` and no web server at all.

**Why the delivered code is not simply wrong.** A single funnel makes it impossible to forget the transaction, the receipt or the ETag on a new endpoint, because there is nowhere to put a new endpoint except inside the funnel. The idiomatic version regains that safety through *filters attached deliberately* - which is stronger when you attach them and weaker when you forget. If you refactor this yourself (a good, large exercise), the check that keeps you honest is `dotnet test`: `ApiScenarioTests` asserts the idempotency, `If-Match`, role and rollback behaviour over HTTP, so any endpoint that loses a filter during the move shows up as a failing status-code assertion rather than as a quiet hole.

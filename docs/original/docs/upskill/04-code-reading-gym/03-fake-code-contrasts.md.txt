# Fake Code Contrasts

All snippets below are illustrative fake code: not from this repo.

## 1. Entity Leak

Bad:
```csharp
// Illustrative fake code: not from this repo.
return Results.Ok(employee);
```
Better: map to DTO as this repo does in [Program.cs](../../../server/Nethris.Api/Program.cs#L412-L432). Why: entities carry navigation shape and persistence concerns.

## 2. UI-Only Authorization

Bad:
```ts
// Illustrative fake code: not from this repo.
if (user.role === "Manager") showApproveButton();
```
Better: UI may hide controls [App.tsx](../../../client/src/App.tsx#L183-L184), but API must enforce ownership [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353).

## 3. N+1 Query

Bad:
```csharp
// Illustrative fake code: not from this repo.
foreach (var e in employees) Console.WriteLine(e.Department.Name);
```
Better: eager-load needed navigations as in [Program.cs](../../../server/Nethris.Api/Program.cs#L119-L124), then measure for over-fetching.

## 4. Unsafe Approval

Bad:
```csharp
// Illustrative fake code: not from this repo.
pto.Status = Approved;
```
Better: reject closed requests and check manager ownership [Program.cs](../../../server/Nethris.Api/Program.cs#L337-L353).

## 5. Swallowed API Error

Bad:
```ts
// Illustrative fake code: not from this repo.
catch {}
```
The repo intentionally catches login fetch failure for demo fallback [api.ts](../../../client/src/api.ts#L24-L35). Outside demo login, prefer visible error handling.

## 6. Public Contract Drift

Bad:
```ts
// Illustrative fake code: not from this repo.
type PtoRequest = { status: string };
```
Better: use string unions as in [types.ts](../../../client/src/types.ts#L1-L4), and compare them to server enums [Enums.cs](../../../server/Nethris.Api/Domain/Enums.cs#L18-L31).

## 7. Direct State Mutation

Bad:
```ts
// Illustrative fake code: not from this repo.
snapshot.ptoRequests.push(created);
setSnapshot(snapshot);
```
Better: create new arrays as in [App.tsx](../../../client/src/App.tsx#L104-L119).

## 8. Seed Every Startup

Bad:
```csharp
// Illustrative fake code: not from this repo.
db.Employees.AddRange(seedEmployees);
```
Better: guard seeding as in [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L15), then make it more granular if seeds expand.

## 9. Hard-Coded Production Secret

Bad:
```json
// Illustrative fake code: not from this repo.
"Key": "prod-secret"
```
Current dev key is in [appsettings.json](../../../server/Nethris.Api/appsettings.json#L5-L9). Better production shape: environment variable/user-secrets/key vault.

## 10. Unbounded List Forever

Bad:
```csharp
// Illustrative fake code: not from this repo.
return await db.Employees.ToListAsync();
```
Current list is unpaginated in [Program.cs](../../../server/Nethris.Api/Program.cs#L141-L146). Fine for lab; add limits before real data.


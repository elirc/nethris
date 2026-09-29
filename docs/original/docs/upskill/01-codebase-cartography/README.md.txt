# Codebase Cartography

Use this section to build a map before changing code. The codebase has a compact shape:

| Area | Primary files | Ownership |
| --- | --- | --- |
| API composition | [Program.cs](../../../server/Nethris.Api/Program.cs#L12-L397) | DI, middleware, endpoint routing, authorization policies. |
| Domain model | [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L3-L26), [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L3-L27), [Enums.cs](../../../server/Nethris.Api/Domain/Enums.cs#L3-L31) | Business nouns and state values. |
| Persistence | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L6-L73) | EF Core sets, relationships, precision, delete behavior. |
| Seed data | [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L9-L123) | Demo tenant, accounts, roles, starter PTO. |
| Client state and UI | [App.tsx](../../../client/src/App.tsx#L29-L543) | Tab navigation, forms, derived state, rendering. |
| Client API boundary | [api.ts](../../../client/src/api.ts#L4-L105) | Fetch calls, bearer token, demo fallback. |

Read [05-key-flows.md](05-key-flows.md) before attempting tickets. A senior does not memorize file names; they understand ownership and blast radius.


> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Writing Tests Here

These recipes are proposed because the repo currently has no test project.

## API Integration Test Setup

Suggested project: `server/Nethris.Api.Tests`. Use `Microsoft.AspNetCore.Mvc.Testing`, xUnit, and a temporary SQLite database. The app currently calls `app.Run()` in [Program.cs](../../../server/Nethris.Api/Program.cs#L397-L397); for `WebApplicationFactory`, you may need to expose `public partial class Program`.

## Recipes

| Recipe | Arrange | Act | Assert |
| --- | --- | --- | --- |
| Login happy path | seed users from [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L39-L45) | POST [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83) | token and role returned |
| PTO validation failure | authenticated employee | POST bad dates to [Program.cs](../../../server/Nethris.Api/Program.cs#L295-L323) | 400 |
| Permission failure | manager token, PTO from non-report | POST decision [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385) | 403 |
| HR final approval | HR token, pending request | POST approved | status approved, balance deducted [Program.cs](../../../server/Nethris.Api/Program.cs#L367-L373) |
| Audit visible to HR only | employee and HR tokens | GET [Program.cs](../../../server/Nethris.Api/Program.cs#L387-L395) | employee 403, HR 200 |
| Org chart shape | seeded data | GET [Program.cs](../../../server/Nethris.Api/Program.cs#L255-L270) | Riley root, Alex under Maya from [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L29-L34) |

## Commands

Inferred after test project exists:

```powershell
dotnet test
dotnet test --filter "Pto"
```

Frontend inferred after adding Vitest/RTL:

```powershell
npm test
```


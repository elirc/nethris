# Fast Track: Weekend Win

## Commands

| Goal | Command | Status |
| --- | --- | --- |
| Install frontend deps | `cd client; npm install --include=dev` | Verified earlier in this workspace. |
| Build frontend | `cd client; npm run build` | Verified passing on 2026-06-09. Script is defined in [client/package.json](../../client/package.json#L6-L9). |
| Run frontend | `cd client; npm run dev` | Inferred from [client/package.json](../../client/package.json#L6-L9) and [client/vite.config.ts](../../client/vite.config.ts#L4-L8). |
| Restore backend | `cd server/Nethris.Api; dotnet restore` | Blocked: `dotnet` command not found during verification. |
| Run backend | `cd server/Nethris.Api; dotnet run` | Inferred from Web SDK project [server/Nethris.Api/Nethris.Api.csproj](../../server/Nethris.Api/Nethris.Api.csproj#L1-L17). |

## First Ten Files

1. [README.md](../../README.md#L1-L39) - app purpose, seed logins, run commands.
2. [server/Nethris.Api/Nethris.Api.csproj](../../server/Nethris.Api/Nethris.Api.csproj#L1-L17) - target framework and packages.
3. [server/Nethris.Api/Program.cs](../../server/Nethris.Api/Program.cs#L12-L68) - service registration and middleware.
4. [server/Nethris.Api/Contracts/Contracts.cs](../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109) - API contracts.
5. [server/Nethris.Api/Domain/Employee.cs](../../server/Nethris.Api/Domain/Employee.cs#L3-L26) - central domain entity.
6. [server/Nethris.Api/Data/HrisDbContext.cs](../../server/Nethris.Api/Data/HrisDbContext.cs#L16-L72) - persistence relationships.
7. [server/Nethris.Api/Data/SeedData.cs](../../server/Nethris.Api/Data/SeedData.cs#L9-L85) - starter organization and accounts.
8. [client/package.json](../../client/package.json#L6-L22) - frontend scripts and dependencies.
9. [client/src/api.ts](../../client/src/api.ts#L12-L105) - browser-to-API boundary.
10. [client/src/App.tsx](../../client/src/App.tsx#L29-L69) - top-level state and role-aware derived state.

## Two Flows To Trace

- Login: [client/src/App.tsx](../../client/src/App.tsx#L71-L87) -> [client/src/api.ts](../../client/src/api.ts#L12-L35) -> [server/Nethris.Api/Program.cs](../../server/Nethris.Api/Program.cs#L70-L83) -> [server/Nethris.Api/Security/JwtTokenService.cs](../../server/Nethris.Api/Security/JwtTokenService.cs#L11-L39).
- PTO approval: [client/src/App.tsx](../../client/src/App.tsx#L123-L162) -> [client/src/api.ts](../../client/src/api.ts#L79-L88) -> [server/Nethris.Api/Program.cs](../../server/Nethris.Api/Program.cs#L325-L385) -> [server/Nethris.Api/Data/HrisDbContext.cs](../../server/Nethris.Api/Data/HrisDbContext.cs#L58-L71).

## Small Safe Change

Documentation-only: add one example to [08-reference/command-cheatsheet.md](08-reference/command-cheatsheet.md) after running it. If product-code changes are later allowed, a safe first code ticket is fixing the visible mojibake in [client/src/App.tsx](../../client/src/App.tsx#L344-L344) and [client/src/App.tsx](../../client/src/App.tsx#L446-L446); keep it tiny and run `npm run build`.

## Teach-Back

Explain this in five minutes: "A PTO request starts as React form state, becomes a DTO, is authorized by ASP.NET Core, is persisted by EF Core, and returns a response shape rendered by the UI." Use exact anchors from [01-codebase-cartography/05-key-flows.md](01-codebase-cartography/05-key-flows.md).

## What This Skips

It skips migrations, test implementation, production secrets, deployment, rate limiting, and background jobs. Those are not present in the repo; see [08-reference/risk-register.md](08-reference/risk-register.md).

## Verification Notes

`npm run build` passed. `dotnet --version` failed because `dotnet` is not on PATH. No product code was changed for this documentation pass.


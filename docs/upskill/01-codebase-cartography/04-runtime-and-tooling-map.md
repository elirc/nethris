> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Runtime And Tooling Map

## .NET

- The API uses `Microsoft.NET.Sdk.Web` and targets `net8.0` in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L1-L5).
- Nullable reference types are enabled in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L2-L5).
- Packages include JWT bearer auth, EF Core design, EF Core SQLite, and Swagger in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L8-L16).
- Runtime configuration uses SQLite at `Data Source=nethris.db` in [appsettings.json](../../../server/Nethris.Api/appsettings.json#L2-L4) and a development JWT key in [appsettings.json](../../../server/Nethris.Api/appsettings.json#L5-L9).

## React

- Vite dev/build/preview scripts live in [client/package.json](../../../client/package.json#L6-L9).
- React, React DOM, Vite, TypeScript, and lucide icons are declared in [client/package.json](../../../client/package.json#L11-L22).
- Vite uses React plugin and port 5173 in [client/vite.config.ts](../../../client/vite.config.ts#L1-L8).
- The frontend API base defaults to `http://localhost:5187` in [client/src/api.ts](../../../client/src/api.ts#L4-L4).

## Runtime Boundaries

Browser state is local React state in [client/src/App.tsx](../../../client/src/App.tsx#L29-L37). Server state is persisted through EF Core in [Program.cs](../../../server/Nethris.Api/Program.cs#L317-L322) and [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L384). There is no background worker runtime in the current repo.

## Verification Notes

`npm run build` passed. `dotnet --version` failed, so backend commands are inferred from project files rather than executed.


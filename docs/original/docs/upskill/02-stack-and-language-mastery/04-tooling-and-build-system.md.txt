# Tooling And Build System

## .NET Build

The API project file is [server/Nethris.Api/Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L1-L17). It uses:

- `Microsoft.NET.Sdk.Web` in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L1-L1)
- `net8.0` in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L3-L3)
- nullable and implicit usings in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L4-L5)
- EF Core SQLite and Swagger packages in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L8-L16)

No solution file or SDK pin (`global.json`) was found. That is acceptable for a small lab, but a team repo should consider a solution file and SDK pin for reproducibility.

## Frontend Build

Scripts are [client/package.json](../../../client/package.json#L6-L9). Vite config is [client/vite.config.ts](../../../client/vite.config.ts#L1-L9). TypeScript config is present at `client/tsconfig.json`.

Verified:

```powershell
cd client
npm run build
```

Result: passed.

## Dependency Judgment

npm audit reported two moderate findings earlier. Do not blindly run `npm audit fix --force` in a training repo; first inspect whether it changes major versions. For .NET dependencies, use `dotnet list package --vulnerable` after installing the SDK.

## Drill

Explain why `npm run build` proves TypeScript and bundling, but does not prove the API contract works at runtime.


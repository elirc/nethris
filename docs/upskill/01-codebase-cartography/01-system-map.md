> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# System Map

## Project Graph

```text
repo
|-- server/Nethris.Api          ASP.NET Core 8 API
|   |-- Domain                  EF entities and enums
|   |-- Contracts               request/response records
|   |-- Data                    DbContext and seed data
|   |-- Security                password hashing and JWT creation
|   `-- Program.cs              composition root and endpoints
`-- client                      Vite React TypeScript app
    |-- src/App.tsx             UI shell and workflows
    |-- src/api.ts              API/demo data boundary
    |-- src/types.ts            TS contracts mirroring API responses
    `-- src/demoData.ts         offline demo snapshot
```

## Runtime Boundaries

| Boundary | Evidence | What crosses it |
| --- | --- | --- |
| Browser -> API | [client/src/api.ts](../../../client/src/api.ts#L90-L105) | JSON requests and optional bearer token. |
| API -> SQLite | [server/Nethris.Api/Program.cs](../../../server/Nethris.Api/Program.cs#L14-L15), [appsettings.json](../../../server/Nethris.Api/appsettings.json#L2-L4) | EF Core DbContext operations. |
| API -> JWT consumer | [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L21-L38) | Signed claims for user, role, employee id. |
| Client demo fallback | [client/src/api.ts](../../../client/src/api.ts#L24-L39) | Local demo data when API login cannot be reached. |

## Public Interfaces

- Minimal API endpoints are declared in [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L395).
- Response/request records are declared in [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109).
- React has no router package; tabs are local state in [App.tsx](../../../client/src/App.tsx#L19-L37) and rendered conditionally in [App.tsx](../../../client/src/App.tsx#L210-L352).

## Confirmed Absences

No `.sln`, `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, test project, CI workflow, Dockerfile, compose file, EF migrations folder, hosted service, queue, or `IHttpClientFactory` usage was found by file inventory excluding `node_modules`.

## Judgment Drill

Draw three boxes: Client, API, Database. Put each file above into one box. Then mark which files you would touch for "add sick leave type". If your answer touches only the UI, you missed persistence and contract ownership.


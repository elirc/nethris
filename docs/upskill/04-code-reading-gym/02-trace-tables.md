> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Trace Tables

## Trace: UI Login To JWT

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | [App.tsx](../../../client/src/App.tsx#L71-L87) | `FormData` | UI | form -> `login(email,password)` | defaults hide real auth entry |
| 2 | [api.ts](../../../client/src/api.ts#L12-L35) | `{ email, password }` | client API | POST or demo fallback | fallback masks outage |
| 3 | [Program.cs](../../../server/Nethris.Api/Program.cs#L72-L81) | `UserAccount` | API | verify -> token response | no lockout |
| 4 | [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L21-L38) | claims | security | claims -> JWT | stale role until expiry |

## Trace: Directory Search

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | [api.ts](../../../client/src/api.ts#L42-L49) | snapshot promises | client | parallel fetch | all-or-nothing |
| 2 | [Program.cs](../../../server/Nethris.Api/Program.cs#L117-L147) | `IQueryable<Employee>` | API/EF | filters -> list -> DTOs | no pagination |
| 3 | [App.tsx](../../../client/src/App.tsx#L43-L51) | `Employee[]` | UI | local search filter | duplicated filtering |
| 4 | [App.tsx](../../../client/src/App.tsx#L413-L435) | selected employee | UI | facts + reports | depends on full snapshot |

## Trace: PTO Request

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | [App.tsx](../../../client/src/App.tsx#L275-L293) | form inputs | UI | user -> submit event | minimal validation |
| 2 | [App.tsx](../../../client/src/App.tsx#L89-L121) | payload | UI | payload -> local snapshot update | optimistic audit |
| 3 | [api.ts](../../../client/src/api.ts#L54-L77) | DTO | client API | POST or demo object | demo bypasses server rules |
| 4 | [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L319) | entity + audit | API/EF | validate -> save | no balance check |

## Trace: Approval Permission

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | [App.tsx](../../../client/src/App.tsx#L65-L69) | visible requests | UI | status/role filter | not security |
| 2 | [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385) | route id + decision | API | load PTO -> authorize -> mutate | needs tests |
| 3 | [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353) | claims + PTO employee manager | API | resource auth | IDOR boundary |
| 4 | [Program.cs](../../../server/Nethris.Api/Program.cs#L363-L378) | status | domain logic | transition state | balance can go negative |

## Trace: Persistence Mapping

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L8-L14) | DbSets | EF | table access | all tables on one context |
| 2 | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L18-L71) | model builder | EF | constraints/relationships | migrations absent |
| 3 | [Program.cs](../../../server/Nethris.Api/Program.cs#L399-L410) | includes | API/EF | eager-load graph | over-fetching |
| 4 | [Program.cs](../../../server/Nethris.Api/Program.cs#L412-L471) | entities | API | entities -> DTOs | mapping in `Program.cs` grows |

## Trace: Error Path

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | [api.ts](../../../client/src/api.ts#L100-L104) | non-OK response | client API | throw generic error | loses body details |
| 2 | [App.tsx](../../../client/src/App.tsx#L76-L85) | `Error` | UI | message -> auth error | only login shows error |
| 3 | [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303) | bad PTO input | API | 400 with message | UI does not render API error for PTO |


# Boundaries And Layers

## Current Boundaries

| Layer | Owns | Must not own | Evidence |
| --- | --- | --- | --- |
| React UI | Rendering, local form state, role-aware visibility | Final authorization, persistence invariants | [App.tsx](../../../client/src/App.tsx#L29-L69), [App.tsx](../../../client/src/App.tsx#L300-L324) |
| Client API | HTTP transport, token header, demo fallback | Business rules | [api.ts](../../../client/src/api.ts#L90-L105) |
| Minimal API | Request binding, auth checks, orchestration, mapping | Long-term domain complexity if it grows | [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L395) |
| Domain entities | Data shape and navigation | UI details | [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L3-L26), [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L3-L27) |
| EF Core | Relationships, indexes, delete behavior | Request authorization | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L16-L72) |

## Boundary Leaks To Watch

- `Program.cs` contains endpoint definitions, state transitions, mappings, and claims helpers in one file [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L502). This is fine for now; possible risk as features grow.
- UI demo fallback mirrors business behavior in [api.ts](../../../client/src/api.ts#L54-L88). Useful for demos; risk of drift.
- TypeScript contracts mirror C# contracts manually: [types.ts](../../../client/src/types.ts#L69-L81) vs [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L80-L91).

## Drill

For "add PTO cancellation", list one line in each layer that proves where the change belongs.


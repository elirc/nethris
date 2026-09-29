# Annotation Drills

For each drill, annotate inputs, outputs, dependencies, invariants, side effects, failure modes, and one test you would add.

| Drill | Code | Focus | Self-grade |
| --- | --- | --- | --- |
| 1. Login handler | [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83) | authn, password verification, DTO response | Basic: names inputs. Solid: explains `Include`. Strong: adds lockout/rate-limit test. |
| 2. JWT creation | [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L18-L38) | claims, expiry, signing | Strong answer mentions stale roles and revocation. |
| 3. Employee search | [Program.cs](../../../server/Nethris.Api/Program.cs#L117-L147) | `IQueryable`, filters, includes | Strong answer identifies `ToLower().Contains` performance risk. |
| 4. Employee create | [Program.cs](../../../server/Nethris.Api/Program.cs#L155-L201) | duplicate check, account creation, audit | Strong answer notices no validation framework. |
| 5. Org chart recursion | [Program.cs](../../../server/Nethris.Api/Program.cs#L434-L449) | recursion, hierarchy invariant | Strong answer proposes cycle detection. |
| 6. PTO request | [Program.cs](../../../server/Nethris.Api/Program.cs#L295-L323) | claim ownership, validation, audit | Strong answer asks about balance and overlaps. |
| 7. PTO decision | [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385) | state machine, authorization, side effects | Strong answer writes allowed transition table. |
| 8. EF model | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L18-L71) | relationships and delete behavior | Strong answer explains each delete behavior. |
| 9. Snapshot loading | [api.ts](../../../client/src/api.ts#L37-L52) | parallel fetch and role-specific audit call | Strong answer notes partial failure behavior. |
| 10. React derived queues | [App.tsx](../../../client/src/App.tsx#L53-L69) | client visibility and memo deps | Strong answer says UI is not security. |
| 11. React PTO submit | [App.tsx](../../../client/src/App.tsx#L89-L121) | forms, local state update, demo/API split | Strong answer notices optimistic local audit. |
| 12. Seed data | [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L9-L85) | startup side effects, idempotency | Strong answer flags migration transition. |

## Drill Template

```md
Inputs:
Outputs:
Dependencies:
Invariant:
Side effects:
Failure modes:
Test I would add:
Line I still do not understand:
```


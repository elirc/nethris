> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# File Reading Order

## Junior Path

| Order | File | Look for |
| --- | --- | --- |
| 1 | [README.md](../../../README.md#L1-L39) | How the app is meant to run. |
| 2 | [client/package.json](../../../client/package.json#L6-L22) | Scripts and frontend stack. |
| 3 | [client/src/types.ts](../../../client/src/types.ts#L1-L100) | Data shapes the UI expects. |
| 4 | [client/src/demoData.ts](../../../client/src/demoData.ts#L22-L55) | Seed accounts mirrored in UI. |
| 5 | [client/src/api.ts](../../../client/src/api.ts#L12-L52) | Login and snapshot loading. |
| 6 | [client/src/App.tsx](../../../client/src/App.tsx#L71-L162) | Event handlers and state updates. |
| 7 | [server/Nethris.Api/Contracts/Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109) | API contract records. |
| 8 | [server/Nethris.Api/Program.cs](../../../server/Nethris.Api/Program.cs#L70-L153) | Login and read endpoints. |
| 9 | [server/Nethris.Api/Program.cs](../../../server/Nethris.Api/Program.cs#L295-L385) | PTO write and approval endpoints. |
| 10 | [server/Nethris.Api/Data/HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L18-L71) | Relationships and delete behavior. |

## Mid-Level Path

Add these:

- [server/Nethris.Api/Security/JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L18-L38) - claims and token lifetime.
- [server/Nethris.Api/Security/PasswordService.cs](../../../server/Nethris.Api/Security/PasswordService.cs#L11-L30) - PBKDF2 hashing and verification.
- [server/Nethris.Api/Data/SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L9-L85) - seed idempotency and two-phase department lead assignment.
- [server/Nethris.Api/Program.cs](../../../server/Nethris.Api/Program.cs#L399-L479) - mapping helpers.
- [client/src/App.tsx](../../../client/src/App.tsx#L53-L69) - derived permissions and visible request filtering.

## Senior Path

Add these questions:

- Is putting all endpoints and helpers in [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L502) still acceptable after the next three features?
- Does [api.ts](../../../client/src/api.ts#L24-L35) hide API outages by falling back to demo auth?
- Should [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L12) use `EnsureCreatedAsync` after migrations are introduced?
- What tests would freeze the authorization invariant in [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353)?


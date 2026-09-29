# Architecture Critique

## Strong Choices

- Clear contract/entity split: [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109) vs [Domain](../../../server/Nethris.Api/Domain/Employee.cs#L3-L26).
- Explicit EF delete behavior and indexes in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L18-L71).
- Server-side resource authorization for PTO approval in [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353).
- Client can run in demo mode through [api.ts](../../../client/src/api.ts#L24-L39), useful for onboarding.

## Risks And Tradeoffs

| Priority | Risk | Evidence | Recommendation |
| --- | --- | --- | --- |
| High | No automated tests | No test projects found; critical auth logic in [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353). | Add API integration tests first. |
| High | Development JWT key in config | [appsettings.json](../../../server/Nethris.Api/appsettings.json#L5-L9) | Move secrets to user-secrets/env for non-dev. |
| Medium | `Program.cs` accumulating responsibilities | [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L502) | Extract endpoint groups/services when adding features. |
| Medium | No migrations | [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11) | Introduce migrations before schema growth. |
| Medium | Unbounded org chart and employee list | [Program.cs](../../../server/Nethris.Api/Program.cs#L141-L146), [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L261) | Add pagination/search limits before large data. |
| Medium | UI/API contract drift | [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L80-L91), [types.ts](../../../client/src/types.ts#L69-L81) | Add contract tests or generated OpenAPI client. |

## If I Owned This For Three Months

1. Add integration tests around login, PTO visibility, approval authorization, and audit logging.
2. Add migrations and replace `EnsureCreatedAsync`.
3. Split `Program.cs` into endpoint groups plus small services only where tests demand it.
4. Add structured logging and health checks beyond `/api/health` in [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L68).
5. Add pagination and resource-specific permission policy helpers.

## Confirmed Vs Hypothesis

- Confirmed: no tests/CI/migrations/workers found.
- Confirmed: frontend build passes.
- Hypothesis: `ToLower().Contains` may become a search performance issue; measure with data before optimizing [Program.cs](../../../server/Nethris.Api/Program.cs#L126-L133).
- Hypothesis: audit detail may be insufficient for real HR compliance; confirm requirements first.


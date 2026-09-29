> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Mid-Level Feature Tickets

## Ticket 1: Add PTO cancellation
Touch contracts [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L80-L100), status enum [Enums.cs](../../../server/Nethris.Api/Domain/Enums.cs#L18-L25), endpoint [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385), UI actions [App.tsx](../../../client/src/App.tsx#L480-L500). Risk: refund balance only when cancelling approved PTO.

## Ticket 2: Add PTO type
Touch `PtoRequest` [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L3-L15), contract [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L74-L91), DbContext/migration [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L58-L64), UI form [App.tsx](../../../client/src/App.tsx#L275-L293). Rollback: nullable column first.

## Ticket 3: Add employee create/edit UI
API exists at [Program.cs](../../../server/Nethris.Api/Program.cs#L155-L253); UI currently only displays profiles [App.tsx](../../../client/src/App.tsx#L210-L247). Include permission visibility using [App.tsx](../../../client/src/App.tsx#L40-L41).

## Ticket 4: Add API integration tests
Create test project for [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L395). Priority tests listed in [05-quality-engineering/01-testing-strategy.md](../05-quality-engineering/01-testing-strategy.md).

## Ticket 5: Introduce EF migrations
Replace `EnsureCreatedAsync` [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11) with migration strategy. Risk: data loss if mishandled.

## Ticket 6: Add pagination to employee list
Change endpoint [Program.cs](../../../server/Nethris.Api/Program.cs#L117-L147), contract [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L30-L49), and UI [App.tsx](../../../client/src/App.tsx#L228-L243). Risk: profile panel needs selected employee consistency.

## Ticket 7: Add role management audit detail
Employee account role update is [Program.cs](../../../server/Nethris.Api/Program.cs#L228-L246); audit is [Program.cs](../../../server/Nethris.Api/Program.cs#L248-L249). Add old/new role details.

## Ticket 8: Add approval comments input
Client currently hardcodes comments [App.tsx](../../../client/src/App.tsx#L123-L126). Server contract supports comment [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L100-L100). Add UI field.

## Ticket 9: Add direct-report recursive manager visibility
Manager PTO visibility is direct reports only [Program.cs](../../../server/Nethris.Api/Program.cs#L282-L286). Decide whether managers should see indirect reports; test carefully.

## Ticket 10: Add OpenAPI-generated client
Swagger is enabled [Program.cs](../../../server/Nethris.Api/Program.cs#L47-L62). Replace manual drift between [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109) and [types.ts](../../../client/src/types.ts#L1-L100).

## Ticket 11: Add production-safe demo mode guard
Demo fallback is [api.ts](../../../client/src/api.ts#L24-L39). Gate it behind `VITE_ENABLE_DEMO`.

## Ticket 12: Add structured validation layer
Manual validation is scattered [Program.cs](../../../server/Nethris.Api/Program.cs#L160-L163), [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303). Introduce a consistent approach with tests.


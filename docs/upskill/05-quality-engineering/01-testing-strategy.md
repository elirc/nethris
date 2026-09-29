> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Testing Strategy

## Current State

No test project or frontend test setup was found. That means every testing recommendation here is a proposed contribution, not a claim about existing coverage.

## Test Layers

| Layer | What belongs here | Repo anchors |
| --- | --- | --- |
| Unit | Pure helpers like password hash verify, state transition service if extracted | [PasswordService.cs](../../../server/Nethris.Api/Security/PasswordService.cs#L11-L30) |
| API integration | Auth, resource authorization, EF persistence, audit logging | [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L395) |
| EF/model | relationships, delete behavior, migrations after introduced | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L18-L71) |
| UI component | tab visibility, PTO form behavior, approval queue rendering | [App.tsx](../../../client/src/App.tsx#L179-L352) |
| E2E | login -> request PTO -> approve -> audit visible | [api.ts](../../../client/src/api.ts#L12-L105), [Program.cs](../../../server/Nethris.Api/Program.cs#L295-L395) |

## First Tests To Add

1. Login returns JWT for seeded HR user: [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L39-L45), [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83).
2. Employee cannot see other employees' PTO in `/api/pto`: [Program.cs](../../../server/Nethris.Api/Program.cs#L278-L281).
3. Manager sees direct-report PTO only: [Program.cs](../../../server/Nethris.Api/Program.cs#L282-L286).
4. Manager cannot approve non-report PTO: [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L348).
5. Manager cannot finalize HR-stage PTO: [Program.cs](../../../server/Nethris.Api/Program.cs#L350-L353).
6. HR approval deducts PTO balance and audits: [Program.cs](../../../server/Nethris.Api/Program.cs#L367-L381).
7. Bad PTO dates return 400: [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303).
8. Audit endpoint rejects non-HR users: [Program.cs](../../../server/Nethris.Api/Program.cs#L387-L395).

## What Not To Test

- Do not snapshot-test every CSS class in [styles.css](../../../client/src/styles.css#L1-L643).
- Do not test EF Core itself; test your model behavior and queries.
- Do not mock `DbContext` for integration behavior; use SQLite/in-memory test database with `WebApplicationFactory` once a test project exists.


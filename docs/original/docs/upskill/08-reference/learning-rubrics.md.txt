# Learning Rubrics

## Junior

Observable behaviors:

- Can run verified frontend build from [command-cheatsheet.md](command-cheatsheet.md).
- Can trace login through [App.tsx](../../../client/src/App.tsx#L71-L87), [api.ts](../../../client/src/api.ts#L12-L35), and [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83).
- Makes small scoped changes and runs the relevant check.
- Asks with evidence and line anchors.

## Mid-Level

Observable behaviors:

- Designs cross-layer changes touching contracts, API, EF, UI, and tests.
- Distinguishes UI visibility [App.tsx](../../../client/src/App.tsx#L65-L69) from API authorization [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353).
- Adds useful regression tests.
- Explains risk, rollback, and blast radius.

## Senior

Observable behaviors:

- Identifies architectural commitments in [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L502).
- Prioritizes risks in [risk-register.md](risk-register.md).
- Proposes migration, observability, security, and performance plans.
- Reviews kindly but blocks unsafe permission/data changes.

## Self-Assessment

Score 1-5:

- I can cite exact lines for major flows.
- I can explain why each boundary exists.
- I can write a test plan before writing code.
- I can identify possible risk without overstating it as a bug.
- I can explain how a change rolls back.


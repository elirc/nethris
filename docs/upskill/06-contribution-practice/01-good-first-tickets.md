> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Good First Tickets

## Ticket 1: Document local API port
**Difficulty:** Easy **Time:** 20m **Skills:** docs, verification. **Story:** As a learner, I want one command list so setup is predictable. **Acceptance criteria:** update docs only; cite [client/src/api.ts](../../../client/src/api.ts#L4-L4) and [README.md](../../../README.md#L22-L39). **Checks:** no product code.

## Ticket 2: Fix visible mojibake
**Difficulty:** Easy **Time:** 20m **Skills:** React text cleanup. **Read first:** [App.tsx](../../../client/src/App.tsx#L344-L344), [App.tsx](../../../client/src/App.tsx#L446-L446). **Acceptance criteria:** replace mojibake separator with plain ASCII `-` or correct middle dot; `npm run build`.

## Ticket 3: Add empty-state copy for directory search
**Difficulty:** Easy **Read first:** [App.tsx](../../../client/src/App.tsx#L228-L243). **Acceptance criteria:** when `filteredEmployees` is empty, show an `EmptyState`.

## Ticket 4: Surface PTO API errors
**Difficulty:** Easy-Medium **Read first:** [App.tsx](../../../client/src/App.tsx#L89-L121), [api.ts](../../../client/src/api.ts#L100-L104). **Acceptance criteria:** failed PTO submit shows user-facing error.

## Ticket 5: Add command to verify API health
**Difficulty:** Easy **Read first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L68). **Acceptance criteria:** cheatsheet includes `Invoke-WebRequest http://localhost:5187/api/health`.

## Ticket 6: Add `CancellationToken` to long reads
**Difficulty:** Easy-Medium **Read first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L117-L147), [Program.cs](../../../server/Nethris.Api/Program.cs#L255-L270). **Acceptance criteria:** token is passed to EF async calls.

## Ticket 7: Improve login error specificity
**Difficulty:** Easy **Read first:** [App.tsx](../../../client/src/App.tsx#L76-L85). **Acceptance criteria:** distinguish invalid demo credentials from API unavailable.

## Ticket 8: Add comments for approval state machine
**Difficulty:** Easy **Read first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L337-L378). **Acceptance criteria:** short comment names transition invariant without restating code.

## Ticket 9: Add README link to upskill docs
**Difficulty:** Easy **Read first:** [README.md](../../../README.md#L1-L39). **Acceptance criteria:** root README points to [docs/upskill/README.md](../README.md).

## Ticket 10: Add PTO status legend
**Difficulty:** Easy **Read first:** [App.tsx](../../../client/src/App.tsx#L21-L27), [App.tsx](../../../client/src/App.tsx#L498-L500). **Acceptance criteria:** UI shows status meanings.

## Ticket 11: Add direct-report count to org node
**Difficulty:** Easy **Read first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L434-L449), [App.tsx](../../../client/src/App.tsx#L438-L459). **Acceptance criteria:** count appears without breaking build.

## Ticket 12: Add seed data note to docs
**Difficulty:** Easy **Read first:** [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L29-L45). **Acceptance criteria:** docs list seeded accounts and roles.

## Ticket 13: Add frontend environment example
**Difficulty:** Easy **Read first:** [api.ts](../../../client/src/api.ts#L4-L4). **Acceptance criteria:** `.env.example` documents `VITE_API_BASE_URL`.

## Ticket 14: Extract status label helper
**Difficulty:** Easy-Medium **Read first:** [App.tsx](../../../client/src/App.tsx#L21-L27), [App.tsx](../../../client/src/App.tsx#L498-L500). **Acceptance criteria:** behavior unchanged; build passes.

## Ticket 15: Add audit list empty state
**Difficulty:** Easy **Read first:** [App.tsx](../../../client/src/App.tsx#L327-L352). **Acceptance criteria:** empty audit array renders helpful message.

## Ticket 16: Add duplicate employee UI validation
**Difficulty:** Medium **Read first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L160-L163). **Acceptance criteria:** UI prevents obvious duplicate in loaded snapshot; server remains source of truth.

## Ticket 17: Add basic logger calls
**Difficulty:** Medium **Read first:** [Program.cs](../../../server/Nethris.Api/Program.cs#L295-L385). **Acceptance criteria:** structured logs on PTO request/decision; no sensitive data.

## Ticket 18: Add package vulnerability command
**Difficulty:** Easy **Read first:** [client/package.json](../../../client/package.json#L11-L22), [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L8-L16). **Acceptance criteria:** cheatsheet documents npm and dotnet vulnerability checks.


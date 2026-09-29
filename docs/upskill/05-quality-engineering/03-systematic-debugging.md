> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Systematic Debugging

Use the loop: reproduce -> narrow layer -> hypothesize -> test cheaply -> fix root cause -> add regression coverage.

## Scenario: Login Falls Back To Demo

**Reproduction:** API is down; frontend login with seeded email succeeds. **First question:** API unavailable or credentials invalid? **Narrowing path:** inspect [api.ts](../../../client/src/api.ts#L12-L35), check network panel for `/api/auth/login`, check API health [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L68). **Likely root causes:** API not running, wrong `VITE_API_BASE_URL`, CORS. **Regression test:** UI shows "Demo session" from [App.tsx](../../../client/src/App.tsx#L187-L190). **Senior lesson:** fallback behavior needs visible environment semantics.

## Scenario: Manager Cannot Approve Request

**Reproduction:** manager clicks approve and gets 403. **First question:** is request for direct report and in manager stage? **Narrowing path:** check queue filter [App.tsx](../../../client/src/App.tsx#L65-L69), API resource checks [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353), seed manager graph [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L29-L34). **Regression test:** manager can approve direct report pending-manager only.

## Scenario: Org Chart Hangs Or Overflows

**Reproduction:** org chart endpoint stalls. **First question:** data size or cycle? **Narrowing path:** inspect recursive helper [Program.cs](../../../server/Nethris.Api/Program.cs#L434-L449), manager relationship [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L19-L21). **Likely root causes:** cycle, huge employee count. **Regression test:** reject manager cycles on employee update [Program.cs](../../../server/Nethris.Api/Program.cs#L203-L253).

## Scenario: PTO Balance Is Wrong

**Reproduction:** balance decreases too much or goes negative. **First question:** double approval or insufficient balance? **Narrowing path:** closed guard [Program.cs](../../../server/Nethris.Api/Program.cs#L337-L340), deduction [Program.cs](../../../server/Nethris.Api/Program.cs#L367-L373), request validation [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303). **Regression test:** approving closed request does not deduct again.

## Scenario: Audit Log Missing

**Reproduction:** HR cannot see expected event. **First question:** was event server-side or demo-only? **Narrowing path:** server writes [Program.cs](../../../server/Nethris.Api/Program.cs#L196-L197), [Program.cs](../../../server/Nethris.Api/Program.cs#L318-L319), [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381); UI demo writes [App.tsx](../../../client/src/App.tsx#L104-L119). **Regression test:** API action writes persisted audit.


# Review Katas

## Kata 1: "Let managers approve all PTO"
**Author intent:** simplify approvals. **Fake diff summary:** removes [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353). **Expected findings:** Blocking: IDOR/resource authorization regression. **Good comment:** > This removes the direct-report invariant. Please keep a server-side check and add an integration test for a manager approving a non-report request.

## Kata 2: "Return Employee entity directly"
**Resembles:** mapping helper [Program.cs](../../../server/Nethris.Api/Program.cs#L412-L432). **Finding:** Important: contract leak and navigation serialization risk. **Comment:** > Can we keep `EmployeeResponse` so the API contract remains stable if EF navigation shape changes?

## Kata 3: "Add PTO type only in React"
**Resembles:** PTO form [App.tsx](../../../client/src/App.tsx#L275-L293). **Finding:** Blocking: missing API/entity persistence. **Comment:** > This renders the field but drops it before save. Please update contract, entity, DbContext/migration, seed/demo data, and tests.

## Kata 4: "Use localStorage for JWT"
**Resembles:** session state [App.tsx](../../../client/src/App.tsx#L29-L37). **Finding:** Important security discussion. **Comment:** > Persistence changes the threat model. Please document XSS risk and consider refresh/session strategy before storing tokens.

## Kata 5: "Fix search with `.ToList()` before filtering"
**Resembles:** query [Program.cs](../../../server/Nethris.Api/Program.cs#L117-L147). **Finding:** Blocking for performance. **Comment:** > This moves filtering from SQL to memory. Keep filters on `IQueryable` and add tests around query behavior if possible.

## Kata 6: "Add audit log only in UI"
**Resembles:** local audit update [App.tsx](../../../client/src/App.tsx#L104-L119). **Finding:** Blocking: audit must be server-side. **Comment:** > UI audit is demo feedback, not a trustworthy audit record. Please write audit entries on the API side like [Program.cs](../../../server/Nethris.Api/Program.cs#L318-L319).

## Kata 7: "Swallow all API request errors"
**Resembles:** request helper [api.ts](../../../client/src/api.ts#L90-L105). **Finding:** Important: debugging and UX regression. **Comment:** > Please preserve status/body details so users and logs can distinguish validation, auth, and server failures.

## Kata 8: "Delete department cascades employees"
**Resembles:** delete behavior [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L27-L30). **Finding:** Blocking: data loss blast radius. **Comment:** > HR data should not disappear because a department is removed. Keep restrict or design an archival workflow.


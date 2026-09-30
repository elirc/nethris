# Seven checkpoints for one small feature

## Placement questions

Explain why a department's headcount belongs in its response but not its input. Explain what changes when a browser retries with the same operation key but different serialized body. Distinguish a missing `If-Match` header from a stale valid one. Finally, explain why filtering the twelve visible employee cards cannot implement a correct server-paginated status filter.

If you cannot answer these, start at checkpoint 1. If you can, still produce the contract table before coding. A familiar feature can hide a different update or retry policy.

## 1. Map the existing department path

Read [Workshop.tsx](../../client/src/Workshop.tsx), [workshopApi.ts](../../client/src/workshopApi.ts), [WorkshopContracts.cs](../../server/Nethris.Api/Contracts/WorkshopContracts.cs), [HrisPeople.cs](../../server/Nethris.Api/Domain/HrisPeople.cs) and [HrisApi.Mutations.cs](../../server/Nethris.Api/HrisApi.Mutations.cs). Locate the form field definition, serialized operation, `DepartmentInput`, `SaveDepartment`, response projection, audit insertion and receipt insertion.

Deliver a diagram with these actual files and a sentence for each transition. Checkpoint question: which layer can reject an otherwise valid field change because the record version is stale?

## 2. Write a description contract

Propose an optional department description using the shared optional-text policy: whitespace-only input becomes null; otherwise trim surrounding whitespace, then require at most 250 characters and no control characters in the normalized text. Leading or trailing tabs/newlines can therefore be trimmed, while an embedded control character is rejected. Record the meaning of omission in the replacement-style PUT input. Include ordinary text, blank text, 251 normalized characters, surrounding whitespace and an embedded control character.

Your first deliverable is a table of expected acceptance, stored value and response value. No UI code is needed yet. Have a reviewer challenge one ambiguous row.

## 3. Change persistence and contracts deliberately

The entity is [Department.cs](../../server/Nethris.Api/Domain/Department.cs); inputs/views live in WorkshopContracts. Follow [HrisRules.OptionalText](../../server/Nethris.Api/Domain/HrisRules.cs) instead of adding a second normalization policy. Inspect [HrisDatabase](../../server/Nethris.Api/Data/HrisDatabase.cs) before deciding how a changed entity reaches a database.

For the first exercise, create a fresh practice database and record the schema decision. A migration of existing data is a separate checkpoint with its own copied input and rollback/recovery evidence. Merely ignoring a missing column does not make old data compatible.

## 4. Complete the server path

Wire input validation, entity assignment and response projection. Extend tests so create returns the value, edit replaces it and blank clears it. Reject invalid input without a committed domain, audit or receipt change. Use actual database assertions when claiming rollback. An exception assertion alone cannot establish the surviving state.

## 5. Complete the browser path

Add the field to the correct editor. Display accepted values from the response. Preserve a draft after validation rejection and stale-version conflict. The explicit “use latest version, keep draft” action should remain a user decision; silently changing the expected version underneath an old draft weakens the conflict boundary.

## 6. Transfer the pattern to filtering

Add a status filter with a defined set of accepted values. Compose it into the employee query before both membership paging and total counting. Reset the browser offset when the filter changes. Use fixture employees beyond the first unfiltered page to prove the query is applied on the server.

## 7. Present evidence and limits

Show one valid operation, one rejected input, one stale edit and one independent retry observation. Record the exact command and scope of every check. A build establishes compilation; a mocked unit test establishes a narrower behavior than a real middleware/database scenario. Use the [review rubric](05-solutions-and-review.md) to choose your next improvement.

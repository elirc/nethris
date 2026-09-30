# Four assignments with explicit contracts

These are future feature exercises. The learning-pack addition does not implement them in the application. Finish one ticket and its rejection cases before starting another.

## HRIS-01 — department description

Add an optional description using `HrisRules.OptionalText`: whitespace-only input becomes null; other input is trimmed, then the normalized value must have at most 250 characters and no remaining control characters. Test surrounding tabs/newlines separately from an embedded control character so the contract matches the shared rule. Change the entity, input record, output view, domain assignment, form definition and checks together. Explain omission and clearing under this API's replacement-style PUT semantics.

Acceptance: create returns and displays the description; edit replaces it; blank clears it; invalid text leaves the stored record/audit/receipt unchanged; a stale edit preserves the browser draft. Ordinary read permissions must remain intentional. Use a fresh practice database for the first schema variant and document the compatibility boundary.

Stretch task: design a migration of a copied version-1 database. Specify preflight checks, preserved fields, interruption handling and recovery. Do not implement “migration” by deleting the original file or pretending an unrecognized schema is compatible.

## HRIS-02 — employee status filter

Accept Active, OnLeave and Terminated as explicitly validated choices. Omission means all statuses. Decide whether matching is case-sensitive and document the choice. Apply the predicate in [HrisApi.Reads](../../server/Nethris.Api/HrisApi.Reads.cs) before the item-page and count queries. Add a browser control that resets offset and participates in request-generation protection.

Acceptance: a matching employee beyond the first unfiltered page appears; total reflects the filter; stable ordering survives equal names; zero matches is a valid page; invalid input is rejected; an older response cannot replace a newer filter's results. The selector's own visible value is insufficient evidence that the list query is correct.

## HRIS-03 — searchable manager selection

The current [allRows](../../client/src/workshopApi.ts) helper loads bounded pages and refuses a selector that exceeds its supported maximum. Design a server-backed manager search rather than silently truncating options. Define query length, page size, display label, selected-value loading and the meaning of a no-results state.

Acceptance: a selected manager remains identifiable after a new search; older responses cannot replace newer intent; unavailable or terminated candidates are handled according to a documented policy; the server still validates relationship rules and manager cycles on save. A UI search cannot replace the domain's relationship invariant.

## HRIS-04 — review a password-change proposal

Write a design review for a self-service password change that verifies the current password and decides which sessions are revoked. Keep it separate from employee editing and Admin account reset. Identify which writes belong in one transaction and which data must never enter audit details or response receipts.

Then analyze the case where the password changes, the current session is revoked and the response is lost. The same authenticated retry route may no longer work. State an honest recovery interaction instead of copying the ordinary receipt recipe blindly. This is a design exercise; do not claim a production-ready authentication feature from a written flow.

Each ticket submission includes a source map, contract table, accepted path, rejected path, state-preservation proof and one limitation. Use the [solution guide](05-solutions-and-review.md) only after making those decisions yourself.

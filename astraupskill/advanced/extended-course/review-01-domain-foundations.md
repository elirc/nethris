# Review guide 1: domain foundations

Use this guide after attempting chapters 01-08. The worked answers explain what a strong submission establishes and how to distinguish plausible wrong interpretations. They do not report that your experiments have run. Keep your prediction notebook and add observations only after executing the relevant controlled check.

## One operation, several acceptance stages

For NH-01A, a department rename begins with a captured client operation and a DepartmentInput body. An existing department requires an ETag tied to its identity and loaded version. The dispatcher obtains a current authorized actor and enters the ordinary receipt path. SaveDepartment requires HR, normalizes the name, loads the existing department, checks version, checks uniqueness and leadership conditions, assigns values, marks version modified, saves, and returns a view. The outer handler creates audit and receipt, saves those and commits before sending the response.

The important correction is that the domain save does not end the outer transaction. A late receipt failure can roll back the rename. A client response failure after commit can leave the rename accepted while the client is uncertain. Your trace should contain both counterexamples. They demonstrate why the same word failure can refer to different sides of the acceptance boundary.

For NH-01B, employee identity answers which employee; version answers which accepted representation the editor observed; operation key answers which logical mutation the client is attempting or retrying. A rename or location edit preserves employee identity. A successful accepted edit advances version through persistence preparation. An exact retry preserves operation key and request bytes. A new deliberate edit creates a new key even if the visible values happen to resemble an earlier operation.

For NH-01C, the transport error's status zero does not prove rollback. Exact retry asks the server to find the current user's receipt for the original key and fingerprint. If the first request committed, replay returns its stored accepted result. If it did not, a fresh execution can still be subject to current preconditions and domain rules. The client should preserve uncertainty until a supported observation resolves it.

## Shape versus business validity

For NH-02A, malformed JSON, unknown properties and unsupported enum representations belong to parsing or serializer-contract behavior. A nonexistent department is a relationship validity issue. Duplicate email is a domain/database uniqueness issue. A stale valid precondition is a version issue. These cases require different fixtures. Supplying an unknown property in every negative request would stop them all before the domain rules and falsely inflate coverage.

A useful matrix includes a column called earliest intended boundary. It also lists the valid prerequisites needed to reach that boundary: authentication, operation key, media type, size, body shape and current references. If an observed error comes from an earlier stage, repair the fixture rather than celebrating a generic rejection. Negative tests are strongest when they can explain why the candidate was otherwise acceptable.

For NH-02B, a client employee card should retain stable identity and version separately from labels. A nullable PTO balance should display a restricted or unavailable representation according to the API's meaning, not default to zero. The department name can become stale independently of the employee's DepartmentId. A changed label does not automatically mean the employee relationship changed or that the employee version must advance.

For NH-02C, a compatible description addition needs explicit omission semantics. If an older client omits the new field and the server treats omission as null, a full update can clear an existing description. One acceptable learning-project policy is coordinated deployment with a documented fresh contract. Another is an explicit patch or presence-aware representation. The answer should choose and test one; merely adding a nullable property leaves the ambiguity unresolved.

## Normalization examples with invisible characters

For NH-03A, Text first trims. Null, empty and whitespace-only values are rejected. An ordinary word is returned after surrounding whitespace removal. Tabs or newlines at the edges can disappear during trimming, while an embedded tab or newline remains a control character and is rejected. The maximum applies to the normalized string. OptionalText changes the null/empty/whitespace-only rows to accepted null and otherwise delegates to Text.

Use an escaped input ledger such as leading-tab plus Ops plus trailing-tab, rather than a screenshot in which tabs are invisible. The accepted representation is Ops under this rule. Ops followed by an embedded tab and East is rejected. A raw value containing many surrounding spaces and a short accepted word can pass the field-length rule after trimming, subject to the separate overall request-byte limit. These examples distinguish order of operations clearly.

For NH-03B, emails with only case or surrounding-space differences normalize to the same lowercase trimmed value and should conflict under uniqueness when assigned to different employees. Employee-number comparison also has case-insensitive database collation, while its text normalization trims. Do not infer that all Unicode comparison or every field in the database follows the same policy. The source establishes the specific configured columns and helpers.

For NH-03C, deleting every control character would accept a different transformed string where the current rule rejects embedded controls. That is a behavior change. It can also create collisions or obscure what the user actually submitted. A clear field-specific normalizer is easier to reason about than a generic sanitizer with hidden transformations. If your proposed description deliberately allows line breaks, it needs a different documented contract and rendering tests rather than an inaccurate claim about OptionalText.

## Exact quantity worked table

For a PTO request, negative values and zero fail the positive range. One quarter becomes one unit; one half becomes two; 1.25 becomes five. The value 1.2 fails because multiplication by four is not integral. Exactly 240 becomes 960 units; above 240 fails. Employee balance allows zero and reaches ten thousand hours, represented as forty thousand quarter units. These outcomes come from both increment and caller-specific range checks.

For salary, 123.45 becomes 12,345 cents. A three-decimal value such as 1.005 is rejected rather than rounded into an accepted amount. Equal minimum and maximum can satisfy the band relationship if each endpoint is valid. An inverted band fails even when both amounts individually satisfy precision and range. The database constraint operates on cents, so ten billion stored cents corresponds to the one-hundred-million amount maximum.

For NH-04A and NH-04B, the best test uses literal expected unit counts from the table. Computing expected output by calling the same helper defeats the independent oracle. A deliberately truncating conversion should be caught by 1.2 hours or another unsupported fraction. A round-trip property should generate only accepted increments when the claim is exact preservation; arbitrary decimals belong in rejection testing.

For NH-04C, minutes and hours cannot be distinguished reliably by magnitude. Eight is a valid-looking value in either unit. An explicit field or versioned request contract resolves the ambiguity. The migration must account for stored representation, client conversion, bounds, response meaning and historical interpretation. Renaming a label while keeping arithmetic unchanged would be a serious unit error.

## Manager graph worked examples

For NH-05A, a root with null manager terminates immediately. A chain C to B to A to null visits each identity once and succeeds within depth. A self-reference fails because the starting employee is already in the visited set. A cycle A to B to C to A fails when A is encountered again. A manager identity missing from the dictionary fails existence after it is visited. An acyclic overlong chain fails the separate depth guard.

Count manager links carefully. The visited set includes the employee, and the guard compares against maximum depth plus one. A diagram that counts the employee as a manager can create an off-by-one expectation. Use explicit labels for employee node and each manager level in the boundary fixture. The source's supported maximum is thirty-two manager levels, not an unrestricted graph traversal.

For NH-05B, attach a subtree root under a nontrivial upper chain. The root's own path can remain short while a deep descendant's path becomes too long. The full-map validation catches this because it checks each employee after applying the proposed edge. An incremental algorithm would need the edited employee and all affected descendants, plus reliable knowledge of the proposed ancestor chain. Checking only the edited row is not equivalent.

For NH-05C, validating before recursive organization projection protects against cycles and missing references in the loaded graph. Fetching maximum plus one detects an oversized dataset rather than silently truncating it. The resulting roots and child lookup are meaningful only after those invariants hold. A graph that cannot be safely represented should produce an explicit refusal rather than a partial chart that looks complete.

## Reference lifecycle answers

For NH-06A, an active reference can be retained or newly selected if other rules pass. An inactive reference can be retained when it is already the employee's assignment, but it is rejected for a new assignment. A nonexistent reference fails regardless of display text. The form should include the existing inactive identity in its options while excluding unrelated inactive choices. This paired behavior is more precise than active-only filtering.

For NH-06B, the existing workflow clears or changes department leadership before moving the employee. Each accepted step has its own version and possible conflict. If another HR user changes the department between load and save, the stale precondition should trigger review. A proposed atomic transfer would need both relevant expected versions and one transaction covering the combined relationship changes. It is a new command with a clear value proposition, not merely a hidden bypass.

For NH-06C, historical approval references can block employee deletion even when the employee has no current direct reports. The deletion predicate covers history as well as present structure. A proposed archival model should define how historical names are displayed. Current related names and captured historical names are different data strategies; the latter cannot be reconstructed perfectly from current state after arbitrary renames.

## Visibility and calendar review

For NH-07A, PTO balance is visible to self, current direct Manager under the specific branch, HR and Admin; an unrelated Manager or ordinary other Employee receives the null representation. Salary bands require HR or Admin under the job-role projection. Use a distinctive stored number and an allowed control to prove actual redaction rather than missing fixture data. The response body is the authority for exposure, not a hidden column in a screenshot.

For NH-07B, session clearing must invalidate pending reads as well as clear visible cached state. Otherwise an old authorized response can appear in a new viewer's context. For NH-07C, a sensitive field requires review of every serialization path, including exports and errors, not only its form control. The exact scope should remain small enough to verify directly.

For NH-08A, a same-day range spans one inclusive date and permits up to eight hours under the workshop rule. Thirty-one dates means a day-number difference of thirty. The independent 240-hour maximum still applies. A start before today or beyond the AddYears boundary fails. The helper uses a calendar-year operation, so use the installed runtime's documented behavior for leap-day edge cases rather than substituting a fixed duration.

For NH-08B, touching at one endpoint date counts as overlap. Rejected and cancelled requests do not participate in the same active overlap set, while approved requests do. For NH-08C, a working-day extension needs a calendar identity or policy history sufficient to explain old accepted charges. A weekday-count helper alone does not settle holidays, later calendar edits or already-approved history. Preserve the current simplified rule as the baseline and label the extension clearly.

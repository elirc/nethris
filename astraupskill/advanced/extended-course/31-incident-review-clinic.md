# 31. Incident review clinic

The following fictional incidents combine several source boundaries. They are teaching scenarios, not reports of failures in the user's database. For each, write a causal timeline and a discriminating observation before reading the review discussion. Use the actual workshop rules rather than importing assumptions from a different HR system.

## Incident A: the manager can see a person but cannot approve their request

A Manager sees an employee in the directory and assumes that every PTO request for that employee is available for approval. The directory and PTO workflow have different visibility and authority rules. The PTO query for Manager actors requires both assigned-manager identity and current direct-manager identity. The decision path adds stage, self-review, version and history conditions.

Start by identifying which screen the observation concerns. Directory visibility does not establish PTO visibility. Then record the employee's current manager, the request's assigned manager, the actor's current role, the request status and its approval history. A role label alone is insufficient. If the employee's manager changed after submission, the old assignment can remain while current authority no longer matches.

**NH-31A:** Build two fixtures: one where the actor is the assigned current manager and one where the current relationship changed. Keep the request version and stage valid in both. Predict visible query results and direct decision responses. Explain why a client-hidden button is not the only evidence needed; a direct request must also be refused where appropriate.

Review discussion: the correct response may be cancel and resubmit under the current relationship, as the existing decision message explains. A proposed automatic reassignment feature would need a policy for decisions already recorded. Do not repair the incident by changing only the displayed manager name or by granting every Manager access to every request.

## Incident B: an inactive department disappears from an employee editor

A contributor simplifies department options to include only active departments. An employee already assigned to an inactive department then opens for an unrelated phone edit. The selected identity is absent from the options, and the form appears blank or chooses another value. The backend permits retaining the existing inactive assignment, but the interface no longer represents it correctly.

Trace the original selector predicate: active records or the current selected identity. The second branch preserves a valid existing relationship. A new searchable selector must retain the same meaning even if the selected record is outside the current result page. The domain distinction between old and new assignment should guide UI design.

**NH-31B:** Write a regression test with an inactive current department and a valid phone change. Assert that the original department identity remains selected and submitted unless the learner deliberately changes it. Add a new-employee case proving the inactive department is not offered as a new assignment. These paired cases reject both over-restrictive and over-permissive fixes.

Review discussion: the bug lies in representation of an allowed state, not necessarily in the server rule. Making the server reject all inactive references would hide the interface problem by breaking another supported workflow. Preserve the domain contract and repair the selector's selected-value handling.

## Incident C: two successful-looking approval responses deduct twice

A new client retry helper creates a fresh operation key whenever fetch fails. After a committed final approval loses its response, the helper sends another logical operation instead of replaying the first. Depending on domain state, the second request may conflict or reveal another implementation error; it cannot retrieve the original receipt by the old key because the key was discarded.

Inspect captured operation identity before assuming the server's receipt protocol is broken. Record method, path, query, If-Match and body representation for both requests. The same visible form does not prove the same fingerprint. If the new helper reserializes changed fields or refreshes the ETag, it has changed the request's meaning as well as its key.

**NH-31C:** Construct a controlled lost-response case and compare a correct exact retry with the proposed fresh-key helper. Inspect accepted status, balance, approval count, audit count and receipt identities. The goal is to show which protocol was exercised, not to assume every fresh-key retry necessarily succeeds. A closed-request conflict can still be a worse recovery experience than receiving the original accepted result.

Review discussion: retain the captured operation for uncertainty and create a new key only for a new deliberate intent. The server's once-only behavior must be assessed through business effects. Matching response text alone is not an accounting proof, and a conflict after a lost response does not by itself tell the user whether their first action succeeded.

## Incident D: a validation error appears after tracked state changed

A debugger stops inside a PTO decision after status has been assigned but before a validated approval comment is accepted. The comment is invalid, an exception occurs, and the developer sees the changed status on the tracked object. They conclude that the failed request partially approved leave.

The observation is real but its scope is in-memory state inside a request. The decisive question is whether accepted storage changed after the outer transaction ended. The domain can mutate tracked objects before a later validation or save failure; transaction rollback prevents those attempted changes from becoming accepted database state. A fresh context after failure is the appropriate observer for durability.

**NH-31D:** Design a fixture with an invalid embedded control in the decision comment and otherwise valid authority, version and workflow state. Observe the thrown error and then read request status, balance and approvals through a fresh context. If you instrument internal state, label it separately. Do not use the failed context's tracked entity as the sole persisted-state oracle.

Review discussion: an in-memory mutation can still be a design concern for reusable services, but it is not identical to a committed partial transaction. A precise review can recommend earlier validation for clarity while preserving the evidence that the outer transaction protects accepted state. Separate those two claims rather than choosing one broad verdict.

## Incident E: a status filter reports no people on page four

An HR user is on a later directory page, applies a narrow proposed status filter, and sees no items even though matching employees exist. There are two common causes to distinguish. The client may have retained the old offset, or the implementation may have filtered only the already-paged results. Both can produce an empty screen, but their fixes differ.

Inspect the request URL and response total. If the server returns a nonzero filtered total with an offset beyond the result set, reset offset when the applied filter changes. If the server total remains unfiltered and the browser removes rows afterward, move the predicate into the server query before Count and paging. Use a fixture with known matches on multiple original pages.

**NH-31E:** Write two separate regression tests, one for query composition and one for client offset reset. Add a delayed old-filter response to test generation ownership. Assert the applied control value, total and item identities together. A test that checks only that the dropdown changed can pass while the displayed population is wrong.

Review discussion: pagination bugs often arise from confusing a subset with the full query. Stable ordering, accurate totals and explicit applied-filter state make the relationship visible. Avoid fixing the symptom by automatically fetching every employee and filtering locally unless you deliberately redesign and verify the whole contract.

## Incident F: a schema feature works only on a fresh database

A department-description implementation passes tests that create new databases, but an existing compatible workshop file fails because its table lacks the new column. The contributor assumes EF will update it automatically on startup. The actual HrisDatabase constructor creates schema only for a new file and checks markers for existing files.

Review the feature's compatibility claim. Fresh-database testing can prove the new model works in a new workshop, but it does not establish migration of existing data. The release needs either an explicit fresh-dataset limitation for the learning exercise or a reviewed migration with preservation tests. Updating only the model and leaving the old version marker unchanged can misrepresent compatibility.

**NH-31F:** Create an old-schema synthetic database, preserve its hash, and rehearse the chosen upgrade path on a copy. Verify department identities, employee references, settings and private signing material are preserved as intended. Report the exact supported source and destination versions. Do not repair the original file by deleting it or manually changing markers without performing the schema conversion.

Review discussion: schema identity is a promise about representation. A feature that changes stored shape must either honor that promise through migration or clearly reject unsupported files. Both are more trustworthy than silently resetting learner data to make a demo appear successful.

## Incident G: all tests pass, but the report is misleading

A report says the feature is fully verified, yet it ran only pure normalization tests. The feature also changed a persisted field, an HTTP contract and a stale-draft interface. The tests may be correct and useful while the conclusion exceeds their scope. The remedy is accurate accounting and targeted missing evidence, not discarding the passing results.

**NH-31G:** Rewrite the report to state which helper cases passed, which persistence and HTTP checks remain, and which browser interaction needs verification. Propose the smallest meaningful additional checks. A round-trip database test, a direct invalid request and a stale-form recovery journey may address the actual gaps more effectively than running unrelated suites repeatedly.

Review discussion: truthful scope improves collaboration. It lets reviewers assess risk without guessing what all tests means. Preserve passed, failed, setup-blocked and not-run as separate outcomes, and connect each to the feature claim it supports. Confidence should follow evidence rather than replace it.

## Clinic handoff

Submit three incidents from different layers with predictions, controlled observations, revised explanations and a narrowly scoped recommendation. Include at least one case where the implementation follows its current contract and the misunderstanding lies in interpretation or documentation. Diagnosis is the skill being assessed: identify the observer, trace the boundary, and change only what the evidence supports.

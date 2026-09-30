# Review guide 4: delivery and investigations

This guide supports chapters 25-31. It focuses on choosing evidence that matches a claim, preserving existing work, and avoiding fixes that merely hide a symptom. Use it after writing your own failure matrices, capstone ledgers and review comments.

## HTTP boundary reasoning

For NH-25A, declared-size rejection and streamed-size rejection are separate cases. The helper checks ContentLength when present, then counts bytes during reads. An exact-limit body may pass size handling and fail later JSON or domain validation; report that distinction. An oversized body with malformed JSON can establish the earlier size refusal only if the experiment confirms that is the boundary reached.

Strict UTF-8 decoding rejects invalid sequences rather than silently substituting replacement characters. A decoded string can still fail JSON parsing. A parsed object can still contain unmapped fields or invalid enum representations. A structurally accepted request can still fail domain relationships. The stage matrix should name all four distinctions so a generic 400 is not treated as proof of every rule at once.

For NH-25B, the implementation accumulates body bytes and then decoded text; it is bounded by a small request limit rather than processing arbitrary input with constant memory. A resource measurement must identify which representation and interval it observes. The workshop's loopback restrictions and body bounds are concrete source facts, but they do not constitute a general production security or capacity certification.

For NH-25C, known invalid status input should invite correction, stale edits should invite comparison, missing references should invite a refreshed selection, and uncertain saves should invite exact replay or deliberate inspection. A message should not say nothing was saved when the server or client lacks that certainty. Conversely, a known accepted mutation followed by failed refresh should retain its saved status in the explanation.

## Performance decisions

For NH-26A, choose a path and a valid workload shape. Employee count alone does not characterize manager-chain validation, and page count alone does not characterize reference loading. Record environment, build mode, data size, graph depth or distribution, and repeated observations. A proposed measurement is useful but should not be presented as executed evidence.

For NH-26B, full-reference loading offers a simple complete local set within limits, while searchable selectors reduce initial transfer and scanning at the cost of more asynchronous states. The comparison should include selected-value resolution and inactive-current-reference behavior. A selector that is faster because it omits valid choices is not equivalent. Usability benefits can justify change even when raw timing is acceptable, but they need observed user tasks.

For NH-26C, an incremental manager check can be compared with the simpler full-map oracle over controlled graphs. The affected set includes descendants whose paths change, not only the edited employee. Preserve missing-reference, cycle and depth rules. Generated cases should vary meaningful graph structure rather than only relabeling identical shapes. Report the finite tested domain and avoid claiming a mathematical proof unless you actually provide one.

## Reproducibility and artifact identity

For NH-27A, the setup card should name the real root, declared runtime requirements, package and start routes, disposable database configuration and relevant test command. It should distinguish instructions from commands actually run. The browser and API paths can use different generated artifacts or hosting arrangements, so record which one produced the evidence.

For NH-27B, hashes help identify exact files but do not prove semantic correctness. Pair a generated-asset hash with the behavior observed from that asset. Preserve unrelated local edits and state whether the source tree was clean or contained existing changes. Do not claim a committed release when the delivery consists of local files only.

For NH-27C, a missing dependency before tests start is a setup limitation, not failed assertions. An occupied browser port is not evidence of a product regression. Keep successful build or helper results while identifying the unexecuted scope. Then connect that gap to the feature: a stale-draft UI change still needs interaction evidence, while an unrelated pure rule may already have sufficient targeted coverage.

## Searchable selector capstone

For NH-28A, the acceptance matrix should cover search results and selected-value identity separately. A selected value outside the first page remains represented. An existing inactive department is retained but not generally offered for new assignment. Equal labels remain distinct identities with enough allowed display context to choose correctly. A missing selected record has explicit feedback. An old search result cannot overwrite a newer query or new session.

For NH-28B, implement one reference kind end to end before extracting a generic component. Departments, job roles and managers differ in labels and eligibility. A manager selector can filter self and terminated people locally but still relies on server graph validation. Shared loading and keyboard mechanics may be reusable, while domain predicates should remain explicit enough to inspect.

For NH-28C, compare both performance and completeness. Count requests and records transferred under a known dataset, but also prove that every valid choice can be found and the selected identity survives query changes. A new control should not silently clear a reference because its search page is empty. The mutation body must capture the final chosen identity once for exact retry.

## PTO capstone expected ledger

For NH-29A, start with a known balance and valid manager routing. Creation produces a pending request, reservation capacity and accepted identity/version without deducting stored balance. A stale edit with a valid old ETag conflicts and preserves accepted fields. Manager approval creates its history entry and moves to PendingHr while retaining reservation. Final approval by a different authorized reviewer deducts once and closes Approved.

After simulated response loss, the client can be uncertain while the database is already accepted. Exact replay returns the stored result and should not advance the workflow, add another final approval or deduct again. The ledger needs both client certainty and server accepted state because those can differ temporarily. That is the point of the recovery scenario, not an inconsistency to hide.

For NH-29B, changed body or precondition under the same key produces fingerprint conflict and no new business effect. A new key after uncertainty loses the direct link to the original receipt. The domain may reject a repeated closed-request decision, but that rejection is not as informative to the client as recovering the original accepted response. Do not equate a protective domain conflict with successful replay.

For NH-29C, approved requests are outside the pending-only cancellation/deletion path. The accepted deduction remains under current workshop behavior. A future correction feature should reference the original history and record a deliberate compensating effect, actor and reason. Editing historical rows in place would erase the basis of prior decisions and needs a different explicit policy.

## Refactoring review answers

For NH-30A, a unit-of-work helper must make context and transaction ownership clear. It should not send success bytes before commit or allow nested independent commits to masquerade as atomic work. A callback that opens another connection is a concrete risk to the guarantee. The review comment should show the failure sequence and the accepted-state invariant at stake.

For NH-30B, shared plumbing such as precondition parsing, response serialization and receipt orchestration can be extracted without hiding domain meaning. Employee manager graphs, department leadership and PTO reservation checks are not interchangeable CRUD decoration. A generic abstraction with many flags and opaque callback ordering may make those rules harder to review. Reduced line count is not the only measure of maintainability.

For NH-30C, choose tests based on the changed boundary: role gate, exact replay, transaction rollback, stale ETag, projection and query order. Preserve before-and-after evidence under the same environment where possible. If a test was already failing, report that separately. A structural change should not accidentally alter status codes, fingerprint inputs or JSON property names unless that contract change is deliberate and reviewed.

## Incident clinic reasoning

NH-31A is resolved by separating directory visibility, PTO visibility and decision authority. Record assigned and current manager identities, role, status and history. A former manager's refusal can be correct under the current policy. NH-31B concerns the UI's ability to represent a valid existing inactive reference; rejecting all inactive references on the server would break the supported maintenance path rather than fix the selector.

NH-31C asks whether the client preserved the original operation. Compare bytes and key before blaming the receipt protocol. A fresh-key retry can receive a conflict after the first operation succeeded; that does not prove the first failed. NH-31D separates tracked mutation from accepted storage. Fresh post-failure reads decide the durable outcome, while earlier validation can still be proposed as a clarity improvement.

NH-31E distinguishes stale offset from filtering after paging. Inspect request URL, total and identities to choose the right fix. Add a generation test so an old response cannot reintroduce the wrong population. NH-31F is a compatibility gap: fresh-database success does not migrate old schema. Rehearse on a copy, preserve identity and relationships, and report the supported upgrade path explicitly.

NH-31G is a reporting gap rather than proof that the feature is wrong. Preserve the useful pure-test result and identify the missing persistence, HTTP and browser claims. Ask for the smallest meaningful additional checks. Broad all tests passed language should be replaced with named executed scope and accurate limitations.

## A reusable review method

For any new feature or incident, ask five questions. What user intent is being attempted? Which state and actor does it assume? Where is that assumption checked? Which event makes the effect accepted? What observation independently confirms the result? These questions connect the domain, transport, persistence and interface without requiring a universal framework.

Then write one plausible wrong implementation and identify the test that would reject it. Examples include filtering after pagination, accepting a stale whole-record body with a refreshed version, logging audit outside the transaction, reserializing a changed draft during retry, and publishing an old session's response. If no test distinguishes the wrong implementation, your evidence has a concrete gap worth addressing.

Finish the review with a scoped conclusion. State what the evidence establishes, what remains a proposal, and which next experiment would resolve any material uncertainty. That style makes even a small learning project a useful demonstration of engineering judgment.

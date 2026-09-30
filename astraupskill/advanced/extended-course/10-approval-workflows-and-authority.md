# 10. Approval workflows and authority

A PTO decision is a transition with both state and actor preconditions. Read `Save`, `Decide`, `Remove`, and `Visible` in [HrisPto.cs](../../../server/Nethris.Api/Domain/HrisPto.cs). A new request is assigned to an eligible current manager when available; otherwise it begins at HR. A manager-stage approval advances to HR, while final HR approval deducts balance and closes the request as approved. Rejection closes the request without following the remaining approval path.

## State is not enough

Knowing that a request is PendingManager does not establish that any Manager may decide it. The actor must be the assigned manager and still the employee's current direct manager. The actor cannot be the requester. For the HR stage, the actor must have HR authority and must not already have approved this request at another stage. The workflow requires a different final reviewer.

Consider a person with HR role who made the manager-stage decision because they were the assigned current manager. They cannot then make the final HR decision on the same request under the different-reviewer rule. Role rank alone does not settle authorization. The existing approval history is part of the decision predicate.

## Manager changes alter current authority

A request captures an assigned manager, but the decision rule also checks the employee's current ManagerId. If the relationship changes, the former assigned manager cannot continue approving merely because the old assignment remains on the request. The error guidance points toward canceling and resubmitting after a manager change. This preserves a clear workflow rather than silently rerouting an in-progress decision history.

Visibility for Manager actors similarly requires both assigned and current direct-manager relationships. HR or higher and the requester have broader visibility under the specified predicate. Distinguish visibility from decision authority: seeing a request does not imply permission to make every decision. The requester can see their own request but cannot approve it.

## Edit, delete and cancel preserve different history

A pending request can be edited only before any decisions exist. Once review has begun, the requester must cancel and create a new request instead of rewriting the basis of an existing approval. Deletion is also refused when decisions exist, while cancellation retains them. Approved history is outside the pending-only removal path.

This is a useful example of why delete and cancel should not be aliases. Delete removes an eligible unreviewed request; cancel records a closed workflow state while preserving its decisions. The UI and audit evidence should use those terms consistently. A future retention policy is a separate feature with different requirements.

## Exercise NH-10A: build a transition table

For each status, list allowed requester edits, requester deletion, requester cancellation, manager decision and HR decision. Include the actor and history conditions in each cell. A simple yes/no matrix without those conditions will be misleading for PendingManager and PendingHr. Use a second table if necessary to keep the state table readable.

Add examples for self-approval, unrelated manager, former manager, assigned current manager, HR final reviewer who already approved, and a different HR reviewer. Keep the request version current when testing authority so a stale-version error does not mask the rule you intend to examine.

## Exercise NH-10B: trace a rejected final decision

Start with a request ready for HR and deliberately make its employee balance insufficient in an isolated fixture consistent with the investigation you are performing. Predict the rejection and inspect request status, approvals and balance after the transaction. The test should show that no partial final approval is accepted.

Now consider an invalid optional decision comment. The domain method assigns some tracked state before constructing the approval with its validated comment. Explain the difference between an in-memory entity mutation and an accepted database change when an exception occurs before commit. This distinction is why the outer transaction and fresh post-failure reads matter to your evidence.

## Exercise NH-10C: propose delegation

As a future feature, allow a manager to nominate a temporary delegate. Define the delegation interval, scope, self-approval restriction, current-relationship requirement and final-reviewer separation. Decide whether a request captures the delegate at submission or evaluates delegation at decision time. Each choice has consequences when assignments change.

Write a scenario where a delegate's authority expires between the first accepted decision and an exact retry. Explain how replay authorization should be evaluated under your proposal. A stored receipt is not an excuse to return sensitive or newly forbidden results without considering the current authority contract. The existing receipt chapter provides a concrete starting point.

## Review standard

A strong workflow model combines status, actor, current relationships, captured assignment, version and history. It preserves the distinction between visibility and permission, and between deletion and cancellation. Your final evidence should include a complete successful two-stage path plus rejected self-review, changed-manager and duplicate-reviewer cases.

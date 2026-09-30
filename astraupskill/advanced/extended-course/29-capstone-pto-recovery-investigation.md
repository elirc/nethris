# 29. Capstone: PTO recovery investigation

This capstone uses the existing workshop behavior rather than adding a new policy. Your task is to demonstrate one PTO request from creation through two-stage approval, including a stale decision, lost response, exact replay and accounting verification. Use a fresh fictional database and accounts created by the test setup. The final deliverable is an evidence package another learner can reproduce.

## Establish the scenario

Create a requester with a known balance, an eligible current direct manager, and a different HR reviewer. Record their stable employee and account identities using synthetic labels. Confirm that the manager relationship and account roles satisfy routing requirements. Choose a valid future date range and quarter-hour quantity. Do not rely on seed data without recording which relationships it establishes.

Before sending the request, predict its initial status and assigned manager. If no eligible manager account exists, the request starts at HR instead and no longer exercises the intended two-stage path. A fixture that accidentally bypasses the manager stage cannot support claims about manager approval. Verify the starting state before continuing.

## Record acceptance, not only responses

Capture the create operation's key, body representation, accepted response identity and version, audit evidence and receipt metadata. Avoid recording authentication tokens or passwords in the shared package. The request's hours become pending reservation capacity; the stored balance is not yet deducted. Read accepted state through a fresh observation and include it in your ledger.

Now attempt an edit with a stale version using an otherwise valid request and a fresh operation key. Predict the conflict and unchanged accepted PTO fields. Preserve the rejected draft in the client or test artifact. This case establishes precondition enforcement independently from later decision authorization.

## Exercise the manager stage

Have the assigned current manager approve the request. Record the transition to PendingHr, the new approval history, the version change and the unchanged stored balance. Then attempt a final decision by the same person when the scenario would otherwise give them sufficient role. Predict the different-reviewer restriction. A separate unrelated Manager attempt can establish the current-assignment requirement, but keep it distinct from self-approval.

If you choose to investigate manager reassignment, use a separate request or reset to a documented synthetic state. Changing the manager midway alters visibility and decision authority. Do not casually mix that variation into the main path and then wonder why the original manager can no longer see the request. The evidence should retain a single clear causal sequence.

## Simulate lost final response

Let the different HR reviewer perform the valid final approval, but arrange for the client not to receive a usable result. Preserve the exact captured operation. At this point, the client may show uncertainty even though accepted storage contains Approved, one final approval entry, a balance deduction and a receipt. State whether your simulation discards a known successful response or actually interrupts delivery at a controlled point.

Retry the same operation key, body, route and precondition. The replay should return the stored result without another deduction. Compare accepted balance and approval count before and after replay. Also inspect whether another ordinary mutation audit row was added through the fresh-execution path. The central claim is one accepted business effect, not merely two matching JSON bodies.

## Exercise NH-29A: construct the evidence ledger

Use columns for action, actor, expected version, operation key label, accepted status, stored balance, pending reservation total, approval count, audit effect and client certainty. Fill predicted values before each action. After execution, add observed values and explain any difference. Do not replace predictions after the fact without preserving the original reasoning.

The ledger should show that manager approval keeps the request pending and reserved, while final approval changes stored balance and removes it from the pending subset. An exact replay should not repeat those transitions. A stale rejected edit should not change the accepted record. These are independent facts that together establish the workflow.

## Exercise NH-29B: add a changed-request counterexample

Reuse the final operation key with one changed body value or precondition in a separate request. Predict a fingerprint conflict. Verify no new decision or deduction occurs. This negative control proves your replay test is sensitive to request identity and is not merely accepting all repeated keys.

Then explain why creating a new operation key after an unknown result is unsafe as an automatic retry strategy. The new key represents a new intent and cannot retrieve the old receipt by identity. Domain state may reject it, but the protocol no longer provides the same direct reconciliation mechanism.

## Exercise NH-29C: demonstrate historical protection

Attempt requester deletion or cancellation after approval. Predict the pending-only refusal and preserved approved history. Keep this separate from a proposed refund or administrative correction feature. The workshop does not implement arbitrary reversal of approved deductions through the same remove path.

For an extension proposal, describe how an explicit correction would record actor, reason, accounting effect and relation to the original request. Do not silently edit the original approved row and call that an audit-preserving correction. A different business command needs its own history and retry semantics.

## Presentation and assessment

Prepare a walkthrough that starts with the user's uncertainty after response loss, then shows how operation identity and atomic receipt resolve it. Use the ledger to explain the domain transition and the source links to justify the boundaries. Include the stale edit, changed-fingerprint rejection and historical-protection case as counterexamples.

Assess twenty points each for fixture validity, state/accounting predictions, version enforcement, exact replay evidence and reproducibility. A submission without fresh accepted-state observations cannot receive full accounting credit. A submission that changes request bytes during retry cannot claim to have exercised the exact-replay contract. The final package should make every accepted hour and approval explainable.

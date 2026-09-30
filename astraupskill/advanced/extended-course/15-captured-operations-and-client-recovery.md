# 15. Captured operations and client recovery

Read `operation` and `request` in [workshopApi.ts](../../../client/src/workshopApi.ts), then `submit` and `perform` in [Workshop.tsx](../../../client/src/Workshop.tsx). The client separates editable form state from a captured operation. Once submitted, the operation contains serialized body, path, method, key and precondition. Pending recovery reuses that object instead of rebuilding the request from whatever the form currently contains.

## Capture intent once

The form draft stores strings and booleans. Submit converts number fields, optional blanks, checkboxes and action-specific bodies into a request candidate. It derives the path and method from entity, existing row and action. It then creates one operation. This is the boundary at which editable intent becomes an exact retryable request.

For an existing save, the operation contains an ETag based on the row originally opened. For deletion, the body is omitted. Decisions and cancellations use action routes and their own small bodies. A generic retry implementation that serializes the current editor again can change body bytes or preconditions and conflict with the original key. The captured object prevents that accidental drift.

## Pending is different from busy

Busy describes an operation currently being awaited. Pending can remain after an uncertain outcome even when the interface is no longer actively waiting. The client locks ordinary editing while a pending operation exists and offers exact retry or an explicit stop-and-inspect path. This prevents a new similar mutation from being issued casually while the first one's acceptance is unknown.

The in-flight ref synchronously guards entry to perform, while React state communicates busy status. These roles complement one another. A rendered disabled button helps the user; the ref protects an immediate second call before a render catches up. Do not remove either mechanism based solely on a preference for fewer state variables without analyzing the transition it controls.

## Error classes determine recovery

Transport errors and unreadable responses are represented as status zero by the request helper. Server errors at five hundred or above also leave the pending operation available. Known lower-status rejections clear pending, while a 409 on an existing record can initiate comparison with current saved values. A 401 clears the session workspace. These are different outcomes with different next actions.

After a mutation response succeeds, the client clears pending and editor and reports saved success before refreshing lists. If refresh then fails, it says the change was saved but refresh failed. This is an important distinction: a failed follow-up read should not transform a known accepted mutation into an unknown save. The nested error handling preserves the stronger fact already established by the successful response.

## Exercise NH-15A: draw a client state machine

Include editing, submitting, accepted-awaiting-refresh, refreshed, known rejection, stale comparison, uncertain-pending and signed out. Label which transitions retain editor text, captured operation and session. Use the actual branches to justify the diagram. A single error state is too coarse to describe the recovery controls accurately.

For each state, identify the enabled user actions. During uncertain pending, an ordinary fresh submit should not replace exact retry. During stale comparison, the learner must review current values before deliberately adopting the new version. After accepted-but-refresh-failed, retrying the mutation as a new intent would be the wrong default response.

## Exercise NH-15B: simulate response loss

Create a controlled request stub or isolated HTTP scenario that accepts the mutation but returns an unreadable or interrupted response to the client. Assert that pending retains the same operation key, body and ETag. Then provide a successful replay response and verify the interface clears recovery state. Inspect the request values across attempts rather than only counting fetch calls.

Include a contrast case where the server returns a known validation rejection. Pending should clear so the learner can correct the draft. The form should retain relevant input. This comparison demonstrates that the client distinguishes uncertainty from a definite rejection instead of treating every error as retryable with the same bytes forever.

## Exercise NH-15C: test successful save with failed refresh

Let the mutation succeed, then reject the follow-up list or reference request. Predict the success notice and explanatory refresh error. Verify that no pending exact mutation remains merely because the read failed. Then allow a manual refresh and confirm the accepted record appears.

Your report should name which network call failed. A browser trace with one red request can be misleading if the earlier mutation succeeded. A causal action log explains why the interface truthfully reports accepted change alongside a later loading problem.

## Review standard

A strong answer separates current form, captured request, pending uncertainty, active wait and accepted result. It proves retries preserve bytes and key, and it keeps follow-up read failures from erasing known acceptance. The final artifact should be a state diagram and three controlled recovery journeys with exact request observations.

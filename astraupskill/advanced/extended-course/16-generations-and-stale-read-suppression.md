# 16. Generations and stale-read suppression

The Workshop client can issue overlapping reads when the session, tab, page or applied search changes. A slower old response must not overwrite the newer view. Read `refresh`, `clearWorkspace`, and the associated effect in [Workshop.tsx](../../../client/src/Workshop.tsx). The implementation uses an incrementing load generation to decide whether a response still belongs to the current request context.

## Walk a two-request timeline

Refresh A captures generation one. Before it completes, refresh B increments the shared counter and captures generation two. When A resumes, it compares its generation with the current counter. Because they differ, A returns without publishing its old result. B can publish only if no later refresh or clear has advanced the counter again.

This is relevance checking, not cancellation. The old network request may still complete, but its result is not accepted into the current view. A cancellation mechanism could save resources, yet it would not remove the need to reason about results that race with cancellation. The generation guard gives a clear local rule: only the current generation may publish.

## Session clearing invalidates reads

ClearWorkspace increments the generation before clearing cached pages, references, organization data, editor, pending operation and comparison state. A response from the prior session then fails the generation check instead of restoring old data after sign-out. The ordering matters. Clearing visible state without invalidating the old request would allow it to repopulate the interface later.

Use an account-switch scenario to make the consequence concrete. An HR user begins loading data, signs out, and another employee signs in. The old HR response may contain fields inappropriate for the new viewer. Server authorization was correct for the old request, but its late display would be wrong for the current session. Client relevance and server authorization protect different moments in the workflow.

## Refresh has multiple stages

The refresh first loads the current actor through me. It checks generation before using that result and evaluates whether the selected tab remains allowed. It then loads the selected data plus department, job-role and employee reference collections together. After those complete, it checks generation again before publishing session, references and page or organization data.

Those two checks are important because the request can become obsolete during either stage. A single check before the first await would not establish relevance after later awaits. The catch path also suppresses errors from obsolete generations so an old failure does not overwrite the current view's feedback. Read the exact placement rather than assuming one counter automatically protects every asynchronous branch.

## Exercise NH-16A: control completion order

Build a test plan with deferred promises for two refreshes. Resolve B before A and assert that B remains visible after A completes. Repeat with A failing after B succeeds and verify that A's obsolete error does not become the current error. Do not rely on real network latency to create the order; control it explicitly.

Include a case where the first me response completes, then a new refresh begins while A's data requests are pending. This exercises the second generation check. A test that only delays the initial actor request covers a different point in the sequence and may miss a misplaced later guard.

## Exercise NH-16B: test sign-out during loading

Start a refresh under session S, invoke clearWorkspace through the supported sign-out or session-forgetting path, then resolve the old requests. Inspect page, references, organization and editor state. The old response should not restore the signed-out workspace. If you use a unit-level harness around extracted logic, say what it does and does not prove about the real component.

Then design a browser-level account-switch rehearsal using fictional accounts. It should demonstrate that the old session's sensitive projection is not displayed to the new user. Record the sequence and controlled response delay. A screenshot captured before the old response arrives would not test the relevant race.

## Exercise NH-16C: review a cancellation proposal

Someone suggests replacing the generation checks with AbortController. Explain which resource-saving benefits cancellation can provide and which relevance question remains. An operation may finish just before cancellation or a caller may fail to propagate the signal. A result should still be checked against the state that owns it before publication.

Propose a combined design if you believe cancellation is useful. Define who creates and aborts each controller, which requests share it, and how aborted errors are classified. Do not let cancellation of one consumer accidentally terminate a shared operation another consumer still needs. Keep the proposal proportionate to the observed workload.

## Review standard

A strong answer controls asynchronous order and tests success and error publication after context changes. It distinguishes cancellation from relevance, explains both generation checks, and connects session clearing with protection against stale display. The final evidence should make the race deterministic enough that another learner can reproduce it repeatedly.

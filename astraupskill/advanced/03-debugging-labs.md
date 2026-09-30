# Diagnose rejected and uncertain writes

Use a fresh fictional workshop database and the provided owned-server checks as references. The following tasks describe experiments to perform; they are not new passing test claims. Keep request bodies free of real employee data and exclude passwords/tokens from your evidence notes.

## HRIS-L1 — missing expectation

Read a department, retain its response and attempt a valid update without `If-Match`. Keep a valid session and operation key so the missing expectation is the intended failing boundary. Predict the status, then compare the result with `AnUpdateWithoutIfMatch_Is428` in [ApiScenarioTests](../../tests/Nethris.Tests/ApiScenarioTests.cs).

Check the stored department, audit count and receipt count afterward. Changing several request conditions at once produces ambiguous evidence: an expired session could reject the request before the missing-header code runs.

## HRIS-L2 — stale but well-formed

Read the same department from two clients. Let A save a change, then let B send a new operation with its old valid ETag. Predict 409 from this project's stale-version policy. Record B's draft and the latest accepted value separately. Do not “fix” the failure by replacing B's expectation automatically before the user reviews the current data.

Design a browser assertion that the typed field remains visible after rejection. A server assertion that data did not change and a browser assertion that a draft survived answer different questions; include both when making an end-to-end claim.

## HRIS-L3 — successful write with a lost response

Use the existing [browser check](../../scripts/check_browser.py) as a source for how an owned test environment creates a dropped response. Establish that the first write committed, then retry the exact captured operation. Observe one accepted domain change and a replay response. Change the serialized body while retaining the key and predict the conflict.

Explain why simply clicking Save again with a new UUID cannot recover the original operation by identity. A name-uniqueness rejection may prevent a duplicate row accidentally, but it is not the same contract as recovering the successful response.

## HRIS-L4 — a misleading filter result

For your proposed status-filter implementation, create more employees than fit in one page. Ensure at least one matching employee lies beyond the initial unfiltered page. First filter only loaded browser rows in a disposable exercise variant and explain the incorrect result. Then apply the predicate to the server query used for both items and total.

Test a valid status with zero matches, an invalid status string and a filter changed while on a later page. An empty valid result and an invalid request should not be indistinguishable. Include a stable ordering tie-breaker so paging does not depend on incidental database order.

## HRIS-L5 — failure after an earlier save

Read [check_api.py](../../scripts/check_api.py), which contains an owned HTTP workflow, and locate its receipt-insertion failure experiment. Draw the domain save, audit/receipt save and final commit in order. Predict the surviving rows if the later step fails. Your conclusion should be about the explicit transaction, not a blanket claim that any method calling `SaveChangesAsync` is automatically atomic with every other method.

For every lab, keep a short evidence table: isolated condition, expected boundary, observed status, surviving state, supporting check and excluded claim. This makes a failed experiment useful even when you have not fixed the implementation yet.

# Practice: make a small feature complete

These tasks are exercises, not claims that the features are already implemented. Work in your own practice branch or copy. Preserve a known-good package and use a separate learning database for experiments. Attempt the reasoning and tests before reading [the review guide](06-SOLUTIONS-AND-REVIEW.md).

## Exercise 1 — Read an existing CRUD path

Choose departments. Identify the form fields, serialized input, API dispatch, domain validation, EF entity, output view and audit insertion. Draw arrows between the actual filenames. Then explain why DepartmentView includes headcount while DepartmentInput does not.

Write three example requests: valid create, duplicate name differing only in case, and update with a missing If-Match header. Predict the HTTP status and whether any row, audit or receipt should commit. Run those requests against a temporary workshop and compare the results. Your deliverable is an annotated trace, not new code.

## Exercise 2 — Add a department description

Introduce an optional short description, at most 250 characters without control characters. This deliberately crosses the full CRUD path: entity, input, output, validation, editor and tests.

Start by writing a contract table. Specify whether blank text becomes null and whether an omitted property clears the description or leaves it unchanged. Existing PUT endpoints use replacement-style inputs; keep the semantics consistent. Use HrisRules.OptionalText rather than copying slightly different trimming logic.

Changing the EF entity changes the schema. HrisDatabase identifies schema version 1 and refuses unknown databases; it is not a migration framework. For the exercise, use a new database path and document the schema change. A follow-up migration should be separately designed and tested against a copy. Do not silently make a version-1 file “compatible” by ignoring a missing column.

Acceptance criteria: create displays the description, edit updates it, blank clears it, overlong/control-containing input fails without partial writes, ordinary employees can read only the intended field, and stale edits still preserve drafts.

## Exercise 3 — Add an employment-status filter

The directory already searches names, email and employee number, and the API supports a department filter. Add a status filter with explicit values Active, OnLeave and Terminated. An invalid status should produce a validation error rather than return an empty page silently.

Apply the filter before Count, Skip and Take. Keep the browser page offset synchronized: changing a filter should return to the first page. Test with enough data to span two pages, including a matching employee that was not on the first unfiltered page.

Explain why filtering the twelve currently loaded cards in React is incorrect. Your tests should prove the total count and membership, not merely that a select element exists.

## Exercise 4 — Add a password-change flow

The Admin account editor can reset a password and revoke sessions. Design a separate self-service password-change endpoint that requires the current password and the current authenticated session. Decide whether all sessions, including the current one, are revoked. State the rule in the UI before submitting.

Do not reuse employee update for this feature. Never return a password hash or include passwords in audit details or durable response receipts. Test an incorrect current password, malformed inputs, successful change, old-session rejection and new-password login. Think carefully about lost-response recovery when the operation revokes the session needed to replay it.

## Exercise 5 — Improve reference selectors

The workshop loads bounded employee, department and job-role lists for selectors. At 2,000 employees this remains a teaching-sized design, but a real organization may be much larger. Propose a searchable server-backed manager selector.

Define stable paging, debounced queries, cancellation or stale-response rejection, and how a currently selected manager remains visible when it is outside the current search result. A valid manager is still validated on the server; a filtered dropdown does not enforce the business rule.

## Exercise 6 — Design approved PTO cancellation

Approved cancellation is intentionally absent. Before coding it, define who can request and authorize a refund, which dates remain eligible, how many hours are restored, and what prevents duplicate refunds. Decide whether it needs a new state and immutable refund history.

Write invariants: one approval deduction, at most one matching refund, no negative balance, and the mutation/audit/receipt committed together. Include competing refund requests, stale editors and response loss in your test plan. Only then implement the smallest complete workflow in a practice copy.

**This exercise is design-first on purpose** - it is an RFC kata, not a buildable ticket with a fixed answer, and it is the only one in this chapter that works that way. Judge your result against these acceptance criteria rather than against a reference implementation:

1. The written design names every new state (or explains why no new state is needed) and draws the transitions, including the illegal ones.
2. It states who may request a refund and who may authorise it, in terms of *roles*, and says what happens when those are the same person.
3. It defines eligibility in dates you could test: a refund for hours already taken, for today, and for a future approved block are three different answers.
4. Every invariant in the paragraph above appears as a sentence that could become a test name.
5. It says what happens on a duplicate refund request with the same idempotency key, and with a different one.
6. It lists what it deliberately does not solve, and why that is acceptable for a workshop.

If you do implement it, do so in a practice copy, and write the tests from criterion 4 first.

For every exercise, finish with a short review note: problem, resulting behavior, files changed, tests run and limitations. That explanation is part of the engineering work.


## Exercise 7 - Tighten the stale-edit review the client already has

**Start from what exists (checked 2026-10-06).** The basic conflict path is already implemented. A 409 on save sets `stale` (`client/src/Workshop.tsx:134`) and disables the form (line 164). The recovery panel (line 163) lists **every** field as `Saved: … · Draft: …` and offers one button, *Use latest version, keep draft*, which swaps in the latest row's ETag and keeps the typed values. `check_browser.py` covers this as "stale edit shows current values and preserves the draft for explicit review". Do not rebuild it. Improve it.

**Goal.** Make the review panel answer "what actually conflicts?" and give the user a real choice.

1. Show only the fields where the latest saved value differs from the draft, and mark fields the user never touched. To tell those apart you need the row the editor *opened* with. `editor.row` is replaced on recovery, so capture the original snapshot separately.
2. Add a second explicit choice, *Discard my draft*, which loads the latest saved values into the editor. Neither choice may happen automatically.
3. Keep it impossible to submit without an ETag. Explain which status the server would return (`HrisRequest.cs:44`, 428) and why the user could do nothing useful with it.

**Check.** Run `python scripts/check_browser.py` after packaging. The existing stale-edit scenario must still pass. On the server side, `AnUpdateWithAStaleIfMatch_Is409` covers departments only. Add an `ApiScenarioTests` case for `employees/{id}` that does accepted-then-stale and asserts both the 409 and the body's `code`, which is `"conflict"` (`HrisRules.Version`, `Domain/HrisRules.cs:46-49`, throws `HrisError.Conflict`, `Domain/WorkshopModels.cs:42`). Then GET the row and assert its ETag version advanced exactly once.

## Exercise 8 - Prove the idempotency key does its job

**Start from what exists.** `operation()` (`client/src/workshopApi.ts:25-26`) creates one `crypto.randomUUID()` key per user attempt and freezes the method, path, body and ETag with it. The UI's exact-retry button resends that same `Operation`. The server side already has all three contract tests: `ARetryWithTheSameKeyAndBody_ReplaysTheStoredReceipt`, `ReusingAKeyForADifferentBody_Is409` and `AMutationWithoutAValidIdempotencyKey_IsRejected` (422).

**Goal.** Close the gap the course admits in `VERIFICATION.md`: reloading the page loses the pending `Operation`, even though the server still holds the receipt for 24 hours.

1. Persist the uncertain `Operation` (not the token) so that a reload can offer "retry the exact operation". Decide where it lives (`sessionStorage` vs `localStorage`) and when it is deleted: on any definite response, success **or** failure.
2. Pressing submit after a *visible* error must still create a **new** key, because that is a new attempt.
3. Editing the draft and retrying with the old key must still produce **409** (`HrisApi.Mutations.cs:73-76`). Say in one sentence why replaying the old receipt would be worse.

**Check.** Before you change anything, prove the existing guard is load-bearing. Temporarily delete the `Guid.TryParseExact` check (`HrisApi.Mutations.cs:23-26`) and run `dotnet test tests/Nethris.Tests/Nethris.Tests.csproj -c Release`. Exactly `AMutationWithoutAValidIdempotencyKey_IsRejected` should fail: the request is accepted with 201 under the empty key. Restore the check. For the reload feature, extend `check_browser.py`'s lost-response scenario: reload after the dropped response, sign in again, and assert one retry yields one department.

## Lab: red-first against the guards

Before you trust any of the above, watch the suite catch each guard being removed. For each row, disable the guard, **predict which test names go red**, run `dotnet test tests/Nethris.Tests/Nethris.Tests.csproj -c Release` (about six minutes), compare, and restore. Locations were checked on 2026-10-06. "Disable" sometimes means "replace", because a method that must return a value will not compile with its body commented out.

| Guard to disable | Where | How to disable it so it still compiles | Prediction to write down |
|---|---|---|---|
| The `Idempotency-Key` UUID check | `HrisApi.Mutations.cs:23-26` | delete the `if` block | Only the malformed-key test, or the replay tests too? |
| The `If-Match` requirement (the 428) | `HrisRequest.Expected`, `HrisRequest.cs:34-45` | replace the `throw` with `return 0;` | Does the stale-conflict test still pass? What status does the no-`If-Match` test now get? |
| The role minimum for `audit-logs` | `MinimumRole`, `HrisApi.cs:134-137` | delete that `if` block | Which of the three Employee-refusal tests notice? |
| Server-side session revocation | `HrisAuth.Logout`, `Security/HrisAuth.cs:74-75` | comment out the two-line `if` that deletes the session row | Does any test fail, or is revocable sign-out covered by only one? |

The last row is the interesting one: if only a single test covers revocable sessions, the suite is one deletion away from silently losing that guarantee. Finding a hole like that is a better result than a clean sweep of predictions. Compare your predictions with the answers in [06](06-SOLUTIONS-AND-REVIEW.md#the-red-first-lab-reasoned-answers).

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


## Exercise 7 - Resolve an ETag conflict without losing the draft

**Scenario.** Two HR users open the same employee. The first saves. The second saves a minute later with the `If-Match` value they loaded.

**Build it.** In `client/`, make the conflict path a first-class state rather than an error toast.

**Acceptance criteria.**

1. A save that returns **409** leaves every field the user typed exactly as they typed it. Nothing is refetched over the top of it.
2. The UI shows, side by side, the value the user is trying to save and the value the server now holds, field by field, for the fields that differ.
3. The user has two explicit choices: *keep my version* (which re-sends with the current ETag after they confirm) and *discard mine* (which loads the server's version). Neither happens automatically.
4. A save with no `If-Match` is impossible from the UI - if the client has no ETag it must re-read before offering a save button. Prove you understand why: what status would the server return, and what would the user be able to do about it?
5. After a successful save the new ETag from the response is stored; a second save works without a reload.

**Required tests.** Add an `ApiScenarioTests` case that performs the accepted-then-stale sequence and asserts `409` with the *response body's* code. Add a second asserting that the record's version advanced exactly once across the two attempts.

## Exercise 8 - Prove the idempotency key does its job

**Scenario.** A PTO request is submitted over a flaky connection. The response never arrives and the browser retries.

**Build it.** Make the client generate one `Idempotency-Key` per *attempt by the user* and reuse it for every automatic retry of that attempt, discarding it only when a response (success **or** failure) is received.

**Acceptance criteria.**

1. Retrying after a lost response creates exactly one PTO request, and the second response carries `Idempotency-Replayed: true`.
2. Pressing submit again after a *visible* error generates a **new** key, because that is a new attempt.
3. Editing the form and submitting with the old key is refused with **409** (the fingerprint changed) rather than silently replaying the stale receipt. Say in one sentence why replaying would be worse than the error.
4. The key survives a page reload if and only if the attempt does.

**Required tests.** Port the three cases into `ApiScenarioTests`: replay with the same key and body, a changed body with the same key (409), and a missing or malformed key (422). Two of the three already exist - write the third yourself and make sure it fails before your change if you remove the guard.

## Lab: red-first against the guards

Before you trust any of the above, watch the suite catch each guard being removed. For each row, comment the guard out, **predict which test names go red**, run `dotnet test tests/Nethris.Tests/Nethris.Tests.csproj -c Release`, compare, and restore.

| Guard to remove | In | Prediction to write down |
|---|---|---|
| The `Idempotency-Key` UUID check | `HrisApi.Handle` | Which tests fail: only the malformed-key one, or the replay ones too? |
| The `If-Match` requirement (the 428 branch) | `HrisApi` precondition helper | Does the stale-conflict test still fail, or does it start passing for the wrong reason? |
| The role minimum for `audit-logs` | `HrisApi.Handle` | Which of the three Employee-refusal tests notice? |
| The session lookup during authentication | `HrisAuth.Require` | Does any test fail, or is revocable sign-out only covered by one? |

The last row is the interesting one: if only a single test covers revocable sessions, the suite is one deletion away from silently losing that guarantee. Finding a hole like that is a better result than a clean sweep of predictions.

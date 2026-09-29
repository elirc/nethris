# Concepts that connect a form to reliable data

## Identity and permission are different questions

Authentication asks, “Which account is making this request?” Authorization asks, “May that account perform this action now?” Nethris signs a JWT containing stable account/session identity. [HrisAuth.Require](../server/Nethris.Api/Security/HrisAuth.cs) then loads current database state: the session must exist and be unexpired, the account must be active, and the employee must not be terminated.

Imagine Jordan signs in as HR at 09:00 and an Admin demotes Jordan at 09:10. Trusting a role embedded in an eight-hour token would preserve the old permission. Loading the current role makes the next request observe the change. Logout deletes the server session, so keeping a copy of the old token does not keep access.

A business title is not a security role. Editing a JobRole does not grant account access. Only Admin can create or change accounts. HR can manage ordinary employee data, but cannot edit an Admin employee or include accountRole in an employee request. Unknown input properties are rejected.

The browser also has a session boundary. An old refresh may already hold an authorized response when sign-out finishes. Clearing only the token would let that late response restore stale session state or cached views. The current UI increments its request generation and clears cached records when a session ends or a new account signs in. Both successful and failed old reads are ignored. The browser regression holds a completed directory response, signs out, and then releases that response to prove the signed-out screen stays in place.

## A transaction defines one durable outcome

Suppose HR approval performs three writes: update request state, deduct hours and insert approval history. If only two succeed, the database tells contradictory stories. The API therefore owns an immediate SQLite write transaction around authentication, validation, domain writes, audit and receipt creation.

“Immediate” means the connection obtains the write reservation before reading state that it intends to change. This makes two concurrent approvers take turns rather than both deciding from an old balance. EF SaveChanges is not the final commit when an outer transaction exists. See [HrisApi.cs](../server/Nethris.Api/HrisApi.cs) for the boundary and [the HTTP checks](../scripts/check_api.py) for a late-failure rollback example.

## Versions protect intent

A record version answers whether the row still matches the version the user edited. Suppose Maya opens employee version 4. Jordan saves a new manager, producing version 5. Maya's PUT still carries the strong ETag for version 4. The API rejects it, and the browser preserves Maya's draft.

Refreshing the ETag automatically would silently overwrite Jordan's change. Instead, the editor shows current saved values alongside the draft and requires explicit review. After review, the user can keep the draft and use the latest version for a new attempt. The server remains authoritative if another writer wins again.

The EF version is also a concurrency token. [HrisDbContext](../server/Nethris.Api/Data/HrisDbContext.cs) increments modified records and checks their original version in updates. Direct domain access still receives protection against competing tracked edits.

## Receipts protect against uncertainty

A network error does not establish that a save failed. The database may have committed immediately before the response disappeared. Retrying with a new operation key can create a duplicate.

Nethris stores a receipt for 24 hours, scoped to the account and operation key. Its fingerprint includes method, path, query, If-Match and exact serialized body. Reusing the key with the same fingerprint returns the recorded result. Reusing it with different content returns a conflict. Current authorization is checked before replay, including PTO review scope. The receipt and mutation commit together.

A version and a receipt solve different problems. Versions detect stale intent. Receipts recover the outcome of one particular intent. The browser keeps an uncertain Operation in memory and disables changes to its draft until the user retries or explicitly stops retrying and inspects saved records. Reloading loses that in-memory snapshot; the database receipt remains.

## Quantities need explicit units

The domain exposes decimal hours, but the database stores quarter-hour integers. 7.75 hours becomes 31 quarter units. 120 hours becomes 480 units. Subtracting 31 from 480 yields 449, or 112.25 hours. Values such as 0.1 hours are rejected instead of rounded silently.

Salary bands use integer cents. 50000.25 becomes 5000025. Values with more than two decimal places are rejected. The workshop does not implement multiple currencies or payroll. Timestamps become integer Unix milliseconds so SQLite can order them reliably. Capture “now” into a local value before composing an EF expiry query; leaving DateTimeOffset.UtcNow inside a provider expression caused a real setup failure during development.

## State transitions carry business rules

PendingManager moves to PendingHr after the assigned current direct manager approves. PendingHr moves to Approved after a different HR/Admin reviewer approves. Rejection closes a pending request. A requester may edit or delete pending requests without decisions; once review starts, cancellation preserves history. Approved requests cannot be edited, cancelled or refunded in this workshop.

Pending hours reserve balance. Approved hours have already reduced balance. Do not subtract both again when computing availability. These rules are simplified teaching policy, not a claim about any employer's leave policy.

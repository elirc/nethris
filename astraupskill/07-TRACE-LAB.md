# Trace lab: predict state before running the request

Trace tables train you to reason about state changes rather than recognize familiar code. For each scenario, write your prediction first. Then inspect the source and run the relevant [HTTP checks](../scripts/check_api.py) or browser workflow. Explain any mismatch in terms of the actual boundary that rejected or committed the operation.

## Lab A — A create response disappears

Initial state: no department named Learning Lab, no receipt for operation key K, and an authenticated HR session.

| Step | Request or event | Expected durable state |
|---|---|---|
| 1 | POST departments with key K | Transaction starts; no committed change yet |
| 2 | Department SaveChanges succeeds | Row exists inside the transaction only |
| 3 | Audit and receipt SaveChanges succeed | All writes remain in the same transaction |
| 4 | Transaction commits | Department, audit and receipt are durable |
| 5 | Network drops the response | Database stays committed; browser outcome is uncertain |
| 6 | Browser retries exact request with K | Stored response returns; no second department or audit |

Now change only the retry's body while keeping K. Predict the result. It is a conflict because the fingerprint differs. Next, demote the HR account before the retry. The current account permission is checked before replay, so the former HR cannot use the receipt to perform the privileged operation.

Find the order yourself. `auth.Require` runs at [HrisApi.cs:62](../server/Nethris.Api/HrisApi.cs), before `Mutate` is called at line 82. The receipt lookup is inside `Mutate` at `HrisApi.Mutations.cs:31`. For PTO decisions, `Replay` (lines 71-85) also re-checks that the caller may still review that request (`RequireDecisionStillAllowed`, lines 89 onward). If the code looked up and returned receipts before authentication, what information could a caller recover after losing permission?

## Lab B — Two editors and one row

Start with department version 3, name Engineering. Editor A changes the cost center; editor B changes the name. Both carry version 3.

A wins and produces version 4. B's request must fail rather than overwrite A. The browser retains B's name and its old cost-center value in the draft. The comparison panel reveals both saved and draft values. If B chooses to keep the whole draft, B may intentionally replace the cost center too; that is why review matters.

The existing editor is not an automatic field-level merge engine. Design a merge policy on paper: which fields are unchanged from B's original snapshot, and which were intentionally edited? What additional original-draft data would the browser need to distinguish those cases? Do not claim the current application already performs that merge.

## Lab C — Balance reservations

Start with 120 hours. Request A reserves 80 hours across enough dates. Request B asks for 48 hours on different dates. B must fail because pending reservations plus B exceed the balance. The stored balance is still 120.

After A is finally approved, stored balance becomes 40. A is no longer counted as pending. A new eight-hour request should fit. If your availability calculation subtracts both approved history and the already-reduced balance, it will understate availability. If it ignores pending requests, it will overbook.

Find the overlap and pending-sum logic in [HrisPto.Save](../server/Nethris.Api/Domain/HrisPto.cs). Explain why those reads belong inside the write transaction, and why counting only visible browser cards cannot establish available balance.

## Lab D — The manager changes

Alex submits PTO with Maya as AssignedManagerId. Before Maya reviews it, HR assigns Alex to a different manager. The captured manager and current direct manager no longer agree. Maya's decision is rejected.

The workshop's recovery path is cancellation and resubmission, which captures the current eligible manager. It does not silently transfer an old approval request or let HR bypass the first stage. Discuss the advantages and costs of this simple policy. A production workflow might need explicit reassignment, notification and history; those are additional features with their own contracts.

## Lab E — Delete a record with history

Create a department with no employees, then delete it using its current ETag. That is allowed. Try deleting Engineering while employees reference it. That is rejected. Archive it instead, then try assigning a new employee to it; the new assignment is rejected while existing employees retain their relationship.

Now try deleting an employee with a user account or PTO history. The relationship guard rejects it. Termination is a separate business state and has additional checks for active leadership, direct reports and pending PTO. A delete button cannot decide which policy applies; the API must enforce it.

## Explain your result

For each lab, finish with four sentences: what the user intended, what state the server read, which invariant applied, and what committed. Avoid vague explanations such as “the backend handles it.” Name the transaction, version, foreign key, role check or receipt that actually determines the outcome.


## The code you are tracing

Two short excerpts make the tables above concrete.

**Every request opens a transaction before it does anything else** (`server/Nethris.Api/HrisApi.cs:48-55`). This is why lab A's steps 1-4 are a single unit: there is no window in which the department row exists without its audit row and receipt.

```csharp
var body = route.IsWrite ? await HrisRequest.Body(http.Request) : "";

// One request, one connection, one transaction: deferred for reads, immediate for writes.
await using var db = database.Create();
await db.Database.OpenConnectionAsync();
var connection = (SqliteConnection)db.Database.GetDbConnection();
await using var transaction = connection.BeginTransaction(deferred: !route.IsWrite);
await db.Database.UseTransactionAsync(transaction);
```

`deferred: !route.IsWrite` (`IsWrite` is `Method != "GET"`, line 36) is worth pausing on: a read takes a deferred transaction, which does not acquire a write lock, so concurrent readers do not block each other. Nothing commits until an explicit `transaction.CommitAsync()` (for mutations, `HrisApi.Mutations.cs:60`), so **any** path that throws - validation, authorization, a constraint, a receipt insert - discards the whole request. Lab A's step 3 and lab B's rejected save both depend on exactly that.

**The retry is recognised by a fingerprint, not just by the key** (`server/Nethris.Api/HrisApi.Mutations.cs`; `Fingerprint` at 65-69, the lookup at 31, `Replay` at 71-76):

```csharp
var material = method + "
" + http.Request.Path + "
" + http.Request.QueryString + "
" + http.Request.Headers.IfMatch + "
" + body;
return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)));
...
var prior = await db.Receipts.SingleOrDefaultAsync(r => r.UserAccountId == actor.UserId && r.Key == key);
...
if (prior.Fingerprint != fingerprint)
{
    throw HrisError.Conflict("This operation key was already used for a different request.");
}
```

Write the two paths into your trace table separately: *same key, same fingerprint* is a replay (the stored response, with `Idempotency-Replayed: true`), and *same key, different fingerprint* is a **409**. Note that the `If-Match` header is part of the fingerprint - retrying "the same edit" against a newer version is not the same request, and the server is right to say so.

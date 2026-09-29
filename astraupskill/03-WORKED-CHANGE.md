# Worked change: make PTO approval durable and retryable

This chapter explains an implemented change. It is not a request to apply the same patch again. Use [the preserved original Program](../docs/original/server/Nethris.Api/Program.cs.txt) and [the current PTO service](../server/Nethris.Api/Domain/HrisPto.cs) to compare behavior.

## Start with the observed failure

The original API accepted a one-day request for 9,999 hours. Its approval route then failed with an EF concurrency exception because a newly attached approval with a supplied Guid was treated as an existing row. The balance remained unchanged because that SaveChanges rolled back. The correct observation is “approval failed”; a negative balance was not observed.

The source also lacked bounds and a sufficient-balance check. Fixing only the insert behavior would therefore have exposed another problem. A good implementation review follows the whole flow: accepted input, current permission, state transition, quantities, persistence and retry behavior.

## Define the contract before editing the handler

Use a fictional request with one future date, 7.75 hours and a short reason. The request starts at PendingManager when the employee has an eligible active manager account. Otherwise it starts at PendingHr. Capturing AssignedManagerId makes the first reviewer explicit.

The requester cannot approve their own request. The first reviewer must be the assigned current direct manager. The final reviewer must be HR/Admin and must differ from the first reviewer. Pending requests reserve available hours, and overlapping pending/approved date spans are refused.

These choices answer ambiguous questions before code is written. An Admin does not silently bypass the manager stage. If a manager changes and the captured reviewer is no longer current, the requester can cancel and resubmit. Approved cancellation/refund is deliberately outside the implemented workflow.

## Follow the implementation

The HTTP request is conceptually:

```http
POST /api/pto/{request-id}/decision
Authorization: Bearer <current-session-token>
Idempotency-Key: <new-uuid-for-this-decision>
If-Match: "<request-id-without-hyphens>-2"
Content-Type: application/json

{"decision":"Approved","comment":"Coverage arranged"}
```

These placeholders are explanatory; replace them with values from your own temporary workshop. Do not invent an ETag version. A read response or current page row gives the version you are reviewing.

[HrisApi](../server/Nethris.Api/HrisApi.cs) begins an immediate transaction and checks the current session. It verifies the operation key and looks for a committed receipt. For a new decision, it checks the expected version and calls HrisPto.Decide.

The domain method checks role, requester identity, state and stage. Manager approval changes the status to PendingHr. HR approval checks the remaining balance, subtracts hours and changes the status to Approved. A new PtoApproval is added explicitly through the DbSet. Its assigned Guid is configured as ValueGeneratedNever in [the EF mapping](../server/Nethris.Api/Data/HrisDbContext.cs), so it represents a new row.

The method saves the request, balance and approval. The outer API then saves the audit entry and operation receipt and commits everything together. A response is sent only after commit.

## Predict the numbers

| Point in the flow | Request state | Stored employee hours | Decision rows |
|---|---|---:|---:|
| Before submission | No request | 120 | 0 |
| After submission | PendingManager | 120 | 0 |
| After manager approval | PendingHr | 120 | 1 |
| After HR approval | Approved | 112.25 | 2 |
| After exact HR retry | Approved | 112.25 | 2 |

The request reserves 7.75 hours before approval without deducting it twice. The final deduction changes the employee version as well as the request version. An old employee editor must therefore reload before changing the balance.

## Verify more than the happy path

[check_api.py](../scripts/check_api.py) tests invalid quantities, past/overlong dates, overlap, insufficient available balance, self-approval, skipped stages, stale versions and concurrent decisions. It retries the same HR operation and checks that the balance does not change again. It also proves that a late receipt insertion failure rolls back earlier SaveChanges.

[check_browser.py](../scripts/check_browser.py) follows Employee → Manager → HR through actual forms, then verifies the employee's updated balance. Its separate lost-response scenario lets the server commit a department create, drops the browser response and uses the exact-operation retry button. This tests the same transaction/receipt design through a visible recovery flow.

Explain this change in a review as a concrete before/after: previously requests were unbounded and approval insertion failed; now validated requests pass through authorized versioned stages, and the decision, balance, audit and receipt form one durable result.

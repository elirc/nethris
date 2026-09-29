# Side Effects, Async, And Reliability

## Present Side Effects

| Side effect | Evidence | Reliability note |
| --- | --- | --- |
| Database creation/seed | [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L85) | Happens at startup. |
| Employee create audit | [Program.cs](../../../server/Nethris.Api/Program.cs#L196-L197) | Saved with employee in one `SaveChangesAsync`. |
| Employee update audit | [Program.cs](../../../server/Nethris.Api/Program.cs#L248-L249) | No field diff. |
| PTO request audit | [Program.cs](../../../server/Nethris.Api/Program.cs#L317-L319) | Saved with request. |
| PTO decision audit and balance update | [Program.cs](../../../server/Nethris.Api/Program.cs#L367-L381) | Balance update and audit save together. |

## Missing Async Infrastructure

No hosted service, queue, channel, webhook handler, email sender, retry policy, outbox, cache, or `IHttpClientFactory` usage was found. That is not a bug; it is a design boundary. The first realistic async addition would be PTO notification after [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381).

## Reliability Concepts

- Idempotency: repeated approval calls should not double-deduct balance. Current closed-state guard is [Program.cs](../../../server/Nethris.Api/Program.cs#L337-L340).
- Isolation: manager can approve only direct-report manager-stage requests [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353).
- Failure visibility: no `ILogger` usage found, so failures rely on default ASP.NET logging.
- Timeout: no external calls exist; add timeouts when they do.

## Drill

Design `PtoNotificationOutbox` as illustrative fake code. State how it is written in the same transaction as [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381), then processed by a `BackgroundService`.


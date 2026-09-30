# 23. Fault injection and response uncertainty

Failure experiments are most useful when they target one known boundary. Nethris has several: request decoding, domain validation, database save, transaction commit, response serialization and delivery, and client refresh. Read the error mapping in [Program.cs](../../../server/Nethris.Api/Program.cs), the mutation flow in [HrisApi.Mutations.cs](../../../server/Nethris.Api/HrisApi.Mutations.cs), and client recovery in [Workshop.tsx](../../../client/src/Workshop.tsx).

## Name the simulated failure

An interrupted fetch is not the same as a server validation rejection. A database-busy response is not proof of a stale record. An unreadable successful response can leave the client uncertain even when the server accepted the mutation. A failed refresh after a known successful mutation is a read problem with a known saved result. Your experiment name should identify the exact simulated condition.

Create a failure matrix with injection point, expected HTTP or client category, accepted business state, receipt state and recovery action. This forces you to distinguish before-commit and after-commit failures. It also reveals when the client cannot know which side of the boundary the failure occurred on and must use exact retry to reconcile.

## Database contention is a supported error path

The middleware maps SQLite busy or locked error codes to a 503 message asking for exact retry. The database connection has a configured timeout, and writes use an immediate transaction. Do not infer universal latency or throughput from those settings. They establish a bounded local policy, while actual contention behavior depends on the experiment and environment.

For a controlled rehearsal, hold an appropriate lock in a disposable database and issue a valid competing request. Record whether the intended busy path is reached and how the client preserves the operation. Avoid using a valuable workshop database or leaving a lock holder running. The test fixture should own every connection and release it deterministically.

## Response loss can be simulated at different strengths

Discarding a successful response in a test proves that replay works after known acceptance. Closing a connection after commit investigates a more realistic delivery failure but is harder to control. Returning malformed response content to the client tests unreadable-response recovery. Each is useful, but none should be mislabeled as covering every possible network partition.

Use the simplest experiment that establishes the intended claim. If the goal is stable operation bytes across retry, a controlled request stub may be enough. If the goal is exactly one balance deduction through the real server, an HTTP-plus-database scenario is more appropriate. State the remaining untested boundary explicitly.

## Exercise NH-23A: design three fault cases

Choose one pre-domain rejection, one late pre-commit failure, and one post-commit response loss. For each, write the valid starting fixture, the injected condition, expected accepted state and recovery path. Use a fresh operation key for each independent case and preserve the original key only for retries of that same case.

The late failure should be late by evidence, not by its name. Show that the domain path was reached before the injected failure, then inspect a fresh accepted read afterward. A malformed body cannot stand in for a receipt-write failure. This is the same discipline as isolating one invalid field in a unit test, applied to system stages.

## Exercise NH-23B: inspect error mapping

Create a table for known HrisError, concurrency exception, database busy, constraint failure, malformed JSON and an unexpected internal exception. Record the middleware's public category and message shape. Do not expose internal stack traces in the ordinary learner interface. Diagnostic detail belongs in controlled logs or test output with appropriate scope.

Then connect each public category to the client's pending-operation decision. Explain why a known lower-status rejection allows correction while an uncertain or server-error path retains exact retry. A status code alone does not describe every product meaning, so include the actual application message and state transition.

## Exercise NH-23C: prove recovery does not duplicate effects

Use a PTO final approval or department creation. After simulated lost delivery, retry the exact operation and inspect accepted record count, version, audit rows, receipt and any balance effect. The replayed JSON is one observation among several. A hidden duplicate mutation can exist even when responses look similar.

Add a changed-body retry under the same key and verify conflict. This counterexample proves that your harness is actually exercising the fingerprint protocol rather than accepting every repeated call. Preserve all synthetic inputs in the report so another learner can reproduce the distinction.

## Review standard

A strong failure investigation names the injection point, controls its timing, verifies accepted effects independently, and reports the recovery semantics honestly. The final artifact should be a matrix and a small set of reproducible cases, not a large collection of unexplained red network entries.

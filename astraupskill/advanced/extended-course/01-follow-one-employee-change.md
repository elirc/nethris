# 01. Follow one employee change

Begin with a concrete question: what must happen for an HR user to change an employee's location and see that change accepted? The repository contains several generations of names and supporting files, so reading every file alphabetically is a poor substitute for tracing the actual workshop route. Start with [workshopApi.ts](../../../client/src/workshopApi.ts), [HrisApi.cs](../../../server/Nethris.Api/HrisApi.cs), and [HrisPeople.cs](../../../server/Nethris.Api/Domain/HrisPeople.cs).

## The client captures an operation

The client helper creates an operation containing path, method, serialized body, a new UUID key, and an optional ETag derived from a loaded row. The body is serialized when the operation is created. This matters because a later retry can reuse the same captured bytes rather than re-read a form that may have changed. The operation key identifies a logical mutation attempt for the receipt protocol; it is not the employee's identity or editing version.

The request helper sends the captured method, body, idempotency key and precondition header. A network exception becomes an API error with status zero and a message explaining that a save may have committed. The helper does not claim that failure to receive a response proves database rejection. Preserve that uncertainty in your trace. It is the reason exact retry exists as a recovery workflow.

## The dispatcher owns the outer boundary

The dispatcher reads the route and request body, creates a database context, opens a connection, begins a transaction, and attaches the context to that transaction. Writes use the immediate transaction choice shown in the source; reads use the deferred choice. Setup and login have dedicated handling. Ordinary protected routes require an actor before moving into reads or mutations.

For an employee write, the route-level minimum role is HR. The domain operation also requires HR. These checks are not identical in purpose: one protects dispatch before record work, while the domain method preserves its own precondition when called through its supported context. Trace both without declaring duplication automatically unnecessary. A future alternate caller could otherwise bypass a rule that lived only in one route.

## The domain operation validates relationships

`SaveEmployee` normalizes and validates email, employee number, names, status, balance and hire date. For an existing employee, it loads the record with its account, checks the expected version, and evaluates restrictions involving administrator accounts and department leadership. It checks uniqueness, reference records, manager chains, pending leave reservations, and termination prerequisites before assigning the accepted candidate fields and saving changes.

This is much more than copying a request body onto an entity. A location-only edit still travels through an aggregate of rules because the request contains the employee representation the operation accepts. Your source notebook should identify which fields are directly changed and which relationships must remain valid. Do not infer that a visible form with one edited field causes a one-column-only domain command unless the actual contract says so.

## SaveChanges is inside a larger transaction

The domain method calls SaveChanges and returns a projected employee view. The mutation dispatcher then creates an audit row and operation receipt, saves those changes, commits the transaction, and writes the HTTP response. Therefore the first SaveChanges is not the final durable acceptance boundary for the whole request. A later failure can still cause the outer transaction to roll back.

Draw five moments: entity assignment, domain SaveChanges, audit/receipt SaveChanges, transaction commit, and response delivery. A caller observing the final response is later than commit, while an entity inspected in memory is earlier. The application can commit successfully and lose the response afterward. Conversely, it can execute the domain SaveChanges and later fail before commit. These two cases require different recovery explanations.

## Exercise NH-01A: build a vertical trace

Trace a department rename through the same layers. Name the request contract, version source, authorization check, uniqueness rule, domain save, response projection, audit creation, receipt creation and commit. Use function names and short claims rather than copying entire files. Mark the point at which each claim becomes established.

Your trace should include a rejected case. Choose a stale version or duplicate department name and keep the rest of the input valid. Explain what the client retains and what the database must not accept. A success-only trace leaves out the boundaries most likely to matter during maintenance.

## Exercise NH-01B: compare three identities

Create a table for employee identity, editing version and operation key. For each, state who creates it, when it changes, and which error it helps detect. An employee keeps identity while its version advances. A new logical edit receives a new operation key. An exact retry reuses its operation key and captured request. A version should not be refreshed automatically merely to force a stale request through.

## Exercise NH-01C: explain an unknown outcome

Suppose the connection fails immediately after the server commits the employee change, audit and receipt. Predict the client's error category and the purpose of retrying the same operation. Then contrast a failure before commit. The client may not know which happened from the transport error alone. Explain how the stored receipt lets the server resolve the first case without performing the business mutation again.

## Review standard

A strong answer follows one real route and distinguishes decisions, tracked state, transactional writes, acceptance and delivery. It does not treat every method named Save as a commit. It also avoids claiming that the entire HRIS is understood after one trace. The point is to build a reliable method you can reuse for PTO decisions, account changes and future features.

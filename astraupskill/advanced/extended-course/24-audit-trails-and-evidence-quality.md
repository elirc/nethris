# 24. Audit trails and evidence quality

The mutation handler creates an audit row alongside the operation receipt, using the actor, action, entity kind and target identity. Read `Describe` and audit creation in [HrisApi.Mutations.cs](../../../server/Nethris.Api/HrisApi.Mutations.cs), plus the Audit helper in [HrisAuth.cs](../../../server/Nethris.Api/Security/HrisAuth.cs). An audit record answers who performed a named accepted action on which target under this application's representation.

## Know what the row contains

The helper records actor email, action, entity type, entity identity and optional details. It does not automatically store a complete before-and-after snapshot of every field. Therefore an audit row can establish that an operation occurred without being sufficient to reconstruct all historical values. Do not describe the log as event sourcing or full undo history unless those properties are actually implemented.

Describe obtains a target identity from the route when available or from the returned created representation. It derives an ETag when the response contains identity and version. This connects the accepted response with receipt and audit metadata. Review this path when adding a new action that returns a different shape; a missing identity in the response can affect the recorded target.

## Atomicity makes the record meaningful

For ordinary mutations, audit and receipt commit with the business change. A log written before validation and outside the transaction would mean attempted action, not accepted action. Both kinds of logs can be useful, but they need different names and semantics. The current audit placement supports a particular accepted-operation claim.

A replay does not rerun the fresh Execute branch and add another ordinary mutation audit row through that path. If you want separate replay-attempt telemetry, design it as such rather than confusing it with another accepted business action. Counting audit rows as business mutations requires understanding exactly which routes write them, including specialized setup and authentication behavior.

## Evidence needs identity and context

An audit timestamp alone cannot prove which of two similar requests it describes. Stable target identity, actor, action and operation receipt provide stronger context. A human screenshot of a row can still omit the query scope or surrounding state. For learning reports, combine a synthetic operation identifier, accepted response, scoped audit query and relevant business-state assertion.

Avoid logging full passwords, tokens or private signing material to make the report feel complete. Those values are unnecessary to establish the action sequence. Synthetic account labels and redacted request summaries can preserve causality. The goal is enough evidence to assess the behavior, not maximal data collection.

## Exercise NH-24A: map audit semantics

Choose department create, employee edit, PTO decision, cancellation and exact replay. For each, identify the action label, entity kind, target identity source and expected audit count change under the actual route. Inspect specialized routes separately before generalizing the ordinary mutation pattern to all authentication events.

Use a known starting count or a query scoped to your fixture. A total count over a seeded database can be misleading if setup creates its own audit entries. Your expected value should come from the experiment's declared baseline, not an assumption that the database begins with no history.

## Exercise NH-24B: propose richer change evidence

Imagine adding a before-and-after summary for department description edits. Define which fields are recorded, how null and trimmed text appear, and whether sensitive text belongs in the audit record. Decide how the summary is generated from accepted normalized values rather than raw request text. Raw text may differ from what was actually stored.

Consider size bounds and retention. A description field should not make every audit row an unbounded copy of arbitrary input. If a hash is used, explain that it establishes equality of content under the chosen encoding but does not let a reviewer read the content. Choose the representation based on the audit question you need to answer.

## Exercise NH-24C: write a verification package

For one feature, assemble a request summary, accepted response, relevant database observation, audit record and receipt/replay result. Explain the claim each artifact supports. Include a rejected operation and show that it did not create the same accepted audit effect. Keep the report concise enough that a reviewer can follow the causal sequence.

Do not claim immutable or tamper-proof history merely because a table is called AuditLogs. Such guarantees depend on access controls, storage architecture and operational processes beyond this local workshop. State the implemented behavior and any proposed stronger assurance separately.

## Review standard

A strong answer distinguishes accepted actions from attempts, metadata from full history, and application audit semantics from broader integrity guarantees. It uses scoped synthetic evidence and connects audit creation to the transaction boundary. The final report should make an accepted change explainable without exposing unnecessary private values.

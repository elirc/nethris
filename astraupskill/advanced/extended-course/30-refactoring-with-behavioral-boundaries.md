# 30. Refactoring with behavioral boundaries

The application separates dispatcher routing, read handlers, mutation orchestration, HTTP helpers, domain services and database mapping. This organization can be improved over time, but a refactor must preserve the behavior those boundaries coordinate. Read [HrisApi.cs](../../../server/Nethris.Api/HrisApi.cs), [HrisApi.Reads.cs](../../../server/Nethris.Api/HrisApi.Reads.cs), and [HrisApi.Mutations.cs](../../../server/Nethris.Api/HrisApi.Mutations.cs) as one partial class before proposing extraction.

## Start with an invariant map

List the invariants owned by the dispatcher: request-scoped transaction, current actor, minimum route role, receipt orchestration, commit-before-response and consistent serialization. List domain invariants separately: manager graph, active references, balance reservations, workflow authority and version comparison. List storage invariants: uniqueness, restrictive relationships, scaled values and concurrency tokens.

A refactor can move code without changing ownership, but it can also accidentally split an invariant across incompatible lifetimes. For example, extracting an audit writer that creates its own context can detach audit acceptance from the business transaction. Extracting a generic request handler that sends the response before commit can misreport success. These are concrete failure sequences, not abstract objections to abstraction.

## Preserve ordering where it carries meaning

Authentication before receipt lookup prevents replay from becoming an authorization bypass. Domain SaveChanges before response description may establish generated or prepared values needed by the response. Audit and receipt creation before commit makes them atomic with the mutation. Client generation checks after awaits prevent obsolete publication. Not every statement order is incidental.

When reviewing a proposed rearrangement, write what is known before and after each stage. If a helper hides an await or creates a new context, make that effect visible in its contract. A short method is not automatically easier to reason about if its dependencies and acceptance boundary become obscure.

## Characterization tests need independent claims

Before a structural change, select existing tests that cover the behavior at risk and add only missing meaningful cases. Do not freeze every private method call count as the contract. That can prevent harmless reorganization while missing accepted-state errors. Prefer externally visible results, transaction outcomes and preserved data.

For a dispatcher extraction, useful cases include missing precondition, stale valid precondition, exact replay, changed fingerprint, role downgrade, late rollback and successful response projection. A pure mock test asserting that three services were called cannot establish the shared transaction. Keep the real SQLite or HTTP boundary for claims that depend on it.

## Exercise NH-30A: review a proposed unit-of-work helper

Specify a helper that receives an operation callback and owns commit. Define how it creates the context, begins and attaches the transaction, obtains the actor, propagates exceptions and returns a result without sending HTTP bytes prematurely. Decide which concerns remain outside the helper so it does not become an opaque universal framework.

Write a counterexample involving a helper callback that opens another connection. Explain how the API or review convention makes that misuse visible. Type signatures can guide ownership, but they do not automatically prove that every nested operation participates in the intended transaction. Integration tests remain necessary for the actual path.

## Exercise NH-30B: review a generic CRUD abstraction

Compare department, job-role and employee saves. They share broad steps but have different relationship rules, projections and limits. Identify a genuinely shared mechanism, such as precondition parsing or transaction orchestration, and a domain rule that should remain explicit. Avoid hiding manager-graph and reservation logic inside generic callbacks whose ordering is hard to inspect.

Propose an extraction only after writing two concrete callers. If the abstraction needs many flags to express different workflows, explain whether it improves clarity. The goal is maintainable reasoning, not maximum reduction in line count. A small amount of visible repetition can be preferable to a generalized path that obscures authority and history.

## Exercise NH-30C: plan the refactor verification

Create a before-and-after behavior table and identify the targeted existing tests. Preserve source-state information and unrelated user edits. Run the relevant suite under the same environment before and after if feasible, and report any pre-existing failures separately from new regressions. Do not claim the refactor caused a failure that already existed without evidence.

For a documentation-only proposal, stop at the design and test plan rather than pretending code was changed. For an implemented refactor, inspect the final diff for accidental contract changes: status codes, JSON names, query ordering, receipt fingerprint inputs and transaction lifetimes. Small textual changes in these areas can be externally significant.

## Review standard

A strong refactor proposal names the behavior it preserves and the boundary each extracted component owns. It uses concrete counterexamples, keeps domain meaning visible, and validates persistence and HTTP claims at the appropriate layer. The final reviewer should understand why the structure changed and why the accepted behavior remains the same.

# 14. Transactions and late failure

The Nethris dispatcher gives each request a database context and transaction. Domain operations can call SaveChanges before the outer mutation handler adds audit and receipt records. Read [HrisApi.cs](../../../server/Nethris.Api/HrisApi.cs), [HrisApi.Mutations.cs](../../../server/Nethris.Api/HrisApi.Mutations.cs), and [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs). This arrangement is a useful case study in the difference between flushing changes and committing the complete operation.

## Draw the transaction envelope

Place the following actions inside one envelope: load current actor, find an existing receipt, execute domain rules, save domain changes, project the response, add audit and receipt, save again, and commit. Some routes have specialized paths, so this sequence describes an ordinary fresh mutation. The response write occurs after commit. Mark every await, but do not confuse an awaited SaveChanges with the outer acceptance boundary.

The context is explicitly attached to the transaction created from its connection. This is evidence that the domain SaveChanges participates in the outer transaction. A future refactor that creates another context or connection inside a helper could break that relationship. Review dependency flow whenever persistence code is extracted into services. A method signature receiving the transaction-scoped context makes the ownership more visible.

## Late failure can follow an apparently successful domain save

Suppose a department update passes validation and SaveChanges succeeds. Then receipt insertion fails before commit. The intended guarantee is that the department change and audit/receipt do not become partially accepted. A test should read accepted state using a fresh context after the request fails. Inspecting the original tracked entity can show an in-memory updated value even though the transaction rolled back.

This distinction is especially important in debugging. An object assigned a new status is not proof of durable state. An SQL statement executed within an uncommitted transaction is not proof that another completed request can observe it as accepted. Label your observation source: tracked entity, database read in the same transaction, fresh post-failure read, or HTTP response.

## Version preparation also needs observation scope

PrepareChanges advances versions for modified versioned entities before saving. If the transaction later rolls back, the in-memory object can still carry the advanced value until its context is discarded. The accepted database version should be checked independently. A test that retains the failed request's object and reports its version as persisted can reach the wrong conclusion.

For a fresh successful mutation, the response projection and receipt capture the accepted candidate before the final commit, and delivery happens afterward. If commit fails, the response should not be reported as a successful accepted result. Trace the actual control flow and middleware rather than assuming that serializing JSON sends it immediately.

## Exercise NH-14A: create a failure ladder

List failure points before body parsing, during domain validation, during domain SaveChanges, during audit/receipt SaveChanges, during commit, and during response delivery after commit. For each, state which artifacts may exist in memory and which accepted database effects should exist. The final row differs from the others because accepted effects can exist even when the client outcome is unknown.

Choose one deterministic failure point for an executable integration experiment. Explain how the injected failure reaches that point without accidentally failing earlier. A malformed body cannot establish rollback after the domain save. A database trigger or controlled test hook may be appropriate in a disposable database, but document precisely what it simulates.

## Exercise NH-14B: prove atomic audit and receipt

Start with a known record, audit count and receipt count. Attempt a valid mutation whose later receipt or audit write is forced to fail in the test environment. After the request settles, open a fresh context and compare all three categories. Then remove the fault and run a valid control. The positive control ensures the test fixture did not simply prevent every request from reaching the mutation.

For the failure assertion, do not require internal generated identifiers to match a guessed value. Compare the business record's accepted fields and version, and count or query audit/receipt rows scoped to the synthetic operation. This keeps the test focused on atomicity rather than random identity generation.

## Exercise NH-14C: review a service extraction

A proposed helper creates its own HrisDbContext to write audit logs. Explain why that can change the transaction guarantee even if its method is awaited. Identify what must be shared or explicitly coordinated for the audit to remain atomic with the domain change. A separate successful commit cannot be undone merely because the caller's later transaction fails.

Write a code-review comment that names the concrete failure sequence and asks for a boundary-preserving design. Avoid a vague instruction to use transactions; the existing caller already has one. The issue is whether the new helper participates in that exact acceptance boundary.

## Review standard

A strong submission draws the envelope, distinguishes memory from accepted storage, and proves rollback with fresh reads after a deliberately late failure. It also identifies the post-commit lost-response case as uncertainty rather than rejection. Those habits make transaction claims precise enough to survive refactoring.

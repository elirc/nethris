# 05. Manager graphs and global invariants

An employee's manager looks like a single nullable identifier, but changing it can affect the structure of the whole organization. Read `ManagerChain` in [HrisRules.cs](../../../server/Nethris.Api/Domain/HrisRules.cs) and its use in [HrisPeople.cs](../../../server/Nethris.Api/Domain/HrisPeople.cs). The helper follows manager links, rejects cycles, bounds chain depth, and rejects references absent from the supplied map. The save operation checks every employee's resulting chain because a change can deepen descendants too.

## Understand the traversal state

The visited set starts with the employee being validated. The traversal begins at the proposed manager. Each manager must be newly added to the set. If an identity is already present, the chain has returned to the employee or another previously visited node and forms a cycle. The dictionary supplies the next manager until a null root is reached.

Starting with the employee already visited makes self-management fail immediately. It also makes a longer loop such as A managed by B, B by C, and C by A fail when A is encountered again. A depth bound is separate from cycle detection. A long acyclic chain can still exceed the workshop's supported depth. Write separate fixtures so one rule does not accidentally stand in for the other.

## Why descendants matter

Imagine a chain rooted at employee R with thirty descendants below it. Moving R under another chain can make the deepest descendant exceed the supported depth even if R's own chain remains short enough. Checking only the edited employee would miss that consequence. The source builds a map with the proposed relationship and validates every entry before accepting the change.

This is a global invariant enforced during a local edit. The supported employee limit makes a whole-map check a deliberate bounded approach. Do not replace it with a clever incremental algorithm until you can state which affected descendants must be revalidated and demonstrate equivalent behavior. Simplicity has value when the supported scale and correctness requirements are clear.

## Existence and eligibility are distinct

ManagerChain verifies that the referenced manager identity exists in its map. The save operation separately checks that an assigned manager is not terminated. Existence does not imply eligibility. Likewise, a valid acyclic graph does not prove that the selected person has an active manager-capable account for PTO routing; that later workflow performs its own account and role check.

This distinction helps explain apparently surprising outcomes. An employee can have a non-terminated manager relationship while a new PTO request routes directly to HR because no eligible active manager account is available. The organization relationship and approval routing are related, but their predicates are not identical. Trace each before describing the behavior as inconsistent.

## Exercise NH-05A: draw five graphs

Create a root-only employee, a three-person chain, a self-cycle, a three-person cycle, and a missing-manager reference. Predict the first failure category for each. Then create an acyclic chain at the supported depth and one beyond it. Label depth as manager links so the starting employee is not accidentally counted as an extra level.

Your diagrams should include the exact dictionary entries used by the helper. A picture without a missing node's dictionary status can be ambiguous. Keep the fixture small except when investigating the depth boundary. Use stable synthetic identifiers or labels mapped to GUIDs so the result is easy to inspect.

## Exercise NH-05B: demonstrate descendant invalidation

Construct two chains and propose attaching the root of one below the other. Choose lengths so the edited root remains within the limit while a descendant exceeds it. Predict why validating only the edited employee would accept an invalid global result. Then show how iterating every map entry detects it.

For a stronger answer, identify which subset of employees would need checking in a proposed incremental algorithm. Explain how you would discover descendants and avoid relying on stale relationships. Do not implement the optimization as part of the source-reading exercise unless you have a measured need and a comprehensive equivalence test.

## Exercise NH-05C: connect the graph to organization display

Read the organization-chart route in [HrisApi.Reads.cs](../../../server/Nethris.Api/HrisApi.Reads.cs). It loads a bounded employee set, validates manager chains, groups children by manager identity, and recursively projects roots. Explain why graph validation occurs before recursive rendering. A cycle could otherwise prevent termination or leave nodes disconnected from the expected root structure.

Investigate how the employee bound is detected by fetching one more than the supported maximum. This distinguishes an exactly full valid result from a truncated oversized dataset. A simple Take(maximum) followed by rendering would hide additional records. The extra-row technique is a small but useful completeness check.

## Review standard

A strong answer separates cycle, missing-reference, eligibility, depth and total-size rules. It demonstrates why a local edge change can violate a descendant's constraint and connects validation to safe recursive projection. The final artifact should be a set of labeled graphs and a source-based explanation of the exact rejecting boundary for each.

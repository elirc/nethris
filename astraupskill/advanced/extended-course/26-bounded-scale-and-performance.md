# 26. Bounded scale and performance

The workshop declares limits for employees, departments, job roles, PTO requests, manager depth, page size and request bytes. These limits make its intended learning workload explicit. Read [HrisRules.cs](../../../server/Nethris.Api/Domain/HrisRules.cs), [HrisPeople.cs](../../../server/Nethris.Api/Domain/HrisPeople.cs), [HrisPto.cs](../../../server/Nethris.Api/Domain/HrisPto.cs), and [HrisApi.Reads.cs](../../../server/Nethris.Api/HrisApi.Reads.cs). A useful performance investigation starts with supported scale rather than assuming an enterprise workload.

## Identify costs by path

An employee save can load the manager map and validate every chain. A directory read applies predicates, orders and pages a query, then projects related names and counts. A client refresh additionally gathers reference collections. An organization chart loads a bounded complete graph and recursively projects it after validation. These paths have different cost profiles and should not be optimized under one vague label of slow page.

Choose a specific user-visible interval: opening an employee editor, applying a search, saving a manager change, or refreshing an organization chart. Name start and end observations. Include data size, relationship shape, warm or cold state, and whether the run uses a packaged build or development tools. Without that context, a timing number is difficult to reproduce or compare.

## Shape matters as well as count

Two thousand employees arranged in shallow teams differ from a smaller set with long manager chains. A query with many equal names exercises tie-breaking. A department with many employees affects loaded relationship collections and headcount projection. A performance fixture should represent the structure relevant to the path, not only repeat one generic employee two thousand times.

Keep fixtures valid. A graph beyond the depth bound may be rejected before the display work you intended to measure. Such a rejection test is useful but answers a different question. Separate supported-workload timing from invalid-input handling. The report should identify which one was measured.

## Avoid optimizations that weaken correctness

Skipping descendant manager validation could make saves faster while accepting unsupported chains. Returning only the first reference page could make refresh faster while making valid selected employees unavailable. Removing stable ordering could make a query simpler while causing inconsistent pages. A performance change must preserve the contract or explicitly propose a changed product limit.

Before optimizing, write the correctness oracle. For manager validation, compare accepted and rejected graph cases. For selectors, compare the complete expected identity set within the supported maximum. For pagination, compare ordered pages and totals. A faster result with missing records is not an improvement under the existing feature contract.

## Exercise NH-26A: design a workload matrix

Choose small, typical and maximum-supported datasets for one path. Add a structure variation such as shallow versus deep manager chains or many equal names. Define repeated runs and report a distribution rather than one best timing. Keep unrelated background work and environment changes out of the comparison where possible.

If you cannot run the measurement, submit the plan as a plan. Do not invent timings to fill a report table. A well-designed experiment with explicit expected observations is useful preparation, but it is not executed performance evidence.

## Exercise NH-26B: compare selector strategies

Compare current bounded full-reference loading with the proposed searchable selector. Evaluate initial request count, total transferred records, selected-value preservation, server query complexity and stale-response handling. Use the actual maxima to estimate the workload, then identify which estimates need measurement.

A searchable selector may improve large-list usability even if raw network time is acceptable. Conversely, it introduces more interaction states. Recommend it based on a concrete user problem and a complete selected-value contract, not merely because remote search sounds more sophisticated.

## Exercise NH-26C: review a manager-check optimization

Propose checking only affected descendants after a manager change. Define how descendants are found, what baseline graph is authoritative, and how the proposed edge is applied. Create an independent full-check oracle using the existing bounded approach, then compare results over small generated valid and invalid graphs in a disposable harness.

This is a meaningful property experiment because it tests an optimization against a simpler known contract. Do not use the optimized algorithm to generate its own expected results. Include cycles, missing references and depth-boundary cases, and record the supported input domain.

## Review standard

A strong performance report names a user interaction, a representative workload and a correctness invariant. It distinguishes estimates from measurements and supported data from rejection tests. The final recommendation may be to keep the simple bounded design; that is a valid engineering outcome when evidence does not justify extra complexity.

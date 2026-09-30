# 17. Pages, search, and complete selectors

Read the page request parsing and employee query in [HrisApi.Reads.cs](../../../server/Nethris.Api/HrisApi.Reads.cs), then `allRows` in [workshopApi.ts](../../../client/src/workshopApi.ts). The directory view requests a page, while reference selectors gather a bounded complete collection through multiple requests. These are different use cases. A page that correctly shows twelve records is not a complete selector for a database containing more employees.

## Query order defines page meaning

The employee query applies search and the supported department filter before sorting and pagination. It orders by last name, first name and stable identity. The final identity tie-breaker makes employees with equal names deterministic within the query. The total is computed from the same filtered query rather than from the paged subset. This lets the client distinguish total matches from the number currently displayed.

Filtering after pagination would produce misleading empty pages and totals. Suppose the first twelve unfiltered employees contain no matching department but later employees do. Client-side filtering of only that page would incorrectly suggest no matches. Write the expected identity set before implementation so a server-side filter exercise is assessed on the full query semantics.

## Applied search and typed search are separate

The client keeps current search input and an applied filter term separately. Submitting search copies the term and resets offset to zero. This avoids staying on a later page whose offset no longer fits the smaller result set. The distinction also lets the learner type without issuing a new request on every keystroke under the current workflow.

A future live-search design would introduce different timing and cancellation decisions. It should preserve stale-read suppression and a clear relation between displayed results and the active term. Do not call a search implementation broken merely because it intentionally requires submission. Evaluate it against the declared interaction contract.

## Reference loading has a supported maximum

allRows requests pages of one hundred until the accumulated count reaches the reported total. It bounds the loop by a caller-supplied supported maximum and throws when the selector exceeds that supported size. The workshop calls it with two hundred departments, two hundred job roles and two thousand employees. This is a completeness guarantee within declared limits, not an unlimited directory loader.

Separate API requests can observe changes between pages. The current client helper does not itself create a cross-request snapshot. If concurrent edits shift sort order during collection, a stronger consistency design may need a versioned snapshot or another pagination contract. Treat that as a specific limitation or extension question rather than assuming offset pagination has an immutable dataset.

## Exercise NH-17A: build a page fixture

Create enough fictional employees to cross a page boundary, including equal first and last names with distinct identities. Choose a search term and department filter whose matches occur across the original ordering. Write expected total, ordered identities and page contents. Verify filtering occurs before pagination and the identity tie-breaker is retained.

Include an offset beyond the final match. An empty items list with a nonzero total can be a valid page response for that offset. The client may choose to reset or navigate back, but the server should not invent records. Distinguish no matches from no items at this offset in your reasoning.

## Exercise NH-17B: test selector completeness

For allRows, simulate a collection of 150 records returned as one hundred and fifty. Assert that the final fifty are available to a selector and that a selected identity on the second page can be represented. Then simulate a total above the supported maximum and predict the explicit refusal instead of silent truncation.

Keep response metadata internally consistent in the basic test. Add inconsistent totals as a separate robustness investigation and state what the current helper actually does. Do not claim it validates every possible malformed page response unless you can point to those checks.

## Exercise NH-17C: propose searchable selectors

Replace full reference loading with a future searchable selector. Define how the current selected value remains visible even when it is not in the latest search page. Preserve inactive existing department or role selections while limiting new choices according to domain rules. Handle stale search results with a relevance mechanism.

Your design must not confuse absence from a page with deletion. A selected employee may simply be outside the current search. Fetching the selected identity separately is one possible solution, but define authorization, loading and missing-record feedback. A useful selector combines partial search results with an explicit selected-value contract.

## Review standard

A strong answer distinguishes page size, total matches, supported collection maximum and selected-value identity. It verifies query order and completeness, and it recognizes that multi-request collection is not automatically a snapshot. The final artifact should include expected page tables and a selector proposal that preserves current values without offering invalid new assignments.

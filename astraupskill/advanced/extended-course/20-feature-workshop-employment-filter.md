# 20. Feature workshop: employment-status filter

This is a proposed learner feature. The current employee read path supports text search and department filtering; the employment-status filter described here must be implemented and verified separately. Read [HrisApi.Reads.cs](../../../server/Nethris.Api/HrisApi.Reads.cs), the status enum in [Enums.cs](../../../server/Nethris.Api/Domain/Enums.cs), and the search state in [Workshop.tsx](../../../client/src/Workshop.tsx).

## Define the query contract

Choose a query parameter that accepts exactly Active, OnLeave or Terminated under a documented case policy, with absence meaning all statuses. Decide whether an empty value means absence or invalid input. Reject unknown values rather than silently ignoring a misspelled filter, because ignored filters can make the result look complete while showing the wrong population.

Apply the predicate to the employee query before Count, ordering and pagination. Preserve existing search and department predicates as intersections. The response total should describe the combined filtered set. Do not filter only the returned page in the browser; that would produce misleading totals and hide matches on later pages.

## Keep stable ordering and offset behavior

Retain the current last-name, first-name and identity ordering. A status filter does not remove the need for a stable tie-breaker. In the client, changing the applied status should reset offset to zero, just as applying search does. Otherwise a learner on a later page can see an empty result despite valid matches near the start.

Decide whether status changes apply immediately or only with the search form submission. Both can be implemented, but the displayed controls and result context must agree. If immediate, preserve generation-based stale-read suppression when the learner changes filters quickly. If submitted, distinguish current control draft from the applied filter in your state model.

## Compose filters deliberately

Create a fixture with multiple statuses in multiple departments and overlapping name terms. A test of status alone cannot establish that composition works. For example, search for Lee in department D with OnLeave should return only the intersection. The total and every item should agree with that set. Include equal names and a page boundary so ordering and pagination remain visible.

Direct record reads deserve an explicit decision too. The current read helper builds a query with search and department conditions before applying a requested identity. If you add status to the same query, a direct record request with a mismatching status could return missing. Decide whether that is the intended contract or whether filters should apply only to collection routes. Document and test the choice rather than accepting an accidental side effect.

## Exercise NH-20A: write a query truth table

List absent, each valid status, empty, unknown, wrong-case under your chosen policy, repeated parameter and numeric-looking value. Specify accepted meaning or rejection. Then combine a valid status with search and department. The table should identify which parser owns the rule and which error response the client receives.

Avoid a parser that falls back to a default enum value when parsing fails. That can turn invalid input into Active and produce a plausible but incorrect result. The negative test should verify a rejection rather than merely that some records are returned.

## Exercise NH-20B: implement the query before the control

Add and test the server contract first with direct requests. Use an independent expected identity set derived from the fixture. Verify total, items and order for two pages. Then add the client control and prove it serializes the selected value and resets offset. This sequencing makes it easier to distinguish query bugs from UI state bugs.

Do not broaden the feature into a general query language. A small explicit status predicate is enough for the learning objective. If you later add multi-select status, define whether repeated parameters or a list syntax represents the union, and retain clear validation of every member.

## Exercise NH-20C: test a stale response

Delay an Active response, switch to OnLeave, allow the OnLeave response to publish, then release the old Active response. The visible page should remain associated with the current applied filter. This reuses the generation concept but exercises a new user intent. Assert the control value and displayed identities together so an inconsistent label/result combination cannot pass.

Include a failed latest request and decide what feedback should remain. The source's existing refresh behavior may clear or retain particular page state; describe the actual behavior before proposing improvements. A filter feature should not accidentally expose an old result as if it belonged to the new selection.

## Review standard

A strong implementation enforces a precise query vocabulary, composes predicates before pagination, preserves stable ordering, resets offset and suppresses obsolete results. Its report should show a multi-filter fixture and direct invalid-query tests, not only a screenshot of a new dropdown.

# Compare decisions and evidence

For HRIS-01, the description must travel through every layer that owns its meaning. A form field alone cannot persist it. A database property alone cannot make it available in the response projection. Use the shared optional-text rule and make the field's maximum explicit. That rule maps whitespace-only input to null and otherwise trims before checking length and control characters. A tab surrounding ordinary text can disappear during trimming; a tab inside the retained text is rejected. With replacement-style inputs, clearing and omission must follow the chosen input contract rather than a hidden patch convention.

For HRIS-02, compose one filtered query and derive both page membership and total from that predicate. The current employee reader orders by last name, first name and ID. Keep a stable tie-breaker when extending it. Count may execute after materializing the page in source order; the requirement is that it counts the filtered query, not the page slice. Do not accidentally demand an unnecessary execution order when describing the semantic rule.

For HRIS-03, bound the search result and keep the current selection independently resolvable. A dropdown with 100 rows is not proof that the selected manager does not exist elsewhere. On save, revalidate the reference and hierarchy against authoritative current data. Client-side choices are helpful interaction, not a replacement for server rules.

For HRIS-04, distinguish operation recovery from session recovery. If the operation revokes the session used to authenticate its retry, ordinary replay may be unavailable. Explain a recoverable user interaction and identify where additional design/testing is needed. Do not include secrets in a notebook merely to make the demo easy to replay.

## Answers to the debugging predictions

A missing required `If-Match` produces 428 in the named scenario test. A stale well-formed expectation produces 409 in this implementation. An exact retry of a successfully committed operation replays its receipt while it remains eligible under the request/receipt contract. A changed fingerprint with the same key produces 409. These outcomes require the other prerequisites—such as valid authentication—to hold; a request can fail earlier for another reason.

In the transaction failure lab, an earlier `SaveChangesAsync` can be rolled back when it participates in the explicit transaction that has not committed. Check the rows after failure. An in-memory object retaining an assigned value is not proof that its database row committed, and a thrown exception alone is not proof that no row committed.

## Feature review rubric

Score each area 0 for absent, 1 for explained, 2 for demonstrated with reproducible evidence:

| Area | What a full score requires |
|---|---|
| Contract | Input normalization, omitted/blank meaning and response examples |
| Cross-layer completeness | Entity, DTO, domain, UI and query/projection agree |
| Rejected state | Invalid/stale writes preserve database and useful browser draft |
| Recovery | Exact retry meaning and uncertain response behavior are shown |
| Evidence boundary | Commands, observed results, skipped work and limits are separated |

Aim for 8/10 with no zero in rejected-state behavior. A reviewer should be able to rerun the critical example without your personal database. If a check needs a packaged build or installed browser, name that prerequisite instead of treating setup failure as a test result.

## One useful review comment

“The status predicate is applied to `page.items`, so a matching employee on page two of the unfiltered result is never considered. Move it into the server query used for both page and total, and add a fixture that puts a match beyond the original first page.” The comment contains a trigger, consequence, repair boundary and verification case. Use that structure in your own reviews.

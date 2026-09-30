# Review guide 3: features and verification

Use this guide after chapters 17-24. These exercises move from reading current behavior to specifying and implementing new features. The distinction must remain explicit: status filtering, department descriptions and searchable selectors are proposals until you deliver and verify them in your own practice branch. A good answer can be a complete design without pretending that the application has already changed.

## Pages and selector completeness

For NH-17A, derive the complete matching employee identity set first, order it by last name, first name and identity, then take the requested page. The reported total comes from the filtered set before paging. This independent sequence gives you a clear oracle. Filtering the first page afterward can omit valid later matches and produce a misleading total, so include a fixture where the two strategies visibly differ.

An offset beyond the matching set can legitimately return no items with a nonzero total. That observation should not be mislabeled no employees exist. The client may reset offset when a filter changes or provide navigation back. The server response still needs to represent the requested page accurately. Test server query behavior and client offset policy separately.

For NH-17B, a 150-row reference collection requires two requests under the current one-hundred-page helper. The final selected identity may be in the second response. An assertion that only checks the first hundred cannot establish selector completeness. At a total above the supported maximum, the helper's explicit refusal is preferable to silently presenting a truncated list as complete under the current contract.

For NH-17C, selected identity and search result membership are separate. Keep the selected record's authorized display representation even if it is not in the latest page. An existing inactive department may remain selectable as the current value while unrelated inactive departments are excluded from new choices. A missing selected record requires explicit feedback; absence from a search result alone is insufficient evidence of deletion.

## Form conversion and correction

For NH-18A, numeric draft text is converted with Number at submit, checkboxes become booleans, blank optional fields become null, and identity selectors submit their chosen identifier. Server rules still validate the resulting values. Account edit's blank new-password field maps to null so the existing password is retained; it should not be confused with accepting an empty password as a valid new credential.

The operation body also depends on action. A PTO decision uses decision and comment. Cancellation uses an empty object. Deletion omits body. Exact fingerprint behavior makes these details relevant to retry. A test that reconstructs a body from a different field set during retry can change the request even if the visible user action appears unchanged.

For NH-18B, duplicate-name rejection should preserve the editor and allow a corrected new intent. The new intent receives a new operation key. A stale conflict instead requires comparison with current saved values. Both should preserve valuable text, but their recovery actions differ. A single test named error recovery should not blur these two paths.

For NH-18C, labels and alert/status roles are useful markup evidence but not a complete keyboard journey. Record where focus goes, whether disabled fields are explained, and whether comparison or retry controls can be reached. A concrete improvement has an observable acceptance example, such as focus moving to an explanatory heading after a stale response. Avoid claiming an accessibility result you did not observe.

## Department description feature review

For NH-19A, the ticket needs normalization, omission, clearing, size and control-character semantics. If you reuse OptionalText, surrounding whitespace can be trimmed before control validation and whitespace-only input becomes null. Do not write a test expecting every surrounding tab to be rejected while using that helper. An intentional multiline description would require a different contract because embedded newlines are controls under the existing rule.

The compatibility choice is part of the feature. An older full-update client omitting the new field can clear it if omission becomes null. Coordinated deployment is one possible workshop policy, but it should be stated. Presence-aware update semantics are another, with additional contract complexity. The ticket should not leave the server to guess whether omission means unchanged or empty.

For NH-19B, the vertical slice includes domain model, request/view contracts, normalization, persistence handling, form, display and readback. The accepted normalized value should be observed after a fresh request or reload. Seeing it in the draft is not persistence evidence. A direct request with an embedded control establishes server enforcement independently of browser constraints.

For NH-19C, inherit the existing transaction, ETag and receipt protocols. A late injected failure must not leave the description or version accepted without audit/receipt. An exact retry must replay, while changed body under the old key conflicts. Schema handling must be tested on the stated source format. A fresh database proves new-schema operation but not old-database migration.

## Status filter feature review

For NH-20A, choose and document case, empty and repeated-parameter semantics. Unknown values should not quietly become a default status. A parsing failure that returns the enum's default can produce plausible Active results and hide invalid input. The negative test should assert rejection. Absence should retain the declared all-status behavior unless the product deliberately chooses another default.

For NH-20B, the status predicate belongs before Count, ordering and pagination and composes with search and department. Use an independent expected intersection. Verify at least two pages and equal-name tie-breaking. Then verify the client serializes the applied value and resets offset. A screenshot of a dropdown cannot establish any of these query properties.

Direct identity reads need an explicit choice. If the new predicate is added to the shared query before identity lookup, a mismatching status parameter can make an existing record appear missing. That may be acceptable under a documented contract, or you may restrict filters to collection routes. Either way, include the case rather than accepting an accidental behavior unnoticed.

For NH-20C, delay an old Active response and resolve the newer OnLeave response first. When the old response arrives, both the visible applied filter and displayed identities should still describe OnLeave. An assertion of only one side can miss an inconsistent control/result pair. The same principle applies to error publication from obsolete requests.

## Schema and preservation

For NH-21A, a nonexistent path is eligible for initialization; a known compatible file is reopened; an existing empty or wrong-marker file is refused. Matching markers do not guarantee valid required settings, so malformed signing material or missing settings can still fail. Preserve the file and investigate rather than manually changing markers to bypass the check.

For NH-21B, rehearse on a copy of a synthetic database and preserve the original. Compare identities, relationships, settings and relevant history after migration. A row count alone can miss swapped or lost associations. Do not print signing material; an equality assertion or protected local check can establish preservation without exposing it. A rollback plan should identify a known preserved artifact or supported reverse migration.

For NH-21C, operator guidance should distinguish selecting a new fictional dataset from migrating existing work. Automatic reset is not a harmless response to incompatibility. The current constructor deliberately refuses unknown files. A learning prototype can state that it supports a fresh database only, but it should not advertise existing-data compatibility without evidence.

## Matching tests to claims

For NH-22A, OptionalText behavior belongs in a pure rule test; missing If-Match belongs at the HTTP/helper boundary; EF stale updates need real storage mapping; response redaction needs a projection or HTTP test with actor context; browser draft retention needs component/browser observation; late rollback needs an integration boundary that includes the outer transaction. Complementary tests can connect layers without duplicating every case everywhere.

For NH-22B, repair missing prerequisites before asserting the intended negative result. A stale edit without an operation key may fail before Expected or Version. A valid key, authorized actor, JSON body and current references let the test reach the stale comparison. A current-version control proves the candidate otherwise fits the domain. Then inspect the accepted winner through a fresh read.

For NH-22C, report helper passes, unexecuted HTTP scope and manual browser instructions separately. Do not use skipped as a synonym for never invoked if a runner did not report skips. A precise command and environment record lets another learner reproduce the result. Documentation-only changes may require source review rather than full execution, but behavioral implementation claims need the relevant tests.

## Failure experiment review

For NH-23A, a pre-domain failure leaves no business mutation; a late pre-commit failure must roll back attempted writes; post-commit response loss can leave accepted effects and receipt with an uncertain client. Your fixture must establish the intended injection point. Rejecting malformed JSON does not prove late rollback, and discarding a successful response does not prove every possible network failure was simulated.

For NH-23B, middleware categories guide client recovery. Known lower-status rejections can clear pending for correction, while status zero and server-error uncertainty preserve exact retry under the current client logic. A saved mutation followed by failed refresh remains known saved. Include the failed call's identity in the report so one red network entry does not erase earlier successful acceptance.

For NH-23C, inspect business effects after exact replay: one created department or one final PTO deduction, expected version and approval history, and scoped audit/receipt evidence. Then change fingerprint material under the same key and expect conflict. This negative control demonstrates that the harness is sensitive to request identity rather than merely repeating a successful route.

## Audit interpretation

For NH-24A, derive labels and target identities from the actual mutation route and Describe logic. Specialized setup/authentication paths should be inspected separately before counting every audit row as an ordinary CRUD mutation. Establish a scoped baseline because fixture initialization can already create audit records. A count over the whole database without context is a weak oracle.

For NH-24B, a richer audit summary should use accepted normalized values and a defined size bound. Decide whether recording full description text is necessary for the audit question. A hash identifies content equality under an encoding but does not provide readable history. A before-and-after snapshot is a new stored capability; do not claim the current metadata log already reconstructs every field.

For NH-24C, pair request summary, accepted response, fresh state, audit and receipt. Each artifact supports a different claim. A rejected operation should not create the same accepted-action evidence. The log's name does not make it tamper-proof or a complete event-sourced history. State the implemented application semantics and any stronger operational proposal separately.

## Feature-review handoff

A complete handoff explains the current problem, the chosen contract, the source changes and the evidence. It includes one ordinary success, one direct invalid request, one stale or uncertain recovery path and any schema compatibility limit. The reviewer should be able to assess the feature without reading the author's conversation history or trusting a broad claim that everything works. Precision in the report is part of the feature's quality.

# 12. ETags and stale-edit recovery

Nethris represents a record precondition as a strong ETag containing the record identity and positive version. Read `ETag` in [HrisApi.cs](../../../server/Nethris.Api/HrisApi.cs), `Expected` in [HrisRequest.cs](../../../server/Nethris.Api/HrisRequest.cs), and `Version` in [HrisRules.cs](../../../server/Nethris.Api/Domain/HrisRules.cs). The client constructs the matching representation in [workshopApi.ts](../../../client/src/workshopApi.ts).

## Parsing and comparison are separate

Expected checks that the header begins with the quoted compact target identity, ends with a quote, and contains a positive parseable version. Missing or malformed preconditions produce the workshop's 428 precondition error. Once a version has been extracted, the domain compares it with the record's current version. A well-formed stale version produces conflict behavior, commonly 409 in this application.

Do not substitute a familiar status code from another API when teaching this one. The exercise is to predict the actual contract. An absent header and a stale valid header are different cases and should have separate tests. A header for another record identity can be malformed relative to the requested target even if its numeric version is current.

## Version advancement belongs to persistence preparation

The database context marks versioned entities as concurrency tokens and prepares versions before saving. Added records start at one. Modified records advance from their original version with checked arithmetic. Some domain operations explicitly mark the version property modified to make an accepted update advance even when ordinary field tracking might otherwise see no changed value.

This means a record version is not simply a client counter. The client submits the version it observed; the server establishes the next accepted version through the persistence path. A stale form should not increment its own version and hope that the number becomes acceptable. The number expresses the basis of the edit, not a ticket that can be guessed.

## The recovery interface preserves a draft

The Workshop client handles a 409 for an existing editor by trying to load the current saved row. If the version differs, it presents saved and draft values for comparison and offers a deliberate use-latest-version action while keeping the draft. This is an explicit user decision. It is different from silently refreshing the precondition and resending old values without review.

Not every 409 necessarily means a stale version. A uniqueness or relationship conflict can also produce 409. The client compares the loaded saved version with the editor's version to decide whether the stale state remains. Read this detail before claiming every conflict triggers identical recovery. Error categories and current state together shape the next action.

## Exercise NH-12A: build a header matrix

For one known target, include missing header, weak-tag-like input, wrong identity, zero version, nonnumeric version, valid current version and valid stale version. Predict whether rejection occurs during precondition parsing or domain comparison. Keep the body, operation key and authorization valid so those earlier checks do not hide the precondition behavior.

Include a successful control and inspect the returned ETag and version. Do not prove only that the request did not throw. The accepted response should identify the new record state under the server's version policy. Use the actual helper to understand syntax, but compute the expected version transition independently.

## Exercise NH-12B: reproduce two editors

Open the same record at version four in two sessions. Accept A's edit as version five, then submit B's candidate with version four. Record B's retained fields, current saved fields, and response. Explain which accepted values would be lost if B automatically resent its full body against version five.

Now use the deliberate comparison workflow. The learner may choose to keep selected draft changes after reviewing current data. A new operation should represent that new intent. The old idempotency key belongs to the old captured request and should not be reused with a changed precondition or body.

## Exercise NH-12C: test a non-stale conflict

Keep a row's version current but submit a duplicate department name or another validly shaped domain conflict. Predict the 409 and the result of loading the current row. Explain why the editor should not imply that another user necessarily changed this record. The version comparison can remain equal while the requested business state is invalid.

Write an improved error explanation for this case if needed, keeping it as a proposal until implemented. Good recovery guidance should identify the relevant rule and preserve the form, rather than telling the learner to reload repeatedly until a duplicate name becomes acceptable.

## Review standard

A strong answer separates header syntax, target identity, current version, persistence advancement and user recovery. It tests 428 and stale 409 independently, includes a non-stale 409, and preserves the relationship between a captured operation key and its original precondition. These distinctions prevent retry code from silently defeating concurrency protection.

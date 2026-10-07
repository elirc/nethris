# Solutions, reasoning and a review standard

Use this guide after attempting [the exercises](05-PRACTICE.md). The goal is to explain why an implementation preserves the system's contracts. Matching a snippet without understanding its transaction or permission boundary is not sufficient.

## Exercise 1: department trace

The editor derives fields from the department field definition in [Workshop.tsx](../client/src/Workshop.tsx). Submission normalizes optional blank inputs to null, serializes the body once, and captures the operation key and optional ETag in [workshopApi.ts](../client/src/workshopApi.ts). HrisApi authenticates and dispatches to HrisPeople.SaveDepartment. That method validates a name, checks case-insensitive uniqueness, checks the lead's department, saves the row and projects the response.

Headcount is derived from employee relationships. Accepting a client-supplied headcount would create a second source of truth. Valid creation returns 201. A duplicate name returns 409. An existing-record update without a valid strong If-Match returns 428. Rejected mutations should not commit domain changes, an audit entry or a success receipt.

## Exercise 2: optional description

A reasonable input/property pair is string? Description. Normalize with HrisRules.OptionalText(input.Description, "Description", 250). Add the normalized value to the entity and output DTO. Show the field in both the editor and relevant record display. Keep it outside account or authentication code.

The migration question is part of the solution, not optional cleanup. The current database initializer creates an identified fresh schema; it does not maintain a migration history. A practice-only implementation can require a fresh path and clearly state that limitation. A migration implementation needs a reviewed copy, an explicit version transition and tests proving existing records survive.

Useful tests include whitespace-only input becoming null, exactly 250 characters being accepted, 251 being rejected, and a stale PUT failing after another session changes the description. A test that simply creates an entity and reads the same property from that in-memory object misses the database and HTTP boundaries.

## Exercise 3: status filter

Parse and validate the requested enum, then compose it into the IQueryable before CountAsync and the ordered page query. Use the same filtered query for the total and items. Changing the filter in React should reset offset to zero. A response-generation guard should prevent a slower old search from replacing a newer result.

Create a fixture where matching employees fall beyond the first unfiltered page. Ask for a small page and assert its total, returned identifiers and absence of nonmatching statuses. Add an invalid-value test. This catches both UI-only filtering and inconsistent total counts.

## Exercise 4: password change

A good design treats password verification, hash replacement, session revocation and auditing as one transaction. The audit describes the action without recording credentials. New hashes use the current PBKDF2 settings. The server should reject an incorrect current password without changing anything.

If the current session is revoked, a lost response is awkward: the caller cannot use that session to recover a receipt. Do not promise ordinary authenticated receipt replay unless your design actually supports it. A simple documented flow can require signing in again with the new password, then the old password if the result is uncertain. A more sophisticated recovery protocol needs its own security review and tests. This is a useful example of a feature where reusing an existing helper blindly produces misleading UX.

## Exercise 5: selector behavior

A searchable selector should return a bounded list of identity/name pairs, not every employee DTO. Keep the selected record separately from the search results. When an old response arrives after a newer query, ignore it. Validate the chosen ID and manager graph during the write transaction even if the browser previously displayed it as valid.

A test should arrange two queries with reversed response order and assert that the newer result remains visible. Another should select a manager who becomes terminated before save and assert a server rejection with the draft preserved.

## Exercise 6: refund reasoning

A refund needs explicit identity linked to the original approved request. Storing only a changed balance provides no explanation for why hours returned. Enforce a uniqueness rule for the refund identity, and perform the history insert, state transition, balance restoration, audit and receipt within one transaction.

Review concurrent requests separately from retries. Two different operation keys may compete for one refund; one must win and the other must observe the closed state or uniqueness constraint. The same operation key should recover the already committed result. A version protects stale intent but is not a substitute for refund uniqueness.

## What a mid-level review should include

A strong review describes a concrete trigger and before/after behavior, identifies the authoritative permission check, explains the transaction boundary, and names the tests that prove failure behavior. It acknowledges scope limits without inventing production readiness. For a small feature, a clear paragraph plus test evidence is usually more useful than a long list of every file touched.


## Exercise 7: stale-edit review

To tell "fields the other writer changed" from "fields I changed", keep three values per field: the snapshot the editor opened with, the draft, and the latest saved row. A field conflicts only when *both* sides moved away from the snapshot. The current panel cannot make that distinction, because recovery replaces `editor.row` with `latest` (`Workshop.tsx:163`). Keep the opening snapshot in its own state. "Discard my draft" should set the draft from `latest` and clear `stale`. It should not issue a request.

## Exercise 8: surviving a reload

Persist the `Operation` object, which already holds the frozen body, key and ETag (`workshopApi.ts:6`, `25-26`). Never persist the token: the README's design keeps sessions in memory on purpose. `sessionStorage` matches the tab-scoped session model. After a reload the user must sign in again, so the replay happens under a new session but the same account. That works because receipts are scoped by account and key (`HrisApi.Mutations.cs:31`), not by session. Delete the stored operation on any definite response. A 409 for a changed fingerprint is a definite response too: it means the draft changed, not that the outcome is unknown.

## The red-first lab: reasoned answers

These were derived by reading the guards and tests on 2026-10-06, not by running the mutations. Your run is the evidence. If your result differs, trust the run and work out why.

| Guard disabled | Expected red | Why |
|---|---|---|
| `Idempotency-Key` UUID check | `AMutationWithoutAValidIdempotencyKey_IsRejected` only | The request runs under the empty key and returns 201. The replay and key-reuse tests send valid UUIDs, so they do not notice. |
| 428 replaced by `return 0` | `AnUpdateWithoutIfMatch_Is428` only (it now gets **409**) | Version 0 never matches a stored row (versions start at 1, `HrisDbContext.cs:112`), so `HrisRules.Version` raises a conflict. The stale test sends a well-formed tag and still gets its 409. The guard's real value is the *diagnosis*, not the refusal. |
| `audit-logs` role minimum | `AnEmployeeCannotReadTheAuditLog` only | `AuditLogs` (`HrisApi.Reads.cs:202-210`) has no role check of its own. `MinimumRole` is the only gate. |
| Session deletion on logout | `ASignedOutTokenIsRejectedImmediately` only | It is the only xUnit test that signs out. `check_api.py`'s "logout revokes the server session" covers the same ground over a real socket. |

Every row turns exactly one test red. Each guarantee depends on a single test, which is the finding to write in your review.

## The code behind three of these answers

Exercise 2's normalizer is two lines, and both of them matter (`server/Nethris.Api/Domain/HrisRules.cs:17-18`):
```csharp
public static string? OptionalText(string? value, string field, int maximum = 250) =>
    string.IsNullOrWhiteSpace(value) ? null : Text(value, field, maximum);
```

Whitespace-only input becomes `null` rather than `" "`, so "absent" has exactly one representation in the database - which is why the suggested test "whitespace-only input becomes null" is a real test and not a formality. Everything else delegates to `Text`, so the length rule is not duplicated per field. A field added with `input.Description?.Trim()` instead would store `""` for a blank form and quietly create a second way to mean "no description".

Exercise 7's precondition check is where the 428-versus-409 distinction actually lives (`server/Nethris.Api/HrisRequest.cs:34-45`):

```csharp
public static long Expected(HttpContext http, Guid id)
{
    var tag = http.Request.Headers.IfMatch.ToString();
    var prefix = $"\"{id:N}-";
    var wellFormed = tag.StartsWith(prefix, StringComparison.Ordinal) && tag.EndsWith('"');
    if (wellFormed && long.TryParse(tag[prefix.Length..^1], out var version) && version > 0)
    {
        return version;
    }

    throw new HrisError(428, "precondition", "Send the record's strong ETag in If-Match.");
}
```

Read what it does *not* do: it never compares the version with the stored row. This method answers only "did the client state a version for *this* record, in the right form?" - a missing, malformed, or other-record ETag is a **428**, a client-shape problem. The comparison with the stored version happens later, in `HrisRules.Version` (`Domain/HrisRules.cs:46-49`) against the loaded row, and a mismatch there is a **409**: a real conflict with another writer. Collapsing the two into one status is the most common way this contract is got wrong, and `ApiScenarioTests.AnUpdateWithoutIfMatch_Is428` and `AnUpdateWithAStaleIfMatch_Is409` exist precisely to keep them apart.

Exercise 3's filter composition follows the pattern the existing directory read already uses (`server/Nethris.Api/HrisApi.Reads.cs:87-118`): build one `query`, compose each filter into it (search at 90-95, `departmentId` at 97-105), and derive **both** the page and the total from that same variable:

```csharp
var rows = await query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.Id)
    .Skip(page.Offset).Take(page.Limit).ToListAsync();
var items = rows.Select(e => HrisPeople.View(e, actor)).ToList();
await HrisRequest.Send(http, new Page<EmployeeView>(items, await query.CountAsync(), page.Offset, page.Limit));
```

A status filter is one more `if (...) query = query.Where(e => e.Status == value);` placed beside the `departmentId` block. Mirror that block's validation too: an unparseable value throws `HrisError.Invalid` (422) instead of being ignored. Parse with `Enum.TryParse<EmploymentStatus>(raw, ignoreCase: false, out var value) && Enum.IsDefined(value)`, because `TryParse` alone accepts numeric strings such as `"7"`.

If `total` comes from a differently-filtered query, your pagination lies to the user - and nothing in a screenshot will show it.

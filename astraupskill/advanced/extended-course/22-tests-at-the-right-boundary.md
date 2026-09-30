# 22. Tests at the right boundary

The repository has pure rule tests, real SQLite foundation tests, and in-process HTTP scenarios. Read [FoundationTests.cs](../../../tests/Nethris.Tests/FoundationTests.cs) and [ApiScenarioTests.cs](../../../tests/Nethris.Tests/ApiScenarioTests.cs). Their value comes from the boundary each actually exercises. A passing numeric helper test does not establish HTTP header handling; an HTTP test does not automatically establish browser draft preservation.

## Pure rules need controlled inputs

Quarter-unit, money, PTO-date and manager-chain rules can be tested with small deterministic values. The PTO-date helper accepts today explicitly, so tests can use a fixed date. A manager graph can be represented by a small dictionary. These tests are fast and easy to explain because the expected result comes from the stated rule rather than a large environment.

Keep negative fixtures focused. A manager-cycle test should not also reference a missing node if the intended failure is the cycle. A PTO-span test should use valid quarter hours and a permitted start date. Otherwise the test can pass for an earlier unrelated rejection and leave the named rule untested.

## Foundation tests use the real storage model

The foundation suite creates a unique temporary database. It tests timestamp ordering in SQLite, inserting an approval with an assigned GUID, competing tracked edits, restart preservation of exact quantities, unknown-file refusal, and rollback after an earlier SaveChanges. These checks exercise EF mapping and database behavior that an in-memory list substitute would not establish.

For example, the competing-edit test loads one employee into two contexts, saves the winner, then expects a concurrency exception from the loser and reads the accepted winner through a fresh context. The fresh read is important. It verifies persisted outcome rather than inspecting one context's tracked object. The test's structure is a useful model for new versioned features.

## HTTP scenarios include the real request pipeline

ApiScenarioTests uses WebApplicationFactory with a unique Hris:Database path. It boots the actual application in process, including middleware, authentication and request-scoped persistence. This is stronger evidence for route contracts than calling HrisPeople directly with a handcrafted Actor. It still does not run the browser component or a deployed external server.

The fixture uses synthetic test credentials and initializes fictional colleagues. Tests should not depend on a real workshop database or account. When adding scenarios, preserve that isolation and avoid global configuration that could redirect the harness to personal data. A test's safety and reproducibility depend on its setup as much as its assertions.

## Exercise NH-22A: map claims to tests

Classify these claims: surrounding tabs are trimmed by OptionalText; missing If-Match produces 428; stale EF updates preserve the winner; salary fields are redacted for an Employee response; a browser retains a rejected description draft; a late audit failure rolls back the domain change. Choose the smallest test layer that can observe each claim and identify any useful complementary layer.

Do not force every behavior into one suite. A pure test can establish normalization exhaustively, while a small HTTP example proves that the route uses the contract. A browser journey can establish user recovery without enumerating every numeric boundary again. This layered approach makes failures easier to diagnose.

## Exercise NH-22B: strengthen a weak test

A test named stale edit is rejected sends an old ETag but also omits the operation key. Explain why it may never reach the version comparison. Repair the setup with valid authorization, key, media type and body, then assert the intended status and unchanged accepted record. Add a current-version control request to prove the candidate is otherwise valid.

Likewise, a redaction test should set a distinctive sensitive value and compare allowed and disallowed actors. If the fixture's value is already null or zero, the test may fail to distinguish real redaction from missing setup. Independent expected values make the assertion meaningful.

## Exercise NH-22C: report execution accurately

Write a verification note for a change where foundation tests passed, HTTP tests were not run because required dependencies were unavailable, and browser steps remain manual instructions. State each result separately. Do not call unexecuted tests skipped by the runner unless the runner actually reported them that way.

Include the exact command and working directory for executed checks, plus the scope of the changed feature. A documentation-only change may not require a full application run, but any claim that a new behavior works needs corresponding evidence. Match the verification to the claim rather than inflating it with unrelated green checks.

## Review standard

A strong test plan identifies the boundary, isolates the condition, uses a valid control and inspects accepted state where relevant. It distinguishes unit, storage, HTTP and browser evidence. The final report should let another reviewer tell exactly what ran, what passed, and what remains a proposed experiment.

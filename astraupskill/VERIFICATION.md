# Verification and practical limits

This page distinguishes delivered evidence from proposed exercises. The final machine-readable result is [evidence.json](evidence.json). It records the checks used for delivery, their commands and limitations. Read that file for exact passing counts and execution dates; do not infer completion from the presence of a test script.

## What was changed

The original .NET 8 / React prototype was reviewed at source level and run against a fresh temporary database. The review reproduced numeric-role output, failing PTO/audit ordering, HR-to-Admin account escalation, acceptance of a one-day 9,999-hour request and approval insertion failure. The original balance did not become negative during the failed approval; its SaveChanges rolled back.

The improved application uses .NET 10 and a locked React/TypeScript/Vite dependency tree. It has identified fresh database creation, private per-database signing material, server-revocable sessions and current-role authorization. Employee and account administration are separate. Department, job-role and employee CRUD use validation, versions and relationship protection.

PTO now validates dates, quarter-hour quantities, overlap and pending reservations. Decisions follow captured-manager and distinct-HR stages. Approved deduction, history, audit and receipt share a transaction. The browser uses real responses, preserves stale drafts, offers exact retries for uncertain writes and displays role-appropriate controls and private-field projections.

## Evidence layers

The .NET foundation suite tests SQLite mappings and domain invariants, and the .NET HTTP scenario suite (`ApiScenarioTests`) runs the real middleware, JWT validation, EF and SQLite in-process through `WebApplicationFactory<Program>` - see the fix-pass section at the end of this page for exactly what it covers. The Python HTTP suite runs real Kestrel over a real socket. It includes a deliberately failed receipt insertion to prove that earlier domain saves roll back. The browser suite runs real Chromium against the packaged frontend/API and includes a response dropped after the server commits.

[Desktop capture](images/desktop.png) and [narrow capture](images/narrow.png) show actual application state from the acceptance workflow. Images are inspected separately from code checks. A successful build, a passing API test and a screenshot provide different kinds of evidence; none substitutes for the others.

The package script uses locked restores and places matching browser assets under the API's wwwroot. Delivery excludes node_modules, build intermediates, generated packages, private databases and test temporary data. Exact original text files are preserved under docs/original. Historical learning pages are marked so their old defaults and proposed fixes are not confused with current behavior.

The dependency pass updated transitive build tools to versions outside their reported affected ranges: baseline-browser-mapping 2.11.22, browserslist 4.28.9, nanoid 3.3.19 and postcss 8.5.28. The final installed tree and build were checked again. The relevant fix information is in the [baseline mapping advisory](https://github.com/advisories/GHSA-w5vr-8v7q-w6rv), [Browserslist advisory](https://github.com/advisories/GHSA-c83g-rgw3-j3cx), [Nano ID advisory](https://github.com/advisories/GHSA-2v37-7h3g-55p8) and [PostCSS advisory](https://github.com/advisories/GHSA-fxqj-rqcc-2cmp). A clean advisory scan describes the reported dependency database at verification time, not a comprehensive security assessment.

## Scope limits to understand

This is a loopback, single-organization learning application. It is not payroll, a production HR deployment or a legal interpretation of leave policy. Its date rule permits up to eight hours per calendar date and an inclusive span of at most 31 dates; it does not exclude weekends or holidays automatically.

The workshop caps employees at 2,000, departments and job roles at 200 each, and PTO requests at 10,000. List endpoints are paginated. Reference selectors load bounded lists for teaching simplicity. Org traversal validates cycles and limits manager depth. Larger organizations need a separately designed selector and reporting strategy.

Receipts are retained for 24 hours and cleaned during writes. Browser tokens and uncertain operations are held in memory. Reloading the page requires sign-in and loses the browser's pending operation snapshot. The database receipt still exists, but the UI does not provide cross-reload recovery. Sessions expire after eight hours.

Setup, login and logout do not use the CRUD receipt protocol. A password reset that revokes your own session requires sign-in again; current authorization still applies before any receipt can be replayed. The browser confirms a known successful reset before returning to sign-in.

Approved PTO cannot be cancelled or refunded. Historical employee records with relationships cannot be hard-deleted. Unknown database files are refused rather than migrated or reset. A schema change requires a new learning path or an explicit migration design. There is no account recovery, email delivery, federated identity, background scheduler, multi-organization tenancy or production abuse-control system.

## Reproduce and review

Use [the testing guide](04-TESTING-AND-DEBUGGING.md) to run checks against fresh temporary databases. If a later edit changes behavior, keep the old evidence as historical and record a new run. Do not silently label an untested exercise as implemented.

When reviewing the final project report, separate source changes, observed test results and study suggestions. The nine course files explain implemented behavior and clearly label future exercises. That separation lets you use this project as a trustworthy starting point for your own next feature.


## Fix-pass re-run (2026-09-17)

The review's largest honest criticism was that `dotnet test` proved only 13 of the 61 claimed checks, because the HTTP contract lived exclusively in `scripts/check_api.py`. A new class, `tests/Nethris.Tests/ApiScenarioTests.cs`, ports the highest-value scenarios into xUnit using `WebApplicationFactory<Program>`, which runs the real middleware, JWT handler and EF/SQLite transaction in-process. Each test class redirects `Hris:Database` to a temporary file, so the suite can never open a workshop database.

Ported (15 cases): repeat setup refused 409; unauthenticated read 401; wrong password 401; a mutation with no `Idempotency-Key` refused 422; a retry with the same key and body replaying the receipt with `Idempotency-Replayed: true` and exactly one record created; the same key with a different body refused 409; an update with no `If-Match` refused **428**; an update with a stale `If-Match` refused **409** after an accepted edit; a read carrying the record's strong ETag; three role refusals for an Employee account (department write, audit log, account administration); a signed-out token refused although its signature is still valid; an out-of-range `limit` refused 422; and a rejected write leaving no audit row (the transaction actually rolling back).

```
dotnet test tests/Nethris.Tests/Nethris.Tests.csproj -c Release
Passed!  - Failed:     0, Passed:    28, Skipped:     0, Total:    28, Duration: 5 m 59 s - Nethris.Tests.dll (net10.0)
```

28 = 13 delivered foundation cases + 15 new HTTP cases. The run takes about six minutes, almost all of it password hashing in the setup and sign-in paths; that is the cost of testing the real authentication stack rather than a stub, and it is worth it.

One failure on the first run is worth recording as a lesson rather than hidden: `ASignedOutTokenIsRejectedImmediately` returned **415 Unsupported Media Type** because the logout request was sent with no body and therefore no `Content-Type`. The server reads a body for every non-GET request. The fix was in the test (send `{}`), not in the server.

**What is still only in Python.** `scripts/check_api.py` remains the larger black-box suite and still exclusively covers the full PTO approval chain with the distinct-HR reviewer rule, manager-cycle and lead-department checks, concurrent writers through a thread pool, and the 24-hour receipt window. Do not read "28 tests pass" as "all 61 checks pass": the two suites overlap deliberately at the contract level and diverge at the workflow level, and both numbers are reported here rather than added together.

**Stray database files.** The delivery limits admit that a temporary SQLite file was left outside the project by a tooling run. It is not part of the repository and nothing reads it; the README's "Housekeeping" block gives a one-line PowerShell command to find and remove any leftovers under `%TEMP%`. Every suite in this project now writes to a unique temporary directory and deletes it afterwards.

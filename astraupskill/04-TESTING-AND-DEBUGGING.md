# Test behavior at the boundary where it can fail

## What you need installed, and what each unlocks

| Tool | Version used | Unlocks | Without it |
|---|---|---|---|
| .NET SDK 10.x (pinned in `global.json`) | targets `net10.0` | `dotnet build`, and `dotnet test tests/Nethris.Tests/...` - the foundation tests **plus the HTTP scenario tests** | nothing runs |
| Node 22.16 or later 22.x + npm | `client/package.json` `engines`: `>=22.16.0 <23` | `npm install`, `npm run build` and `npm run dev` in `client/` - a TypeScript and bundle check, not a test | you cannot build the browser client |
| Python 3.x + `pip install -r scripts/requirements-browser.txt` | | `python scripts/check_api.py` (the black-box HTTP suite against real Kestrel) and `python scripts/check_browser.py` (the browser workflows) | you lose the browser tier; the HTTP contract is still covered by `dotnet test` |
| A Chromium browser | | `scripts/check_browser.py` only | the browser tier only |

You can do chapters 01-05 and every exercise with the first two rows. Python and Chromium are needed only to reproduce the browser evidence recorded in `VERIFICATION.md`.

A passing TypeScript compile proves that the browser source satisfies its type constraints. It does not prove that the API returns named roles, that SQLite can translate a query, or that a save survives a lost response. Nethris therefore uses several complementary checks.

## Run the layers

From the project root in PowerShell:

```powershell
.\scripts\package.ps1
dotnet test tests/Nethris.Tests/Nethris.Tests.csproj --no-restore
python scripts/check_api.py
python -m pip install -r scripts/requirements-browser.txt
python -m playwright install chromium
python scripts/check_browser.py
```

The package script restores locked dependencies, builds the client, publishes the API and copies matching browser assets into the package. The Python checks start an owned Kestrel process on a free loopback port with a fresh temporary database. They stop that process and remove that private database when finished. They do not point at the database you use for manual practice.

If Chromium is already installed for the test runner, ASTRA_BROWSER_EXECUTABLE can point to its executable. NETHRIS_ASSEMBLY can point to another built API assembly. For browser checks, that assembly's directory must also contain the matching wwwroot assets.

## Understand what each layer proves

[FoundationTests.cs](../tests/Nethris.Tests/FoundationTests.cs) exercises EF/SQLite mappings and pure rules. It checks timestamp ordering, assigned approval inserts, competing tracked updates, exact scaled values, private-key persistence, unknown-file refusal, rollback and manager-chain/date limits.

[check_api.py](../scripts/check_api.py) uses real HTTP and JWT sessions. It covers bootstrap, role privacy, CRUD, protected deletion, ETags, durable receipts, approval stages, balance reservation, concurrent requests and session revocation. The checks are scenario groups with several assertions each; do not report each assertion as a separate test.

[check_browser.py](../scripts/check_browser.py) uses real Chromium against the packaged app. It tests labeled forms, a rejected login, CRUD editors, stale draft review, response-loss recovery, role-specific views, PTO decisions, layout and sign-out. It records desktop/narrow screenshots. This is focused acceptance coverage, not an exhaustive accessibility or visual-regression audit.

## Debug the original sorting failure

The original API stored DateTimeOffset in a SQLite representation that could not support the requested ORDER BY. The employee directory page still loaded, while PTO and audit reads failed. Begin with the failing endpoint and server exception, not a rewrite of the entire application.

Then inspect the expression sent to EF. Confirm that the ordering occurs before materialization. Decide on a persisted representation that supports the operation. The current model converts timestamps to integer Unix milliseconds. Add a test with records inserted out of chronological order and assert the ordered result.

A related setup failure appeared during the improvement work: an expiry predicate included DateTimeOffset.UtcNow directly inside the expression. Capturing now into a local variable lets EF bind the converted parameter. This illustrates why successful model-level tests must be followed by real endpoint tests.

## Debug a stale save

Open the same department in two sessions. Save a changed name in the first session. In the second, save its original draft. Expect a conflict, not a silent overwrite. Inspect the request's If-Match, the current row version, and the editor's comparison panel. The losing draft should remain visible.

Do not “fix” the test by refreshing the version immediately before every PUT. That removes the evidence that the user edited older data. The correct fix is explicit review of current values followed by a new operation.

## Debug an uncertain result

In the browser acceptance test, route.fetch lets the server finish a create; route.abort then hides the response. The UI retains the exact operation and offers a retry. Compare both submitted bodies and keys, then query the API and confirm there is only one department.

Distinguish “request rejected” from “response unreadable.” A clear 422 validation response means the operation was rejected. A network interruption or unreadable response leaves the outcome uncertain. The UI must not silently start a second create.

## Keep evidence honest

Read the [verification record](VERIFICATION.md) and its evidence file for the delivered run. Earlier failed runs remain useful debugging evidence but are not counted as passing checks. Repeated successful runs do not increase the number of distinct scenarios. A screenshot proves the visible state at capture time; it does not prove every permission boundary.

When a regression fails, record the command, expected behavior, actual behavior and smallest relevant log excerpt. Remove the defect, rerun affected checks, then run the integrated acceptance path when the changed boundary can affect it.


## The xUnit suite is now two classes

```powershell
dotnet test tests/Nethris.Tests/Nethris.Tests.csproj -c Release
```

Counted from the attributes on 2026-10-06: 13 + 15 = 28 cases, which matches the recorded run in `VERIFICATION.md`. Expect the run to take several minutes, mostly PBKDF2 password hashing during setup and sign-in.

| Class | What it covers | Why at this level |
|---|---|---|
| `FoundationTests` (11 methods = 10 `[Fact]` + 1 `[Theory]` with 3 `[InlineData]`, so 13 cases) | SQLite timestamp ordering, attached-approval insertion, exact quarter-hour and cents storage, and the other data-layer facts the change depends on | These are statements about EF and SQLite, not about HTTP. Testing them through a web request would make the failure harder to read, not easier. |
| `ApiScenarioTests` (15 `[Fact]`) | Setup and repeat-setup, unauthenticated and wrong-password sign-in, the idempotency contract (missing key, replay, key reuse), `If-Match` (428 and stale-conflict) with the strong ETag on reads, role-based refusals for an Employee, revocable sessions, pagination bounds, and a rejected write leaving no audit row | Every one of these is a statement about the *wire*: a status code, a header, or what a second request can observe. Only an HTTP test can make them. |

`ApiScenarioTests` uses `WebApplicationFactory<Program>`, which boots the real `Program.cs` in-process - the real middleware chain, the real JWT handler, the real EF/SQLite transaction per request - with no port, no `dotnet run` child process and no readiness polling. The database is redirected per test class:

```csharp
factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    builder.UseSetting("Hris:Database", Path.Combine(directory, "api-test.sqlite")));
```

That one line is the reason these tests can never open a real workshop database, which is exactly the property the Python suite documents in its own first line. Two setup requirements are easy to forget elsewhere: the API's `Program.cs` must end with `public partial class Program { }` (it does), and the test project needs `Microsoft.AspNetCore.Mvc.Testing` (it has it).

Read one and notice the shape - arrange through the API's own endpoints, act, assert on the status code and a header:

```csharp
[Fact]
public async Task AnUpdateWithoutIfMatch_Is428()
{
    using var created = await Post("departments", Department("Precondition required"), adminToken, Guid.NewGuid());
    var row = await Read(created);

    using var response = await Put("departments/" + row.GetProperty("id").GetString(),
        Department("Renamed"), adminToken, Guid.NewGuid(), ifMatch: null);

    Assert.Equal((HttpStatusCode)428, response.StatusCode);
}
```

**What the Python suite still owns.** `scripts/check_api.py` remains the larger black-box suite and still covers scenarios that have not been ported: the full PTO approval chain with its distinct-HR rule, manager-cycle and lead-department checks, concurrent writers through a thread pool, and the 24-hour receipt window. Read the count honestly: `dotnet test` proves the foundation facts plus the HTTP contract above, and `python scripts/check_api.py` proves the rest. Neither number is the whole story on its own, and `VERIFICATION.md` keeps them separate.

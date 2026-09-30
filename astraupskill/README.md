# Nethris: a relational CRUD workshop

**Continue with the [expanded learning path](advanced/README.md): guided checkpoints, worked traces, debugging labs, independent exercises, solutions and a capstone.**

Nethris is a single-organization HR information system. You will work with employees, departments, job roles, user accounts and time-off requests. This course targets a junior engineer who can write functions and wants to become comfortable owning a complete web feature: understanding existing code, designing a change, implementing it, proving its behavior and explaining its limits.

Start with the [project README](../README.md) for commands. The active browser entry is [main.tsx](../client/src/main.tsx), which renders [Workshop.tsx](../client/src/Workshop.tsx). The original prototype and its historical course are preserved under [docs/original](../docs/original/README.md.txt). Read the current files when answering questions about current behavior. An old page that describes automatic demo login is historical evidence, not a setup instruction.

## Prerequisites and glossary

**Prerequisites.** You should be able to read C# and `async`/`await`, read a React component, and read a `SELECT` with a join. You do **not** need prior ASP.NET Core, EF Core, JWT or xUnit experience; this course introduces each where it is used. You do need to be comfortable running commands in a terminal. The `.netpastebin` course in this workspace is a lighter predecessor: it teaches ETags, request ids and receipts against one JSON document before you meet them over EF Core here.

**What you need installed.**

| Tool | Unlocks | Needed for |
|---|---|---|
| .NET SDK 10.x (pinned in `global.json`) | `dotnet build`, `dotnet test tests/Nethris.Tests/...` | everything |
| Node LTS (20 or 22) + npm | `npm install` / `npm run build` / `npm run dev` in `client/` | the browser client |
| Python 3.x + `pip install -r scripts/requirements-browser.txt` | `python scripts/check_api.py`, `python scripts/check_browser.py` | reproducing the black-box HTTP and browser evidence |
| A Chromium browser | `scripts/check_browser.py` only | the browser tier |

Chapter 04 repeats this table with exactly which checks each row unlocks. The first two rows are enough for every chapter and every exercise.

**Housekeeping: the stray database file.** The delivery's limits admit that a temporary SQLite file was left *outside* the project directory by a tooling run. It is not part of the repository and nothing reads it. To be sure your machine has none left over:

```powershell
# Look for stray workshop databases outside the project, then delete the ones you recognise.
Get-ChildItem -Path $env:TEMP -Filter 'nethris-*.sqlite*' -Recurse -ErrorAction SilentlyContinue |
    Select-Object FullName, Length, LastWriteTime
# Remove-Item <path>   # run only for files you have inspected
```

The workshop's own database lives at `server/Nethris.Api/App_Data/nethris-learning.sqlite` (or wherever `Hris:Database` points). Deleting *that* resets the workshop, which is a legitimate way to start over - the test suites never touch it, because each one redirects `Hris:Database` to a temporary path.

**Time estimate (about 15-20 hours; this is the longest course in the set).**

| Stage | Estimate |
|---|---|
| Orientation (run it, click around, switch roles) | 60-90 min |
| 01 Codebase map | 2-3 h |
| 02 Concepts | 3-4 h |
| 03 Worked change | 3-5 h |
| 04 Testing and debugging | 2-4 h |
| 05 Practice (8 exercises; 6-8 are multi-session) | several sessions |
| 06 Solutions and review | 60-90 min |
| 07 Trace lab | 60 min |
| VERIFICATION | 30 min |

**Glossary.**

| Term | What it means here | Where in this project |
|---|---|---|
| **Employee vs user account** | An employee is an HR record; a user account is a sign-in. One can exist without the other, and the course's sharpest bugs live where they are confused. | `server/Nethris.Api/Domain/Employee.cs` and `Domain/UserAccount.cs` |
| **Role (Employee / Manager / HR / Admin)** | The permission level on the *account*. A job title such as "Engineering manager" grants nothing. | `SecurityRole` in `server/Nethris.Api/Domain/Enums.cs` |
| **Optimistic concurrency** | Two people edit the same record; instead of locking, each write states the version it read, and the loser is refused. | the `IVersioned` version on `server/Nethris.Api/Domain/PtoRequest.cs` |
| **Strong ETag / `If-Match`** | The HTTP headers carrying that version: the server sends `ETag: "<id>-<version>"`, the client echoes it in `If-Match`. "Strong" means byte-exact equality, not a weak/semantic match. | the `If-Match` reader in `server/Nethris.Api/HrisRequest.cs` |
| **428 vs 409** | **428 Precondition Required** = you sent no `If-Match` at all. **409 Conflict** = you sent one and it is out of date. Different client fixes: add the header, versus reload and re-review. | the 428 throw in `HrisRequest.cs`; the version comparison raises the 409 |
| **Idempotency key** | A client-generated UUID (`Idempotency-Key`) sent with every mutation, so a retry after a lost response is recognised and replayed instead of creating a second record. | the `Idempotency-Key` header read in `server/Nethris.Api/HrisApi.Mutations.cs` |
| **Receipt / replay** | The stored response of an accepted mutation, and returning it again for a repeated key (with `Idempotency-Replayed: true`). Receipts here expire after 24 hours. | `OperationReceipt` in `Domain/WorkshopModels.cs`; `Idempotency-Replayed` in `HrisApi.Mutations.cs` |
| **Fingerprint** | A hash of method + path + query + `If-Match` + body. A key reused with a *different* fingerprint is a client bug, answered 409 rather than replayed. | `Fingerprint(http, method, body)` in `HrisApi.Mutations.cs` |
| **Transaction** | The unit in which the record change, the audit row and the receipt are written together, so none of them can exist without the others. Every request in this API runs inside one. | `connection.BeginTransaction(deferred: !route.IsWrite)` in `server/Nethris.Api/HrisApi.cs` |
| **Revocable session** | A row the server checks alongside the JWT signature, so signing out invalidates a token that is still cryptographically valid. | the `Sessions` rows checked in `server/Nethris.Api/Security/HrisAuth.cs` |
| **Distinct-reviewer rule** | Two-stage PTO approval where the HR approver must not be the same person as the manager approver. | the approval stage checks in `server/Nethris.Api/Domain/HrisPto.cs` |
| **Captured manager** | The manager stored on the PTO request when it is filed, so a later reorganisation does not change who was asked to approve. | `AssignedManagerId` in `Domain/HrisPto.cs` |
| **Quarter-hour / cents storage** | PTO hours are stored in exact quarter-hour units and money in cents, because binary floating point cannot represent 0.1 exactly. | `HrisRules.QuarterUnits` and `HrisRules.Cents` in `Domain/HrisRules.cs` |
| **`WebApplicationFactory<Program>`** | The standard .NET tool that boots the real API in-process for tests and hands you an `HttpClient`: no port, no child process. | `tests/Nethris.Tests/ApiScenarioTests.cs` |
| **Black-box suite** | `scripts/check_api.py` - it talks to a really-running server over HTTP and knows nothing about the internals. | `scripts/check_api.py` |

## What you will actually build understanding of

An employee is a business record. A user account grants access to that employee. A job title such as Engineering manager does not itself grant Manager permission. A department can have a lead, but the lead must belong to that department. A PTO request has dates, quarter-hour quantities, a requester, a captured manager and a state. These distinctions are the foundation of the exercises: most important bugs occur where two related concepts are treated as interchangeable.

For example, creating a Software engineer job role should not create a sign-in account. Editing an employee email should synchronize the linked account email without allowing HR to promote that account to Admin. Approving PTO should insert a decision and update the balance in one transaction. It should not fabricate an approval in React and hope that the server agrees.

## Suggested order

1. **Orientation, 60–90 minutes:** run the package, initialize fictional colleagues, inspect the directory and switch between Employee, Manager and HR accounts. Draw the relationship between employee and account.
2. **Code reading, 2–3 hours:** use [the codebase map](01-CODEBASE-MAP.md) and trace one GET and one POST. Predict which database tables each touches.
3. **Core reasoning, 3–4 hours:** work through [the concepts](02-CONCEPTS.md). Explain transactions, versions and operation receipts using the same failed-save example.
4. **A complete change, 3–5 hours:** follow [the worked change](03-WORKED-CHANGE.md), then reproduce its happy path and failure cases.
5. **Verification, 2–4 hours:** run [the testing and debugging guide](04-TESTING-AND-DEBUGGING.md). Deliberately make one regression in a private practice copy and identify the test that catches it.
6. **Independent work, several sessions:** attempt [the practice tasks](05-PRACTICE.md) before reading [the solutions and review guide](06-SOLUTIONS-AND-REVIEW.md).
7. **Explain your understanding:** complete [the trace lab](07-TRACE-LAB.md) and compare your claims with [verification evidence](VERIFICATION.md).

These are study estimates, not deadlines. If you cannot explain why a mutation is atomic, spend more time on the trace before adding another screen.

## First session

Run the package script, start the API, and open http://127.0.0.1:5187. Create your first Admin account with a password of at least twelve characters. The optional fictional seed adds hr@example.test, manager@example.test, employee@example.test, colleague@example.test and reviewer@example.test. These fixture accounts use the workshop password you just chose. There are no preconfigured production credentials and no external email is sent.

As Employee, request 7.75 hours on one future date. As Manager, approve the first stage. As HR, approve the second. Return to the employee directory and inspect the balance. A starting balance of 120 becomes 112.25 only after the HR decision. Then explain why retrying the same completed decision cannot deduct another 7.75 hours.

Keep a notebook with three columns: observation, source location, evidence. “The button disappeared” is an observation. “The API checks the current role inside a transaction” is a source claim. A rejected direct HTTP request is evidence for the permission boundary. Keeping these separate is a valuable mid-level engineering habit.

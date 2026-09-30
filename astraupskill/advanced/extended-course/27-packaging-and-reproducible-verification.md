# 27. Packaging and reproducible verification

The [root README](../../../README.md) distinguishes packaged application use, two-terminal development, HTTP checks and browser checks. The project uses .NET and a separate client toolchain, so a reproducible report must identify which build and runtime path it exercised. Read the setup instructions and scripts before running commands copied from another repository.

## Know the artifact being tested

The package script builds browser assets for the server's packaged application. A development Vite session can serve current source while a packaged server still serves older generated assets. A screenshot alone may not reveal which path produced it. Record the command, working directory and owned URL in your verification note so the evidence belongs to a known artifact.

The README explains that the package script replaces generated browser assets but preserves the learning database. Treat those as different kinds of state. Rebuilding assets should not be described as resetting the workshop. If you need a new fictional dataset for a test, configure a new absolute Hris__Database path rather than deleting the ordinary database as a setup shortcut.

## Use the repository's declared versions

Inspect global.json, the project files and client package metadata for the expected toolchain. Record the installed versions you actually use. A successful run under one SDK is evidence for that environment, not proof that every .NET version works. Likewise, a missing dependency can block a check without establishing a source defect.

For documentation-only work, do not install or rebuild unrelated components merely to produce a larger verification log. For a real feature change, run the checks that exercise its affected boundaries. A department description needs contract, persistence and UI evidence; a prose correction to a helper explanation can be validated against source and focused examples.

## Distinguish the test routes

The xUnit project includes in-process application scenarios with temporary SQLite files. The Python HTTP and browser checks start owned servers with fresh temporary databases according to the documented setup. These routes can complement each other, but their results are not interchangeable. A browser check demonstrates interaction with built assets; an in-process HTTP scenario isolates route and persistence behavior without a browser.

Before a server-based check, ensure the port and output location belong to the test. Do not reuse an unknown already-running server and call the result a clean rehearsal. Preserve prior evidence in separate output directories. If a runner refuses an occupied port or existing output, treat that as an isolation signal rather than something to bypass blindly.

## Exercise NH-27A: write a setup card

Create concise instructions containing prerequisites, root directory, package command, start command, disposable database configuration, test project path and relevant optional browser setup. Mark commands actually executed separately from reproduction instructions. Include an expected successful observation such as a test summary or health response, without inventing an unverified count.

The card should also explain how to stop the owned server. A learning experiment that leaves multiple indistinguishable servers running becomes harder to reproduce. Identify process ownership explicitly when your automation starts a helper, and close only the process it owns.

## Exercise NH-27B: assemble an evidence manifest

Record source state, changed files, executed commands, exit codes, artifact paths and any skipped or unavailable scope. Use hashes where identifying exact generated files matters. A hash proves which bytes were inspected, not that the feature is correct. Pair it with the relevant behavioral observation.

If the workspace contains unrelated user edits, preserve them and state the source-state limitation. Do not claim a clean repository merely because your own change is confined to one folder. A local feature review can still be useful with explicit scope and a focused diff.

## Exercise NH-27C: rehearse an interrupted verification

Imagine the client build succeeds but the API test setup fails because a required package is unavailable. Write an accurate report that preserves the build result and identifies the unexecuted API scope. Then imagine API tests pass but the browser server cannot bind its port. Distinguish a setup problem from a failed browser assertion.

For a release decision, connect the missing evidence to the change. A new stale-draft interface needs browser or component evidence that the blocked run did not provide. A backend-only quantity helper may already have meaningful focused coverage. Avoid both overstating readiness and demanding irrelevant checks by habit.

## Review standard

A strong verification package identifies the artifact, environment and boundary under test. It preserves databases and prior evidence, owns its processes, and reports passed, failed, blocked and not-run accurately. The final setup card should let another learner repeat the work without depending on hidden local state.

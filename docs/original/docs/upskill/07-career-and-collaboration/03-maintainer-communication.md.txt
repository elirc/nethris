# Maintainer Communication

## Ask Without Outsourcing Thinking

Weak: "How do I add tests?"

Strong: "I found no test project. I propose `Nethris.Api.Tests` with `WebApplicationFactory` for PTO authorization around [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353). Is xUnit acceptable here?"

## Bug Report Template

```md
## Repro
## Expected
## Actual
## Environment
## Suspected area
## Evidence
```

## Responding To Review

- Confirm the invariant.
- State what changed.
- Mention command run.
- Avoid drive-by refactors outside scope.

Example: "Agreed, UI-only approval hiding is insufficient. I kept the React filter in [App.tsx](../../../client/src/App.tsx#L65-L69) but added/kept server resource authorization at [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353)."


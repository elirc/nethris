# Writing PRs And RFCs

## PR Template

```md
## What

## Why

## How tested

## Risk / rollback

## Follow-ups
```

## Repo-Tailored Examples

- "Added PTO cancellation": cite touched lines near [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385), [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L3-L15), [App.tsx](../../../client/src/App.tsx#L480-L500).
- "Added migrations": explain transition from [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11).

## When To RFC

Write an RFC for changes that affect contracts, data model, auth, or reliability. Example: generated OpenAPI client affects [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109) and [types.ts](../../../client/src/types.ts#L1-L100).


# Architecture And Patterns

Read this section when you can already run or at least trace the app. Architecture here is not ceremony; it is the set of boundaries that decide how expensive future changes become.

Core files:

- Composition root and endpoints: [Program.cs](../../../server/Nethris.Api/Program.cs#L12-L397)
- Entities and relationships: [Domain](../../../server/Nethris.Api/Domain/Employee.cs#L3-L26), [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L16-L72)
- Contracts: [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109)
- UI workflows: [App.tsx](../../../client/src/App.tsx#L29-L543)
- Client boundary: [api.ts](../../../client/src/api.ts#L4-L105)


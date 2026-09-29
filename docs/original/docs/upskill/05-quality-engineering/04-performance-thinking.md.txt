# Performance Thinking

Measure first. This repo is small, so most items are possible risks rather than proven bottlenecks.

| Domain | Likely hotspot | Evidence | How to measure | Possible fix |
| --- | --- | --- | --- | --- |
| DB | Unbounded employee list | [Program.cs](../../../server/Nethris.Api/Program.cs#L141-L146) | EF query logging, seeded large data | pagination |
| DB | Case-insensitive search | [Program.cs](../../../server/Nethris.Api/Program.cs#L126-L133) | query plan | normalized column or provider-specific search |
| DB/UI | Org chart loads all employees | [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L267) | response size/timing | lazy expansion or cached tree |
| Server allocations | DTO/list materialization | [Program.cs](../../../server/Nethris.Api/Program.cs#L451-L471) | dotnet-counters/profiler | pagination/projection |
| React render | Single large `App.tsx` state | [App.tsx](../../../client/src/App.tsx#L29-L37) | React profiler | component split/memo after evidence |
| Network | Snapshot loads many endpoints | [api.ts](../../../client/src/api.ts#L42-L49) | browser network panel | aggregate endpoint or cache |
| Bundle | Icon library and app bundle | [package.json](../../../client/package.json#L11-L17) | Vite bundle output | code-split later |

Drill: choose one "possible risk" and write the metric that would make it a real priority.


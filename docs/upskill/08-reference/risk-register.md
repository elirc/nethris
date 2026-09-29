> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Risk Register

| Risk | Evidence | Impact | Likelihood | Suggested test | Suggested fix | Confidence |
| --- | --- | --- | --- | --- | --- | --- |
| No automated tests | No test projects found; auth logic in [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353) | regressions in permissions | High | API integration tests | add test project | High |
| Backend build unverified here | `dotnet` unavailable during verification | hidden compile/runtime errors possible | Medium | `dotnet restore && dotnet build` | install SDK | High |
| Dev JWT key in config | [appsettings.json](../../../server/Nethris.Api/appsettings.json#L5-L9) | secret misuse if deployed | Medium | config review | env/user-secrets/key vault | High |
| Demo fallback masks API outage | [api.ts](../../../client/src/api.ts#L24-L39) | false confidence in local testing | Medium | API-down UI test | gate demo mode | High |
| No migrations | [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11) | schema evolution risk | High if app grows | migration smoke test | introduce migrations | High |
| Unbounded employee/org reads | [Program.cs](../../../server/Nethris.Api/Program.cs#L141-L146), [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L261) | latency/memory at scale | Medium | load test with large seed | pagination/lazy org loading | Medium |
| PTO balance can go negative | deduction at [Program.cs](../../../server/Nethris.Api/Program.cs#L367-L373), no balance check in [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303) | invalid HR data | Medium | approve request over balance | validate balance policy | Medium |
| Org chart cycle risk | recursion [Program.cs](../../../server/Nethris.Api/Program.cs#L434-L449), self-manager only prevented on direct update [Program.cs](../../../server/Nethris.Api/Program.cs#L226-L226) | stack overflow/hang | Low now | cycle data test | validate manager chain | Medium |
| Sparse audit detail | audit helper [Program.cs](../../../server/Nethris.Api/Program.cs#L481-L490) | weak investigation trail | Medium | audit assertions | old/new diffs + correlation id | Medium |
| UI mojibake | [App.tsx](../../../client/src/App.tsx#L344-L344), [App.tsx](../../../client/src/App.tsx#L446-L446) | polish/accessibility issue | High visible | build and visual check | replace characters | High |
| Manual contract drift | [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109), [types.ts](../../../client/src/types.ts#L1-L100) | runtime mismatch | Medium | contract tests | generated client | High |


# Refactor And Design Katas

1. Boundary leak: split endpoint groups from [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L395). Self-grade on behavior preservation and testability.
2. Outbox/eventing: design PTO notification after [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381). Self-grade on idempotency and retry.
3. Remove duplication: align demo PTO behavior [api.ts](../../../client/src/api.ts#L54-L88) with server rules [Program.cs](../../../server/Nethris.Api/Program.cs#L295-L385).
4. Improve type safety: generated client from [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109).
5. Design migration: convert [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11) to migrations.
6. Reduce N+1/over-fetch risk: review includes [Program.cs](../../../server/Nethris.Api/Program.cs#L119-L124), [Program.cs](../../../server/Nethris.Api/Program.cs#L406-L410).
7. Write an RFC: "PTO cancellation with balance refund."
8. Review flawed PR: "Manager can approve all PTO" using [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353).


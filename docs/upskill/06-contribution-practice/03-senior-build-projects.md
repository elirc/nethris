> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Senior Build Projects

## Project 1: Production-Ready Auth
**Problem:** login has JWT but no lockout/rate limiting/refresh/revocation. **Anchors:** [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83), [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L21-L38). **Plan:** threat model, rate limiting, secret management, token lifetime, tests, rollback.

## Project 2: PTO Workflow Domain Service
**Problem:** state machine lives inline [Program.cs](../../../server/Nethris.Api/Program.cs#L337-L378). **Value:** testable approval invariants. **Plan:** extract service, add transition tests, keep endpoint thin, migrate gradually.

## Project 3: Migration And Schema Governance
**Problem:** `EnsureCreatedAsync` [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11) blocks mature migrations. **Plan:** add migrations, seed strategy, rollback guide, CI migration check.

## Project 4: Observability Baseline
**Problem:** health exists [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L68), but no explicit logs/metrics/tracing. **Plan:** structured logs, request correlation, metrics for PTO transitions, dashboard, alert rules.

## Project 5: Reliable PTO Notifications
**Problem:** no background infrastructure; approval writes end at [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381). **Plan:** outbox table, `BackgroundService`, retry, idempotency, dead-letter visibility.

## Project 6: Contract Governance
**Problem:** manual TS/C# contracts [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109), [types.ts](../../../client/src/types.ts#L1-L100). **Plan:** OpenAPI generation or contract tests, compatibility policy, versioning.

## Project 7: Large-Org Performance
**Problem:** unbounded reads [Program.cs](../../../server/Nethris.Api/Program.cs#L141-L146), [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L261). **Plan:** measure, paginate, index, cache org chart, load testing, rollout flag.


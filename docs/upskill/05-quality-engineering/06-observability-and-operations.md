> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Observability And Operations

## Current Signals

- Health endpoint exists at [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L68).
- Swagger is enabled in development [Program.cs](../../../server/Nethris.Api/Program.cs#L58-L62).
- Audit logs exist as data records [AuditLog.cs](../../../server/Nethris.Api/Domain/AuditLog.cs#L3-L12) and are exposed to PeopleOps [Program.cs](../../../server/Nethris.Api/Program.cs#L387-L395).
- No explicit `ILogger`, OpenTelemetry, metrics, exception middleware, Docker, deployment, or rollback files were found.

## How Would I Know This Broke?

| Flow | Signal today | Better signal |
| --- | --- | --- |
| Login | HTTP status from [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83) | failed login rate metric, lockout logs |
| PTO request | audit row [Program.cs](../../../server/Nethris.Api/Program.cs#L318-L319) | structured log with request id |
| PTO approval | audit row [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381) | metric by status transition |
| Org chart | request latency only | query timing and response size |
| Seed/startup | app fails to start | startup migration/seed logs |

Drill: add a proposed structured log event name for each major flow.


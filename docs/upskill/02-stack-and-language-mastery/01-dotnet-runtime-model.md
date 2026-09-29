> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# .NET Runtime Model

## CLR, JIT, And Startup

The API is a .NET 8 Web SDK app in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L1-L5). At runtime, the CLR loads the app, JIT-compiles hot methods, builds services in [Program.cs](../../../server/Nethris.Api/Program.cs#L12-L50), then runs startup seeding in [Program.cs](../../../server/Nethris.Api/Program.cs#L52-L56). Why it matters: startup work is on the critical path. If [SeedData.EnsureSeededAsync](../../../server/Nethris.Api/Data/SeedData.cs#L9-L85) blocks or fails, the app never becomes ready.

Failure modes:

- Heavy startup work increases cold-start latency.
- Database creation in [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11) can fail before health checks are available.
- No migration flow exists yet, so schema evolution needs design.

Drill: mark which startup lines are pure configuration and which perform side effects.

## GC And Allocations

Most endpoint handlers create DTO records and lists, for example employee responses in [Program.cs](../../../server/Nethris.Api/Program.cs#L141-L146) and PTO responses in [Program.cs](../../../server/Nethris.Api/Program.cs#L288-L292). These allocations are fine at this size. At scale, avoid materializing huge tables without pagination.

Failure modes:

- The org chart loads every employee in [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L261).
- The recursive helper creates nested lists in [Program.cs](../../../server/Nethris.Api/Program.cs#L436-L440).

Drill: estimate allocations for 10 employees vs 10,000 employees.

## Async/Await And Task

Endpoints use `async` EF calls such as login lookup in [Program.cs](../../../server/Nethris.Api/Program.cs#L72-L74), PTO creation save in [Program.cs](../../../server/Nethris.Api/Program.cs#L317-L319), and approval save in [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381). `await` frees the request thread while I/O is pending; it does not make CPU work faster.

Transferable failures:

- Sync-over-async deadlock: blocking on `.Result` or `.Wait()` inside request code. None found.
- Missing cancellation: no endpoint accepts `CancellationToken`; investigate adding it to long-running reads such as [Program.cs](../../../server/Nethris.Api/Program.cs#L255-L270).
- Serial awaits when parallel is safe: frontend snapshot uses `Promise.all` in [client/src/api.ts](../../../client/src/api.ts#L42-L49), a good contrast.

## IDisposable And Scoped Lifetimes

`HrisDbContext` is registered through `AddDbContext` in [Program.cs](../../../server/Nethris.Api/Program.cs#L14-L15), which creates a scoped context per request. Startup seeding manually creates a scope in [Program.cs](../../../server/Nethris.Api/Program.cs#L52-L56). Why it matters: `DbContext` is not thread-safe and should not be singleton.

Drill: explain why `JwtTokenService` can be scoped safely in [Program.cs](../../../server/Nethris.Api/Program.cs#L25-L25), and what would go wrong if it captured a scoped `DbContext` inside a singleton.

## ValueTask, ConfigureAwait, Spans

No `ValueTask`, `ConfigureAwait`, `Span<T>`, or memory-oriented parsing code was found. Do not add them for style. Use them when profiling or API contracts justify the complexity.


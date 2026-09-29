> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Pattern Catalog

## Pattern: Composition Root
**Problem:** One place should wire framework services and middleware. **Real example:** [Program.cs](../../../server/Nethris.Api/Program.cs#L12-L68). **Why it works:** dependencies are visible. **Failure modes:** file becomes a dumping ground. **Drill:** mark service registration vs middleware vs endpoints.

## Pattern: Minimal API Endpoint
**Problem:** Map HTTP routes to small handlers. **Real example:** [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83). **Second:** [Program.cs](../../../server/Nethris.Api/Program.cs#L295-L323). **Failure modes:** business logic grows inline. **Drill:** extract responsibilities without changing behavior.

## Pattern: DTO Record
**Problem:** Keep API contracts separate from EF entities. **Real example:** [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L30-L49). **Second:** [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L80-L91). **Failure modes:** manual client/server drift. **Drill:** compare with [types.ts](../../../client/src/types.ts#L30-L81).

## Pattern: EF Relationship Configuration
**Problem:** Make cardinality and delete behavior explicit. **Real example:** [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L23-L34). **Second:** [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L67-L71). **Failure modes:** accidental cascade deletes. **Drill:** explain every `Restrict`.

## Pattern: Eager Loading
**Problem:** Load navigation data required for response mapping. **Real example:** [Program.cs](../../../server/Nethris.Api/Program.cs#L119-L124). **Second:** [Program.cs](../../../server/Nethris.Api/Program.cs#L406-L410). **Failure modes:** over-fetching, large joins. **Drill:** decide what includes PTO list really needs.

## Pattern: Server-Side Resource Authorization
**Problem:** Role checks are not enough for resource-specific actions. **Real example:** [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353). **Second:** [Program.cs](../../../server/Nethris.Api/Program.cs#L278-L286). **Failure modes:** IDOR. **Drill:** write a test for manager approving non-report PTO.

## Pattern: Claim Helper
**Problem:** Centralize claim parsing. **Real example:** [Program.cs](../../../server/Nethris.Api/Program.cs#L492-L501). **Failure modes:** `Guid.Parse` throws if claim missing. **Drill:** design safer helper behavior.

## Pattern: Password Hashing Service
**Problem:** Store password hashes, not plaintext. **Real example:** [PasswordService.cs](../../../server/Nethris.Api/Security/PasswordService.cs#L11-L30). **Failure modes:** weak iteration count over time, no rehash upgrade path. **Drill:** propose versioned hash upgrade.

## Pattern: JWT Claim Issuer
**Problem:** Convert authenticated user into portable signed identity. **Real example:** [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L21-L38). **Failure modes:** stale role until token expires. **Drill:** explain token revocation tradeoffs.

## Pattern: Demo Fallback
**Problem:** Keep frontend explorable when API is down. **Real example:** [api.ts](../../../client/src/api.ts#L24-L39). **Second:** [demoData.ts](../../../client/src/demoData.ts#L22-L55). **Failure modes:** masks production outages if not gated. **Drill:** add an environment guard design.

## Pattern: React Derived State
**Problem:** Compute visible data from state without duplicating it. **Real example:** [App.tsx](../../../client/src/App.tsx#L43-L69). **Failure modes:** stale memo dependencies, UI auth drift. **Drill:** add PTO type filter mentally.

## Pattern: Functional State Update
**Problem:** Update arrays based on current state safely. **Real example:** [App.tsx](../../../client/src/App.tsx#L104-L119). **Second:** [App.tsx](../../../client/src/App.tsx#L128-L161). **Failure modes:** mutating arrays breaks render assumptions. **Drill:** identify every new array allocation.

## Pattern: Recursive Tree Builder
**Problem:** Build org tree from flat manager ids. **Real example:** [Program.cs](../../../server/Nethris.Api/Program.cs#L434-L449). **Second:** [App.tsx](../../../client/src/App.tsx#L438-L459). **Failure modes:** cycles, deep recursion. **Drill:** design cycle detection.

## Pattern: Audit Helper
**Problem:** Standardize audit entries. **Real example:** [Program.cs](../../../server/Nethris.Api/Program.cs#L481-L490). **Second:** [Program.cs](../../../server/Nethris.Api/Program.cs#L387-L395). **Failure modes:** sparse details, no correlation id. **Drill:** propose structured old/new diff.

## Pattern: Seed Data Guard
**Problem:** Avoid duplicating demo data on startup. **Real example:** [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L15). **Second:** [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L36-L45). **Failure modes:** partial seed leaves app in inconsistent state. **Drill:** make seed idempotent per table.


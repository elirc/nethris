# Framework Mental Models

## ASP.NET Core Pipeline

The app is composed in [Program.cs](../../../server/Nethris.Api/Program.cs#L12-L68). Services are registered first, then `builder.Build()` creates the app at [Program.cs](../../../server/Nethris.Api/Program.cs#L50-L50). Middleware order matters: CORS is before authentication and authorization in [Program.cs](../../../server/Nethris.Api/Program.cs#L64-L66). Endpoints are mapped after middleware in [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L395).

Why it matters: if `UseAuthorization` runs before `UseAuthentication`, policies do not have an authenticated principal. If CORS is too open, browsers can call APIs from unintended origins.

Drill: move mentally through a `POST /api/pto` request and name each framework stage.

## Dependency Injection

The DbContext registration in [Program.cs](../../../server/Nethris.Api/Program.cs#L14-L15) is scoped by default. `JwtTokenService` is registered scoped in [Program.cs](../../../server/Nethris.Api/Program.cs#L25-L25). Minimal API handlers declare parameters like `HrisDbContext db` and `ClaimsPrincipal principal` in [Program.cs](../../../server/Nethris.Api/Program.cs#L155-L158); ASP.NET Core binds these from DI/request context.

Failure mode: captive dependency. If a singleton service captured `HrisDbContext`, it would hold a disposed or cross-request context.

## Model Binding And Validation

Minimal APIs bind JSON into records such as [CreatePtoRequest](../../../server/Nethris.Api/Contracts/Contracts.cs#L74-L78) and [DecidePtoRequest](../../../server/Nethris.Api/Contracts/Contracts.cs#L100-L100). Manual validation appears in [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303). There are no data annotations or FluentValidation rules.

Junior miss: Type shape is not validation. `decimal Hours` means the value is a number, not that it is positive.

## React Render Model

The React app stores session, snapshot, active tab, filters, selected employee, auth error, and loading in state at [App.tsx](../../../client/src/App.tsx#L29-L37). Derived values use `useMemo` for filtered employees and visible requests in [App.tsx](../../../client/src/App.tsx#L43-L63). Render branches are controlled by `activeTab` in [App.tsx](../../../client/src/App.tsx#L210-L352).

Why it matters: React renders from state. Avoid mutating `snapshot` arrays directly; this app uses `setSnapshot` with new arrays in [App.tsx](../../../client/src/App.tsx#L104-L119) and [App.tsx](../../../client/src/App.tsx#L128-L161).

Failure modes:

- Stale closures if event handlers use old state without functional updates.
- Duplicate keys or unstable keys in recursive org rendering; current code uses `node.id` and `report.id` in [App.tsx](../../../client/src/App.tsx#L259-L260) and [App.tsx](../../../client/src/App.tsx#L452-L453).
- UI authorization drift: UI role checks in [App.tsx](../../../client/src/App.tsx#L40-L41) must not replace API authorization.

## Data Fetching

The client uses direct `fetch`, not React Query. Snapshot loading happens after login in [App.tsx](../../../client/src/App.tsx#L76-L81), and the HTTP helper attaches bearer tokens in [api.ts](../../../client/src/api.ts#L90-L105).

Drill: identify which state is server-owned and which is UI-only.


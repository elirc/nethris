# Interview Prep From This Repo

## C#/.NET Runtime

**Question:** Why use async EF calls in endpoints?  
Junior: "So it is faster."  
Mid-level: "It frees the request thread during I/O, as in [Program.cs](../../../server/Nethris.Api/Program.cs#L72-L74)."  
Senior: "It improves scalability under I/O wait, but we still need cancellation, timeouts, and query bounds for endpoints like [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L261)."

**Question:** What does nullable mean on EF navigations?  
Junior: "It can be null."  
Mid-level: "It may be unloaded, e.g. `Department?` in [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L15-L20)."  
Senior: "Nullability describes object availability, not necessarily domain optionality; invariants also need FK constraints and validation."

## ASP.NET/EF

**Question:** Explain authn vs authz in this app.  
Junior: "Login gives a token."  
Mid-level: "Login verifies password [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83); policies require roles [Program.cs](../../../server/Nethris.Api/Program.cs#L26-L30)."  
Senior: "Resource authorization is separate and appears in PTO direct-report checks [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353); that is the IDOR boundary."

**Question:** Where could EF performance degrade?  
Junior: "Database queries."  
Mid-level: "Employee list includes several navigations [Program.cs](../../../server/Nethris.Api/Program.cs#L119-L124)."  
Senior: "Unbounded list and org chart [Program.cs](../../../server/Nethris.Api/Program.cs#L141-L146), [Program.cs](../../../server/Nethris.Api/Program.cs#L257-L261) need measurement, pagination, and query-plan review."

## React

**Question:** What is derived state here?  
Junior: "Filtered employees."  
Mid-level: "The app derives `filteredEmployees`, `visibleRequests`, and `approvalQueue` from state in [App.tsx](../../../client/src/App.tsx#L43-L69)."  
Senior: "Derived UI permissions improve UX but cannot be security; API checks must stay in [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353)."

## Debugging

**Question:** A manager sees a PTO request but approval returns 403. What do you inspect?  
Junior: "The button."  
Mid-level: "Queue filter [App.tsx](../../../client/src/App.tsx#L65-L69) and decision endpoint [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385)."  
Senior: "Check direct-report relationship from data [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L29-L34), stage status, token claims [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L21-L29), then add a regression test."

## System Design

**Question:** Design PTO approval notifications.  
Junior: "Send email after approval."  
Mid-level: "Call an email service after [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381)."  
Senior: "Use an outbox written in the same transaction as approval, process with `BackgroundService`, make delivery idempotent, log failures, and expose retry/dead-letter operations."

## Code Review

**Question:** A PR returns EF entities from `/api/employees`.  
Junior: "It works."  
Mid-level: "It changes the API shape."  
Senior: "It leaks persistence and navigation graph; keep DTO mapping [Program.cs](../../../server/Nethris.Api/Program.cs#L412-L432) and contract stability [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L30-L49)."

## Behavioral Prompts

- Tell me about a time you reduced blast radius: use "added tests around PTO resource authorization" as a concrete story.
- Tell me about a production-readiness gap: discuss dev JWT secret [appsettings.json](../../../server/Nethris.Api/appsettings.json#L5-L9) and migration absence [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11).
- Tell me about mentoring: explain how you would guide a junior through [01-codebase-cartography/05-key-flows.md](../01-codebase-cartography/05-key-flows.md).


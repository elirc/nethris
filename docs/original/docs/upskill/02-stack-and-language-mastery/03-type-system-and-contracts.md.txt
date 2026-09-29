# Type System And Contracts

## C# Records As API Contracts

Request and response shapes are `record` declarations in [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109). Records are concise immutable-ish DTO shapes by convention. They separate API contract shape from EF entity shape.

Real example: [EmployeeResponse](../../../server/Nethris.Api/Contracts/Contracts.cs#L30-L49) includes names such as `Department` and `ManagerName`; the entity stores `DepartmentId`, `Department`, `ManagerId`, and `Manager` in [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L15-L23). This avoids forcing the client to understand EF navigation properties.

Failure mode: adding a field to an entity does not automatically make it safe to expose.

## Nullable Reference Types

Nullable is enabled in [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L2-L5). Entity navigation properties are nullable where EF may not have loaded them, such as `Department?` and `Manager?` in [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L16-L20). Mapping helpers handle nulls with fallbacks in [Program.cs](../../../server/Nethris.Api/Program.cs#L426-L431).

Junior miss: `Department?` does not mean an employee may validly have no department; it often means "this navigation may not be loaded."

## Required Properties

Entities use `required` for core strings such as [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L6-L9) and [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L11-L11). This helps construction but does not replace database constraints or request validation.

## Enums As State Machines

`PtoStatus` is declared in [Enums.cs](../../../server/Nethris.Api/Domain/Enums.cs#L18-L25). The state transitions are implemented procedurally in [Program.cs](../../../server/Nethris.Api/Program.cs#L337-L378). A senior asks whether this should become a dedicated domain service as transitions grow.

## TypeScript Mirror Types

The frontend declares string-union types for status and role in [types.ts](../../../client/src/types.ts#L1-L4) and interfaces for API shapes in [types.ts](../../../client/src/types.ts#L6-L100). These mirror server records but are not generated from them. That is fine for a small app; it becomes drift risk as contracts grow.

Drill: compare [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L80-L91) to [types.ts](../../../client/src/types.ts#L69-L81). List fields that must stay aligned.


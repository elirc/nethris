# 02. Contracts connect shape with meaning

Open [WorkshopContracts.cs](../../../server/Nethris.Api/Contracts/WorkshopContracts.cs) and [HrisApi.cs](../../../server/Nethris.Api/HrisApi.cs). The input records describe the fields a request can supply. The serializer options reject unmapped members, use case-sensitive property matching under the configured web naming policy, disallow integer enum values through the string enum converter, and bound nesting depth. These are boundary choices, not guarantees that every deserialized value is a valid business action.

## Deserialization is only one stage

A JSON object can match the input record shape while naming a nonexistent department, an inactive reference for a new assignment, or a manager relationship that creates a cycle. Those facts require domain and database context. Conversely, an unknown JSON property can be rejected before those domain rules are reached. Tests should distinguish shape rejection from business rejection so their names describe the actual boundary exercised.

Use EmployeeInput as a worked example. Its department and job-role identifiers are structurally GUID values. That does not establish that the records exist or may be newly assigned. Its employment status is a typed enum, but the domain still validates defined values. Its decimal balance must satisfy a quarter-unit rule and remain sufficient for pending reservations. Each layer contributes a different piece of confidence.

## Inputs and views are different contracts

EmployeeView includes display names for related department, role and manager, plus a direct-report count. Those fields are projections rather than values the client can freely set through EmployeeInput. The view also makes PTO balance nullable because authorization affects whether a caller may see it. A null balance in a response can mean redaction, not a stored zero balance or missing employee.

JobRoleView likewise permits nullable salary-band values. A client that converts every null number into zero can misrepresent hidden compensation as a real zero. Model presentation states explicitly: known value, not supplied where relevant, and not visible under the response contract. The exact distinction available depends on the API representation, so do not invent a reason code the current view does not contain.

## Defaults and omitted fields need tests

DepartmentInput and JobRoleInput declare a default active flag. Investigate how the configured serializer handles omitted values in the actual runtime and constructor shape before documenting compatibility. Do not assume every non-nullable C# property automatically produces a friendly required-field error at JSON parsing time. The domain's text and relationship validation remain important.

For a proposed new department description, choose whether omission means unchanged, empty, or absent. The current save route uses a full input record, not a patch document with explicit per-field operations. Adding a nullable property without a clear update contract can erase existing values when an older client omits it. Write an old-client example before selecting a migration and compatibility policy.

## Exercise NH-02A: make a boundary matrix

Create rows for malformed JSON, an unknown property, a numeric enum, an unknown string enum, a valid but nonexistent department identifier, a duplicate employee email, and an otherwise valid stale edit. For each, predict the rejecting layer and the state that remains unchanged. Keep the request body within size bounds and include required headers unless the header itself is the condition under test.

The matrix should not use one malformed request to claim coverage of several later rules. A body rejected by the serializer never proves manager-cycle prevention. Make each fixture's path to the intended boundary explicit. Add a valid control request so a reviewer can see that the basic setup works.

## Exercise NH-02B: design a response consumer

Write a TypeScript presentation model for an employee card that does not confuse a redacted balance with zero. Include display names, stable identity, editing version, and an optional manager. Explain which values are authoritative for an edit and which are convenient display projections. The manager's name should not replace manager identity in a request merely because the name is easier to show.

Then describe a stale reference-name case. A department may be renamed after the employee view is loaded. The employee's department identity remains the same while its displayed label may be old until refresh. This is not necessarily an employee edit conflict. Distinguish relationship identity from the freshness of a projected label.

## Exercise NH-02C: propose a compatible field addition

For department description, write request examples from an old client and a new client, the accepted stored values, and the returned view. Decide whether the first iteration requires a coordinated client/server deployment or supports mixed versions. Include import or database schema compatibility if the field is persisted. Keep the proposal separate from the current source contract, which does not yet contain that field.

The acceptance examples should include clearing a description, whitespace-only input, surrounding whitespace, excessive length and embedded controls. These cases force you to choose a normalization policy. A type of string-or-null is not a complete specification.

## Review standard

Good contract work explains both shape and meaning. It separates client-supplied fields from projections, runtime parsing from domain decisions, and absent values from authorization-sensitive representation. The result should be a small set of request and response examples that another developer can implement without guessing how omitted fields or hidden values behave.

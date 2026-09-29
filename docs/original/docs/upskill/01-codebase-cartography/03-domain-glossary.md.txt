# Domain Glossary

| Term | Meaning here | Anchors |
| --- | --- | --- |
| Employee | Person with profile, department, role, manager, PTO balance, and possible user account. | [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L3-L26), [types.ts](../../../client/src/types.ts#L30-L50) |
| Department | Organizational grouping with cost center and optional lead employee. | [Department.cs](../../../server/Nethris.Api/Domain/Department.cs#L3-L10), [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L37-L41) |
| JobRole | Title, level, and salary band. | [JobRole.cs](../../../server/Nethris.Api/Domain/JobRole.cs#L3-L10), [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L23-L28) |
| UserAccount | Authentication record linked one-to-one to employee. | [UserAccount.cs](../../../server/Nethris.Api/Domain/UserAccount.cs#L3-L12), [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L49-L56) |
| SecurityRole | Role enum used for policies and UI visibility. | [Enums.cs](../../../server/Nethris.Api/Domain/Enums.cs#L10-L16), [Program.cs](../../../server/Nethris.Api/Program.cs#L26-L30) |
| PTO request | Time-off request with dates, hours, reason, status, and approvals. | [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L3-L15), [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L74-L91) |
| PTO approval | Decision event attached to a PTO request. | [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L17-L27), [Program.cs](../../../server/Nethris.Api/Program.cs#L355-L361) |
| Audit log | Append-like record of actor/action/entity/details. | [AuditLog.cs](../../../server/Nethris.Api/Domain/AuditLog.cs#L3-L12), [Program.cs](../../../server/Nethris.Api/Program.cs#L481-L490) |
| Org chart | Recursive tree built from `ManagerId`. | [Program.cs](../../../server/Nethris.Api/Program.cs#L255-L270), [Program.cs](../../../server/Nethris.Api/Program.cs#L434-L449) |

## Drill

For each term, write the invariant that should stay true. Example: "A `PtoApproval` should refer to an existing approver employee." Then find the EF line that enforces or fails to enforce it.


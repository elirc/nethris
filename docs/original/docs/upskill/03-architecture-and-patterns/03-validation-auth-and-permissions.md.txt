# Validation, Auth, And Permissions

## Authentication

JWT bearer authentication is configured in [Program.cs](../../../server/Nethris.Api/Program.cs#L32-L45). Tokens are created in [JwtTokenService.cs](../../../server/Nethris.Api/Security/JwtTokenService.cs#L11-L39). Password hashing uses PBKDF2 with random salt and fixed-time comparison in [PasswordService.cs](../../../server/Nethris.Api/Security/PasswordService.cs#L11-L30).

## Authorization

Role policies are registered in [Program.cs](../../../server/Nethris.Api/Program.cs#L26-L30). Examples:

- Employee create/update requires PeopleOps in [Program.cs](../../../server/Nethris.Api/Program.cs#L201-L253).
- PTO decision requires Approvers in [Program.cs](../../../server/Nethris.Api/Program.cs#L325-L385).
- Audit logs require PeopleOps in [Program.cs](../../../server/Nethris.Api/Program.cs#L387-L395).

Resource-level checks happen inside PTO decision:

- Direct manager check in [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L348).
- Manager blocked from HR-stage final approval in [Program.cs](../../../server/Nethris.Api/Program.cs#L350-L353).

## Validation

PTO dates and hours are validated in [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303). Employee create checks duplicate email/employee number in [Program.cs](../../../server/Nethris.Api/Program.cs#L160-L163). No general request validation framework is present.

## IDOR Checklist

IDOR means insecure direct object reference: a user manipulates an id to access or modify a resource they do not own.

- `GET /api/employees/{id}` allows any authenticated user to fetch any employee profile [Program.cs](../../../server/Nethris.Api/Program.cs#L149-L153). This may be intended for a directory.
- `GET /api/pto` filters employee/manager visibility in [Program.cs](../../../server/Nethris.Api/Program.cs#L272-L293).
- `POST /api/pto/{id}/decision` checks manager ownership in [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L348).

## Junior Misses Vs Senior Checks

| Junior might say | Senior asks |
| --- | --- |
| "The Approvals tab is hidden." | "What happens if the user posts directly to the endpoint?" |
| "The DTO has a `Guid`." | "Is the id scoped to the actor's permissions?" |
| "The form has required fields." | "What does the server reject?" |


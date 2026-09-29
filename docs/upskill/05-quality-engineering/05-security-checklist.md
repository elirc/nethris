> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Security Checklist

Use before merging changes.

| Area | Check | Repo anchor |
| --- | --- | --- |
| Authn | Passwords are hashed and fixed-time compared | [PasswordService.cs](../../../server/Nethris.Api/Security/PasswordService.cs#L11-L30) |
| Token | JWT issuer/audience/key validation enabled | [Program.cs](../../../server/Nethris.Api/Program.cs#L36-L44) |
| Secrets | Development JWT key is not production secret | [appsettings.json](../../../server/Nethris.Api/appsettings.json#L5-L9) |
| Authz | Role policies protect sensitive endpoints | [Program.cs](../../../server/Nethris.Api/Program.cs#L26-L30) |
| IDOR | Resource ownership checked for PTO decisions | [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353) |
| Validation | Server validates PTO hours/dates | [Program.cs](../../../server/Nethris.Api/Program.cs#L300-L303) |
| CORS | Origins limited to dev frontend | [Program.cs](../../../server/Nethris.Api/Program.cs#L17-L23) |
| XSS | React escapes text by default; watch any future `dangerouslySetInnerHTML` | none found |
| CSRF | Bearer tokens reduce classic cookie CSRF; storage strategy still matters | [api.ts](../../../client/src/api.ts#L93-L96) |
| SQL injection | EF LINQ used, no raw SQL found | [Program.cs](../../../server/Nethris.Api/Program.cs#L117-L147) |
| Rate limiting | Not present; add for login | [Program.cs](../../../server/Nethris.Api/Program.cs#L70-L83) |
| Audit | Sensitive writes audited | [Program.cs](../../../server/Nethris.Api/Program.cs#L196-L197), [Program.cs](../../../server/Nethris.Api/Program.cs#L380-L381) |

Pre-merge question: "Can a user change an id, role, or status and see/modify something outside their authority?"


# Data Model And Persistence

## Entities

`Employee` is central: identity, employee number, name, email, phone, location, hire date, status, PTO balance, department, role, manager, user account, and PTO requests are in [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L5-L23). PTO request and approval are in [PtoRequest.cs](../../../server/Nethris.Api/Domain/PtoRequest.cs#L3-L27).

## Relationships

| Relationship | EF config | Judgment |
| --- | --- | --- |
| Employee -> Manager/direct reports | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L23-L26) | Restrict delete avoids cascading through org tree. |
| Employee -> Department | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L27-L30) | Restrict delete protects employee records. |
| Employee -> JobRole | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L31-L34) | Restrict delete protects job history-ish data. |
| Department -> LeadEmployee | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L37-L41) | Set null allows lead to be removed without deleting department. |
| UserAccount -> Employee | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L49-L56) | Cascade delete removes account with employee. |
| PTO -> Employee | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L58-L64) | Cascade deletes PTO with employee. |
| Approval -> Approver | [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L67-L71) | Restrict delete protects approval history. |

## Indexes And Precision

Unique indexes exist for employee email and number in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L20-L21), and user email in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L49-L51). Money/hour precision is set in [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L22-L22), [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L43-L47), and [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L58-L60).

## Migrations

No `Migrations/` folder was found. Current seeding uses `EnsureCreatedAsync` in [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L11-L11). Safe schema-change guidance:

1. Add migrations before production-like use.
2. Replace `EnsureCreatedAsync` with `Database.MigrateAsync`.
3. Add migration tests or at least run migration up/down on a copy.
4. Keep seed data idempotent; current early exit is [SeedData.cs](../../../server/Nethris.Api/Data/SeedData.cs#L12-L15).

## Drill

Write a migration plan for adding `PtoType`. Include API contract, entity, DbContext, seed, UI, tests, and rollback.


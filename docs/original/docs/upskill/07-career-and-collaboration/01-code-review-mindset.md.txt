# Code Review Mindset

Review in this order:

1. Does it work?
2. Is it correct under edge cases?
3. Will it stay correct?
4. Does it fit this codebase?
5. Is it kind to maintainers?

Repo checklist:

- Auth boundary preserved? [Program.cs](../../../server/Nethris.Api/Program.cs#L26-L30), [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353)
- Contract/entity split preserved? [Contracts.cs](../../../server/Nethris.Api/Contracts/Contracts.cs#L5-L109), [Employee.cs](../../../server/Nethris.Api/Domain/Employee.cs#L3-L26)
- EF relationship/delete behavior safe? [HrisDbContext.cs](../../../server/Nethris.Api/Data/HrisDbContext.cs#L18-L71)
- React state updated immutably? [App.tsx](../../../client/src/App.tsx#L104-L119)
- Tests added or gap explained? No test project exists yet.

Good comment example:

> Blocking: this change relies on hiding the button in React, but the API still accepts the request. Please add a server-side resource check like [Program.cs](../../../server/Nethris.Api/Program.cs#L342-L353) and cover it with an integration test.


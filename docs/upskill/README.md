> **Historical prototype documentation.** This page describes the original application or earlier proposals. Use the [current CRUD learning course](../../astraupskill/README.md) for the implemented workshop, setup, permissions and verification. The exact original is preserved under `docs/original`.

# Nethris Upskill Lab

This curriculum turns this C#/.NET and React HRIS app into a training lab for a junior engineer moving toward mid-level and senior judgment. Nethris is a compact employee directory and PTO workflow app: the ASP.NET Core minimal API lives in [server/Nethris.Api/Program.cs](../../server/Nethris.Api/Program.cs#L12-L397), the EF Core model is configured in [server/Nethris.Api/Data/HrisDbContext.cs](../../server/Nethris.Api/Data/HrisDbContext.cs#L6-L73), and the React shell lives in [client/src/App.tsx](../../client/src/App.tsx#L29-L543). The repo has one API project targeting .NET 8 in [server/Nethris.Api/Nethris.Api.csproj](../../server/Nethris.Api/Nethris.Api.csproj#L1-L17) and one Vite client with scripts in [client/package.json](../../client/package.json#L6-L22). It currently has no test projects, no CI workflow, no migrations folder, no Docker files, and no hosted worker code; those are deliberate training gaps recorded in [08-reference/risk-register.md](08-reference/risk-register.md). The highest-value real flows are login, directory loading, org chart, PTO request, PTO approval, and audit logging. The strongest learning tension is that the app is complete enough to trace end to end, but small enough that you can critique boundaries, tests, security, and operations without drowning.

Senior mindset: a junior asks "how do I make it work?", a mid-level engineer asks "is this the right pattern?", and a senior asks "what does this commit us to, who pays for it, and how do we reduce risk?"

## How To Use This

| Timebox | Path |
| --- | --- |
| Weekend | Read [00-fast-track.md](00-fast-track.md), trace PTO in [01-codebase-cartography/05-key-flows.md](01-codebase-cartography/05-key-flows.md), do two drills in [04-code-reading-gym/01-annotation-drills.md](04-code-reading-gym/01-annotation-drills.md). |
| Two weeks | Add the stack docs in [02-stack-and-language-mastery/](02-stack-and-language-mastery/), then attempt five good-first tickets in [06-contribution-practice/01-good-first-tickets.md](06-contribution-practice/01-good-first-tickets.md). |
| Eight weeks | Work through architecture, testing, debugging, performance, security, review katas, and two mid-level projects. |
| Ongoing | Keep [08-reference/risk-register.md](08-reference/risk-register.md) and [08-reference/verification-log.md](08-reference/verification-log.md) current as the code changes. |

## Tracks

- Brand-new junior: start with [01-codebase-cartography/02-file-reading-order.md](01-codebase-cartography/02-file-reading-order.md), then annotate [client/src/App.tsx](../../client/src/App.tsx#L71-L162) and [server/Nethris.Api/Program.cs](../../server/Nethris.Api/Program.cs#L295-L385).
- Junior with stack familiarity: follow the key flows, then implement documentation-only tickets or add tests after product-code permission.
- Mid-level new to repo: study [03-architecture-and-patterns/01-boundaries-and-layers.md](03-architecture-and-patterns/01-boundaries-and-layers.md), [05-quality-engineering/01-testing-strategy.md](05-quality-engineering/01-testing-strategy.md), and [06-contribution-practice/02-mid-level-feature-tickets.md](06-contribution-practice/02-mid-level-feature-tickets.md).
- Senior reviewing architecture: start with [03-architecture-and-patterns/06-architecture-critique.md](03-architecture-and-patterns/06-architecture-critique.md), [08-reference/risk-register.md](08-reference/risk-register.md), and [07-career-and-collaboration/04-interview-prep-from-this-repo.md](07-career-and-collaboration/04-interview-prep-from-this-repo.md).

## Conventions

- Every codebase claim should include a path and line anchor.
- "Illustrative fake code" means the snippet is not in this repo.
- "Investigate" means evidence exists, but no failing test or runtime reproduction was verified.
- Drills are meant to be done with the editor open. Write answers before checking the rubric.
- Verification notes appear in key docs and in [08-reference/verification-log.md](08-reference/verification-log.md).


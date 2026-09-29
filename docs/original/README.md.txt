# Nethris HRIS

A compact HRIS / employee directory platform inspired by BambooHR and Rippling. It includes employee profiles, departments, job roles, an org chart, PTO requests, approvals, audit logs, and role-based API authorization.

## Stack

- ASP.NET Core 8 minimal APIs
- EF Core with SQLite
- JWT bearer auth with seeded users
- React + TypeScript + Vite
- CSS-first responsive UI

## Seed Logins

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@nethris.local | `Password123!` |
| HR | hr@nethris.local | `Password123!` |
| Manager | maya@nethris.local | `Password123!` |
| Employee | alex@nethris.local | `Password123!` |

## Run

Backend:

```powershell
cd server/Nethris.Api
dotnet restore
dotnet run
```

Frontend:

```powershell
cd client
npm install
npm run dev
```

The API defaults to `http://localhost:5187`. The frontend uses `VITE_API_BASE_URL` when set, otherwise that default.

Note: `dotnet` was not available on this machine while scaffolding, so the C# project is ready for restore/build but was not locally compiled here.


# Command Cheatsheet

| Goal | Command | Status |
| --- | --- | --- |
| List repo files | `rg --files -g '!client/node_modules/**' -g '!client/dist/**'` | Verified. |
| Frontend install | `cd client; npm install --include=dev` | Verified earlier. |
| Frontend build | `cd client; npm run build` | Verified passing. |
| Frontend dev | `cd client; npm run dev` | Inferred from [client/package.json](../../../client/package.json#L6-L9). |
| Frontend preview | `cd client; npm run preview` | Inferred from [client/package.json](../../../client/package.json#L6-L9). |
| Backend restore | `cd server/Nethris.Api; dotnet restore` | Blocked: `dotnet` not on PATH. |
| Backend run | `cd server/Nethris.Api; dotnet run` | Inferred from [Nethris.Api.csproj](../../../server/Nethris.Api/Nethris.Api.csproj#L1-L17). |
| Backend health | `Invoke-WebRequest http://localhost:5187/api/health` | Inferred from [Program.cs](../../../server/Nethris.Api/Program.cs#L68-L68). |
| .NET vulnerable packages | `dotnet list package --vulnerable` | Blocked until SDK exists. |
| npm audit | `cd client; npm audit` | Inferred; earlier npm output reported two moderate findings. |
| EF migration add | `dotnet ef migrations add <Name>` | Future-only; no migrations present. |
| Tests | `dotnet test` / `npm test` | Not available; no test setup found. |


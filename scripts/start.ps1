$ErrorActionPreference = 'Stop'
$packageRoot = Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts'
if (-not (Test-Path -LiteralPath (Join-Path $packageRoot 'Nethris.Api.dll'))) { throw 'Run scripts/package.ps1 first.' }
Push-Location $packageRoot
try { dotnet Nethris.Api.dll; if ($LASTEXITCODE -ne 0) { throw 'Nethris stopped with an error.' } }
finally { Pop-Location }

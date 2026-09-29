param([switch]$SkipInstall)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    if (-not $SkipInstall) {
        dotnet restore tests/Nethris.Tests/Nethris.Tests.csproj --locked-mode
        if ($LASTEXITCODE -ne 0) { throw 'Locked .NET restore failed.' }
        Push-Location client
        try { npm.cmd ci --ignore-scripts --no-audit --no-fund; if ($LASTEXITCODE -ne 0) { throw 'Locked npm install failed.' } }
        finally { Pop-Location }
    }
    Push-Location client
    try { npm.cmd run build; if ($LASTEXITCODE -ne 0) { throw 'Frontend build failed.' } }
    finally { Pop-Location }
    dotnet publish server/Nethris.Api/Nethris.Api.csproj -c Release --no-restore -o artifacts
    if ($LASTEXITCODE -ne 0) { throw 'API publish failed.' }
    $webTarget = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts/wwwroot'))
    $expectedTarget = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
    if (-not $webTarget.StartsWith($expectedTarget, [StringComparison]::OrdinalIgnoreCase)) { throw 'Invalid package asset target.' }
    if (Test-Path -LiteralPath $webTarget) { Remove-Item -LiteralPath $webTarget -Recurse -Force }
    Copy-Item -LiteralPath (Join-Path $projectRoot 'client/dist') -Destination $webTarget -Recurse
    Write-Output 'Built artifacts/Nethris.Api.dll with the matching frontend. Run scripts/start.ps1.'
}
finally { Pop-Location }

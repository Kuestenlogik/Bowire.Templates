# Local dev loop: uninstall any previous install, re-pack the template,
# and reinstall the freshly built nupkg. After this, `dotnet new bowire-plugin`
# reflects the current sources in src/.
# Inspired by https://github.com/akkadotnet/akkadotnet-templates/blob/dev/install-dev-templates.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    dotnet new uninstall Kuestenlogik.Bowire.Templates 2>$null | Out-Null

    if (Test-Path "bin/Release") {
        Get-ChildItem -Path "bin/Release" -Recurse -Filter "*.nupkg" -ErrorAction SilentlyContinue |
            Remove-Item -Force
    }

    & "$repoRoot/build.ps1"
    dotnet pack -c Release
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }

    $latest = Get-ChildItem -Path "bin/Release" -Recurse -Filter "Kuestenlogik.Bowire.Templates.*.nupkg" |
              Sort-Object LastWriteTime -Descending |
              Select-Object -First 1

    if ($null -eq $latest) { throw "No .nupkg produced under bin/Release" }

    dotnet new install $latest.FullName
    if ($LASTEXITCODE -ne 0) { throw "dotnet new install failed" }
}
finally {
    Pop-Location
}

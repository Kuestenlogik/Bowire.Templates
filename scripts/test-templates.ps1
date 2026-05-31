# Smoke-tests the bowire-plugin template across its relevant parameter
# combinations. Each variant is scaffolded into a fresh folder under output/,
# restored, built, and tested. Any failure fails the whole script.
#
# Required: the template must already be installed via `dotnet new install`,
# OR pass -Install to have the script pack + install the current sources
# before running. The CI workflow packs + installs separately.
#
# Optional parameter -BowireSdkVersion pins Kuestenlogik.Bowire to a specific version
# (useful when testing against local Prereleases that floating ranges ignore).
# When omitted, the default from Directory.Packages.props is used.
#
# Pass -Clean to wipe any leftover output/run-* folders from previous runs
# before starting the fresh one.
#
# Inspired by https://github.com/akkadotnet/akkadotnet-templates/blob/dev/scripts/test-templates.ps1

[CmdletBinding()]
param(
    [string]$BowireSdkVersion,
    [switch]$Install,
    [switch]$Clean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Exec {
    [CmdletBinding()]
    param(
        [Parameter(Position=0, Mandatory=$true)][scriptblock]$Cmd,
        [Parameter(Position=1, Mandatory=$false)][string]$ErrorMessage = "Command failed: $Cmd"
    )
    & $Cmd
    if ($LASTEXITCODE -ne 0) {
        throw "Exec: $ErrorMessage"
    }
}

function Test-BowirePluginVariant {
    param(
        [Parameter(Mandatory=$true)][string]$Name,
        [Parameter(Mandatory=$true)][string]$IncludeDuplexChannel,
        [string]$ProjectOnly = "false",
        [string]$Preset = "none",
        [string]$IncludeIntegrationTests = "false",
        [string]$Minimal = "false",
        [string]$ProtocolId = ""
    )

    $folder = "$OutputRoot/$Name"
    Write-Output "=== $Name (Preset=$Preset, IncludeDuplexChannel=$IncludeDuplexChannel, ProjectOnly=$ProjectOnly, IncludeIntegrationTests=$IncludeIntegrationTests, Minimal=$Minimal) ==="

    $newArgs = @("new", "bowire-plugin", "-n", $Name, "-o", $folder,
                 "--Preset", $Preset,
                 "--IncludeDuplexChannel", $IncludeDuplexChannel,
                 "--ProjectOnly", $ProjectOnly,
                 "--IncludeIntegrationTests", $IncludeIntegrationTests,
                 "--Minimal", $Minimal,
                 "--skipRestore")
    if (-not [string]::IsNullOrEmpty($BowireSdkVersion)) {
        $newArgs += @("--BowireSdkVersion", $BowireSdkVersion)
    }
    if (-not [string]::IsNullOrEmpty($ProtocolId)) {
        $newArgs += @("--ProtocolId", $ProtocolId)
    }
    Exec { dotnet @newArgs } "dotnet new bowire-plugin failed for $Name"

    # Default mode: pin Kuestenlogik.Bowire in Directory.Packages.props to the
    # caller-supplied version (handles local Prerelease packages like
    # 0.9.4 that the floating default in the template ignores).
    # ProjectOnly / Minimal modes pin the version inside the csproj itself
    # and need no post-processing.
    $selfContained = ($ProjectOnly -eq "true") -or ($Minimal -eq "true")
    if (-not [string]::IsNullOrEmpty($BowireSdkVersion) -and -not $selfContained) {
        $packagesProps = Join-Path $folder "Directory.Packages.props"
        (Get-Content $packagesProps) `
            -replace 'Include="KL\.Bowire" Version=".*?"', "Include=`"Kuestenlogik.Bowire`" Version=`"$BowireSdkVersion`"" |
            Set-Content $packagesProps
    }

    $pluginCsproj = "$folder/src/$Name/$Name.csproj"
    $testsCsproj  = "$folder/tests/$Name.Tests/$Name.Tests.csproj"

    if ($selfContained) {
        # No .slnx in ProjectOnly / Minimal mode — point dotnet at the
        # csprojs directly.
        Exec { dotnet build $pluginCsproj -c Release } "dotnet build failed for $Name (plugin)"
        Exec { dotnet test  $testsCsproj  -c Release } "dotnet test failed for $Name (tests)"
    }
    else {
        Exec { dotnet build "$folder" -c Release } "dotnet build failed for $Name"
        Exec { dotnet test  "$folder" -c Release --no-build } "dotnet test failed for $Name"
    }

    # Prove the generated plugin packs cleanly — catches missing Authors /
    # PackageReadmeFile / other NuGet metadata issues that dotnet build won't.
    # VersionSuffix=smoke keeps NuGet's NU5104 happy when we're restoring
    # against a local Prerelease Kuestenlogik.Bowire (stable packages can't depend on
    # prereleases); real publishes use the csproj's VersionPrefix as-is.
    Exec { dotnet pack $pluginCsproj -c Release -o "$folder/artifacts/packages" --no-build -p:VersionSuffix=smoke } `
        "dotnet pack failed for $Name"

    $nupkg = Get-ChildItem -Path "$folder/artifacts/packages" -Filter "$Name.*.nupkg" -ErrorAction SilentlyContinue |
             Select-Object -First 1
    if ($null -eq $nupkg) {
        throw "dotnet pack produced no .nupkg for $Name"
    }
    Write-Output "  -> packed $($nupkg.Name) ($([math]::Round($nupkg.Length / 1KB, 1)) KB)"
}

Push-Location $PSScriptRoot/..
try {
    if ($Install) {
        Write-Output "=== Pack + install template from current sources ==="
        & "$PSScriptRoot/install-dev-templates.ps1"
        if ($LASTEXITCODE -ne 0) { throw "install-dev-templates.ps1 failed" }
    }

    # Each run writes into output/run-<timestamp>/ so stale directory handles
    # from the previous smoke (antivirus, search indexer, ...) can't block
    # the next one. Old runs stay behind; pass -Clean to wipe any previous
    # output/run-* folders before the fresh run starts.
    if ($Clean -and (Test-Path "output")) {
        Get-ChildItem "output" -Directory -Filter "run-*" -Force -ErrorAction SilentlyContinue |
            ForEach-Object {
                try { Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop }
                catch { Write-Warning "Could not remove $($_.FullName): $($_.Exception.Message)" }
            }
    }

    $OutputRoot = "output/run-{0:yyyyMMdd-HHmmss}" -f (Get-Date)
    New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
    Write-Output "=== Run artifacts: $OutputRoot ==="

    function Test-SidecarVariant {
        # Polyglot sidecar variants can't be built/tested by `dotnet test`
        # (they're Python / Node / Rust / Go projects). Smoke = instantiate
        # via `dotnet new` and verify the expected entry-point files land
        # — proves template.json + sources blocks are wired correctly.
        param(
            [Parameter(Mandatory=$true)][string]$Name,
            [Parameter(Mandatory=$true)][string]$Sidecar,
            [Parameter(Mandatory=$true)][string[]]$ExpectedFiles,
            [string]$ProtocolId = ""
        )
        $folder = "$OutputRoot/$Name"
        Write-Output "=== $Name (Sidecar=$Sidecar) ==="
        $newArgs = @("new", "bowire-plugin", "-n", $Name, "-o", $folder, "--Sidecar", $Sidecar, "--skipRestore")
        if (-not [string]::IsNullOrEmpty($ProtocolId)) {
            $newArgs += @("--ProtocolId", $ProtocolId)
        }
        Exec { dotnet @newArgs } "dotnet new bowire-plugin --Sidecar $Sidecar failed for $Name"

        foreach ($expected in $ExpectedFiles) {
            $path = Join-Path $folder $expected
            if (-not (Test-Path $path)) {
                throw "Expected file missing in $Name scaffold: $expected"
            }
        }
        Write-Output "  -> all $($ExpectedFiles.Count) expected files present"
    }

    Test-BowirePluginVariant -Name "Smoke.NoChannel"       -IncludeDuplexChannel "false"
    Test-BowirePluginVariant -Name "Smoke.WithChannel"     -IncludeDuplexChannel "true"
    Test-BowirePluginVariant -Name "Smoke.ProjectOnly"     -IncludeDuplexChannel "false" -ProjectOnly "true"
    Test-BowirePluginVariant -Name "Smoke.PresetRest"      -IncludeDuplexChannel "false" -Preset "rest"
    Test-BowirePluginVariant -Name "Smoke.PresetMqtt"      -IncludeDuplexChannel "true"  -Preset "mqtt"
    Test-BowirePluginVariant -Name "Smoke.PresetWebSocket" -IncludeDuplexChannel "true"  -Preset "websocket"
    Test-BowirePluginVariant -Name "Smoke.PresetGrpc"      -IncludeDuplexChannel "false" -Preset "grpc"
    Test-BowirePluginVariant -Name "Smoke.PresetSignalR"   -IncludeDuplexChannel "true"  -Preset "signalr"
    # Name contains "Bowire" so Bowire's auto-discovery (which only scans
    # assemblies whose name contains "Bowire") picks up the plugin inside
    # the TestServer. Without the magic substring the /api/protocols endpoint
    # would return [].
    Test-BowirePluginVariant -Name "Smoke.Bowire.IntegrationTests" -IncludeDuplexChannel "false" -IncludeIntegrationTests "true"
    Test-BowirePluginVariant -Name "Smoke.DirtyProtocolId" -IncludeDuplexChannel "false" -ProtocolId "My Proto!!"
    Test-BowirePluginVariant -Name "Smoke.Minimal"         -IncludeDuplexChannel "false" -Minimal "true"

    Test-SidecarVariant -Name "Smoke.Sidecar.Python" -Sidecar "python" `
        -ProtocolId "py-side" `
        -ExpectedFiles @("sidecar.json", "pyproject.toml", "src/py_side/__main__.py", "src/py_side/plugin.py", "tests/test_plugin.py")

    Test-SidecarVariant -Name "Smoke.Sidecar.Node" -Sidecar "node" `
        -ProtocolId "node-side" `
        -ExpectedFiles @("sidecar.json", "package.json", "tsconfig.json", "src/index.ts", "src/plugin.ts", "tests/plugin.test.ts")

    Test-SidecarVariant -Name "Smoke.Sidecar.Rust" -Sidecar "rust" `
        -ProtocolId "rust-side" `
        -ExpectedFiles @("sidecar.json", "Cargo.toml", "src/main.rs", "README.md")

    Test-SidecarVariant -Name "Smoke.Sidecar.Go" -Sidecar "go" `
        -ProtocolId "go-side" `
        -ExpectedFiles @("sidecar.json", "go.mod", "main.go", "main_test.go", "README.md")

    # Verify ProtocolId got lowercased + stripped to [a-z0-9_-].
    $dirtyProtocol = Get-Content "$OutputRoot/Smoke.DirtyProtocolId/src/Smoke.DirtyProtocolId/MyProtocol.cs" -Raw
    if ($dirtyProtocol -notmatch 'Id => "myproto"') {
        throw 'ProtocolId cleanup failed: expected Id => "myproto" in generated MyProtocol.cs'
    }
    Write-Output '  -> ProtocolId cleanup verified: "My Proto!!" -> "myproto"'

    Write-Output ""
    Write-Output "All template variants built and tested successfully."
}
finally {
    Pop-Location
}

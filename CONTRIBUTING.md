# Contributing to Bowire.Templates

Thanks for improving the [Bowire](https://github.com/Kuestenlogik/Bowire) plugin scaffolder. This repo is small and self-contained — the feedback loop is fast.

## Prerequisites

- .NET 10 SDK (preview).
- PowerShell 7+ (the helper scripts are `.ps1`).
- Optional, only if you want the smoke tests to build against the actual Bowire SDK: a local clone of [Kuestenlogik/Bowire](https://github.com/Kuestenlogik/Bowire) packed into `artifacts/packages/`. The root `nuget.config` already points at `C:\Projekte\KL\Bowire\artifacts\packages`; adjust it for your path or pin `Kuestenlogik.Bowire` to an nuget.org version in `src/bowire-plugin/Directory.Packages.props`.

## Local dev loop

One command to rebuild and re-install the template from the current sources:

```pwsh
./scripts/install-dev-templates.ps1
```

That produces `bin/Release/Kuestenlogik.Bowire.Templates.<version>.nupkg` and runs `dotnet new install` on it, so `dotnet new bowire-plugin` reflects whatever you just edited.

Run the full smoke-test matrix (covers every parameter combination CI runs):

```pwsh
./scripts/test-templates.ps1 -BowireSdkVersion 0.9.4 -Install
```

The `-Install` switch chains `install-dev-templates.ps1` before the smoke run, so a single invocation is "pack + install + generate + build + test + pack" across every variant. Each run writes into `output/run-<timestamp>/`; clean up old runs by hand when they pile up.

## What lives where

```
Bowire.Templates/
├── Kuestenlogik.Bowire.Templates.csproj        # the pack project (dotnet pack -> the nupkg)
├── RELEASE_NOTES.md                   # single source of truth for version + changelog
├── build.ps1                          # injects RELEASE_NOTES into the csproj before pack
├── nuget.config                       # nuget.org + local Bowire feed
├── docs/                              # per-template markdown docs
├── scripts/
│   ├── install-dev-templates.ps1      # local dev loop
│   ├── test-templates.ps1             # smoke matrix
│   ├── getReleaseNotes.ps1            # parses RELEASE_NOTES.md
│   └── bumpVersion.ps1                # writes VersionPrefix/PackageReleaseNotes into the csproj
├── .github/
│   ├── workflows/                     # pr_validation + publish_nuget
│   └── dependabot.yml                 # weekly NuGet + Actions updates
└── src/bowire-plugin/
    ├── .template.config/
    │   ├── template.json              # symbols, conditions, sources
    │   └── ide.host.json              # VS / Rider wizard metadata
    ├── Directory.Build.props          # TFM + package metadata for the generated project
    ├── Directory.Packages.props       # CPM; extra versions (MQTTnet, Grpc.Net.Client, ...) conditional on --Preset
    ├── Bowire.Plugin1.slnx           # conditional IntegrationTests project
    ├── src/Bowire.Plugin1/
    │   ├── Bowire.Plugin1.csproj     # conditional PackageReferences for presets + ProjectOnly
    │   ├── MyProtocol.cs              # bodies branched by --Preset
    │   └── MyProtocolChannel.cs       # only when --IncludeDuplexChannel
    └── tests/
        ├── Bowire.Plugin1.Tests/            # always present
        └── Bowire.Plugin1.IntegrationTests/  # only when --IncludeIntegrationTests
```

## Editing the template

- **Adding a parameter** — add a symbol to `src/bowire-plugin/.template.config/template.json`, then optionally wire it into `ide.host.json` so Visual Studio's wizard picks it up. Use `replaces` for string symbols or `condition` for bool/choice gates.
- **Conditional content** — `#if (SymbolName)` in `.cs` files, `<!--#if (SymbolName) -->` in csproj/slnx/props. The template engine strips the directive itself at generation time.
- **Adding a `--Preset` value** — new `choice` in the Preset symbol, matching `#if (Preset == "<name>")` branches in `MyProtocol.cs`, and conditional `PackageReference` in both `Bowire.Plugin1.csproj` (with a `ProjectOnly` fork that pins the version inline) and `Directory.Packages.props`.
- **Adding a smoke variant** — one more `Test-BowirePluginVariant` call at the bottom of `scripts/test-templates.ps1`.

## Commit + release

- `RELEASE_NOTES.md` is the single source of truth. Version bumps go there first; `build.ps1` syncs the csproj on every pack.
- Commit messages: imperative mood, one-line subject, body explains *why*. Mirror the style of recent commits (`git log --oneline`).
- Tag `vX.Y.Z` on `main` triggers `publish_nuget.yml`, which packs, pushes to nuget.org, and opens a GitHub Release with the latest entry from `RELEASE_NOTES.md` as the body.

## Reporting bugs and feature requests

Use the issue templates under `.github/ISSUE_TEMPLATE/`. Include the output of `dotnet --version`, your OS, and the smoke-test variant that fails if applicable — it makes reproduction an order of magnitude easier.

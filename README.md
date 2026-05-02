# Bowire.Templates

[![CI](https://github.com/Kuestenlogik/Bowire.Templates/actions/workflows/pr_validation.yml/badge.svg)](https://github.com/Kuestenlogik/Bowire.Templates/actions/workflows/pr_validation.yml)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

`dotnet new` templates for scaffolding [Bowire](https://github.com/Kuestenlogik/Bowire) protocol plugins.

Will be published to nuget.org as **`Kuestenlogik.Bowire.Templates`** once the accompanying `Kuestenlogik.Bowire` SDK is public; NuGet version / download badges are added at that point.

## Install

```bash
dotnet new install Kuestenlogik.Bowire.Templates
```

## Available templates

| Template                 | Short name         | Docs                                                        |
|--------------------------|--------------------|-------------------------------------------------------------|
| Bowire Protocol Plugin  | `bowire-plugin`   | [docs/BowirePluginTemplate.md](docs/BowirePluginTemplate.md) |

## Quickstart

```bash
dotnet new bowire-plugin -n Contoso.Bowire.Protocol.Foo
cd Contoso.Bowire.Protocol.Foo
dotnet test
```

See [docs/BowirePluginTemplate.md](docs/BowirePluginTemplate.md) for the full parameter list and what the generated scaffold does. `--ProjectOnly true` emits just the two csprojs (no solution, no build props) for dropping into an existing monorepo.

## Uninstall

```bash
dotnet new uninstall Kuestenlogik.Bowire.Templates
```

## Developing this repo

```
Bowire.Templates/
├── Kuestenlogik.Bowire.Templates.csproj      # template pack project
├── RELEASE_NOTES.md                 # single source of truth for version + changelog
├── build.ps1                        # injects RELEASE_NOTES into the csproj before pack
├── nuget.config                     # nuget.org + local Bowire feed
├── docs/                            # per-template markdown docs
├── scripts/                         # dev + CI helpers
│   ├── install-dev-templates.ps1    # local dev loop: pack + uninstall + install
│   ├── test-templates.ps1           # smoke-test both template variants
│   ├── getReleaseNotes.ps1
│   └── bumpVersion.ps1
└── src/
    └── bowire-plugin/              # template source
        ├── .template.config/
        ├── src/Bowire.Plugin1/
        └── tests/Bowire.Plugin1.Tests/
```

Local dev loop:

```pwsh
./scripts/install-dev-templates.ps1
# -> produces bin/Release/Kuestenlogik.Bowire.Templates.<version>.nupkg and installs it
```

Run the same smoke tests CI runs:

```pwsh
./scripts/test-templates.ps1 -BowireSdkVersion 0.9.4 -Install
```

`-Install` packs and installs the template from the current sources before
running the smoke tests. Leave it off if you already ran
`install-dev-templates.ps1` separately.

The `-BowireSdkVersion` parameter pins `Kuestenlogik.Bowire` to a local Prerelease package (check `C:\Projekte\KL\Bowire\artifacts\packages\` for the latest build). Omit it to use whatever is published to nuget.org.

## License

Apache 2.0 — see [LICENSE](LICENSE).

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

| Template                 | Short name         | Kind    | Docs                                                        |
|--------------------------|--------------------|---------|-------------------------------------------------------------|
| Bowire Protocol Plugin   | `bowire-plugin`    | project | [docs/BowirePluginTemplate.md](docs/BowirePluginTemplate.md) |
| Bowire CLI command       | `bowire-cli-cmd`   | item    | adds an `IBowireCliCommand` scaffold (`bowire <verb>`)      |
| Bowire mock emitter      | `bowire-mock-emit` | item    | adds an `IBowireMockEmitter` scaffold (recording replay)    |

`bowire-plugin` is a **project** template — scaffolds a full plugin repo. The two **item** templates drop a single C# file into an existing Bowire-plugin project so you can wire in additional extension points without re-scaffolding.

## Quickstart

```bash
# Full project scaffold
dotnet new bowire-plugin -n Contoso.Bowire.Protocol.Foo
cd Contoso.Bowire.Protocol.Foo
dotnet test

# Add a `bowire foo` CLI subcommand to that plugin
dotnet new bowire-cli-cmd -n FooCommand --Verb foo

# Add a recording-replay emitter to that plugin
dotnet new bowire-mock-emit -n FooMockEmitter --EmitterId foo
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
    ├── bowire-plugin/              # project template (full plugin repo)
    │   ├── .template.config/
    │   ├── src/Bowire.Plugin1/
    │   └── tests/Bowire.Plugin1.Tests/
    ├── bowire-cli-cmd/             # item template — IBowireCliCommand scaffold
    │   ├── .template.config/
    │   └── MyCommand.cs
    └── bowire-mock-emit/           # item template — IBowireMockEmitter scaffold
        ├── .template.config/
        └── MyMockEmitter.cs
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

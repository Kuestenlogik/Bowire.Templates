# Bowire.Templates — Roadmap

Version history and planned work for the `Kuestenlogik.Bowire.Templates` NuGet package.

## v0.1.0 — Initial release

The template ships a single `dotnet new bowire-plugin` scaffolder that produces a fully-working Bowire protocol plugin project.

### Scaffold output

- [x] **Working `IBowireProtocol` implementation** — `DiscoverAsync`, `InvokeAsync`, `InvokeStreamAsync`, `OpenChannelAsync` with functional demo bodies that compile and run against the `Kuestenlogik.Bowire` NuGet package.
- [x] **`PackageType=BowirePlugin`** set so Bowire's CLI + in-app marketplace recognize the package.
- [x] **.slnx solution** (new XML-based format, not legacy `.sln`).
- [x] **xunit unit tests** — exercise `Name`/`Id`/`IconSvg`, `DiscoverAsync`, `InvokeAsync` (echo round-trip) and `OpenChannelAsync` so `dotnet test` passes out of the box.
- [x] **Optional GitHub Actions CI** (`ci.yml`) — builds, tests, and on `v*` tags packs + pushes to nuget.org.
- [x] **Optional `--IncludeDuplexChannel`** — `MyProtocolChannel : IBowireChannel` demo for bidirectional protocols (WebSocket / SignalR / MQTT), plus an extra xunit test exercising the channel.
- [x] **Optional `--IncludeIntegrationTests`** — second test project hosting the plugin inside an ASP.NET Core `WebApplication.UseTestServer()` and hitting `/bowire/api/protocols` + `/bowire/api/services` over HTTP.
- [x] **`--ProjectOnly`** — emits only `src/` + `tests/` (self-contained csprojs with inline `TargetFramework` and pinned versions) for drop-in to an existing monorepo.
- [x] **`--Minimal`** — shortcut that combines `ProjectOnly=true + IncludeCI=false + IncludeIntegrationTests=false` for the leanest possible output.
- [x] **`--IconSvg`** — raw SVG markup embedded in the generated `IconSvg` property.

### Template parameters

- [x] `-n, --name` — free-form project name.
- [x] `--DisplayName` (formerly `--ProtocolName`) — UI tab label.
- [x] `--ProtocolId` — auto-lowercased and stripped to `[a-z0-9_-]` via template-engine `casing` + `regex` generators.
- [x] `--PluginClassName`.
- [x] `--Author` / `--Company` — default to `"Plugin Author"` so `dotnet pack` doesn't warn on empty metadata.
- [x] `--BowireSdkVersion` (formerly `--BowireVersion`) — version range of the `Kuestenlogik.Bowire` NuGet reference.
- [x] `--IncludeCI`, `--IncludeDuplexChannel`, `--IncludeIntegrationTests`, `--ProjectOnly`, `--Minimal`, `--IconSvg`.
- [x] **`--Preset none|rest|mqtt|websocket|grpc|signalr`** — pre-fills `DiscoverAsync` / `InvokeAsync` with transport-specific starter code (HttpClient + OpenAPI probe; MQTTnet connect+publish; ClientWebSocket frame roundtrip; `GrpcChannel.ConnectAsync`; SignalR `HubConnection.InvokeAsync`). Conditional `PackageReference`s pulled in automatically.

### Developer ergonomics

- [x] **Akka.NET-templates style layout** — template sources under `src/bowire-plugin/`, pack csproj at the repo root.
- [x] **`RELEASE_NOTES.md` + `build.ps1`** — single markdown source of truth for `VersionPrefix` + `PackageReleaseNotes`, injected into the pack csproj on every build.
- [x] **`scripts/install-dev-templates.ps1`** — one-command local dev loop.
- [x] **`scripts/test-templates.ps1`** — smoke-tests 11 parameter combinations end-to-end (`dotnet new` → build → test → pack). Supports `-Install`, `-Clean`, `-BowireSdkVersion <pin>`.
- [x] **`pr_validation.yml`** — PR/push CI runs the smoke-test script against the packed template.
- [x] **`publish_nuget.yml`** — `v*` tag packs, pushes to nuget.org, extracts the latest `RELEASE_NOTES.md` entry, opens a GitHub Release.
- [x] **`dependabot.yml`** — weekly NuGet + GitHub Actions updates.
- [x] **Root `nuget.config`** — adds a local `C:\Projekte\KL\Bowire\artifacts\packages` feed so dev + CI can consume pre-release Bowire builds.
- [x] **`.gitattributes`** — pins LF everywhere, CRLF for `*.ps1` / `*.bat`. Stops the "LF will be replaced by CRLF" warning flood on every commit.
- [x] **`.editorconfig`** in the generated template — matches the already-enabled `TreatWarningsAsErrors` + `EnforceCodeStyleInBuild` policy.

### Community onboarding

- [x] **`CONTRIBUTING.md`** — walks new contributors through the dev loop, repo layout, how to add a parameter/preset/smoke case, release flow.
- [x] **`.github/ISSUE_TEMPLATE/{bug_report,feature_request,config}.md`** — structured bug/feature issue templates, Bowire-runtime issues redirected to the Bowire repo.
- [x] **`.github/PULL_REQUEST_TEMPLATE.md`** — PR checklist covering smoke run, RELEASE_NOTES, ROADMAP, docs.

### Docs

- [x] **`docs/BowirePluginTemplate.md`** — per-template reference with parameter table, preset table, scaffold layout, and "Adding to an existing monorepo" recipe.
- [x] **Bowire main-repo `docs/protocols/custom.md`** — now leads with `dotnet new bowire-plugin` (template-first) and keeps the manual-setup path as an alternative. Sample code was also corrected to match the real `BowireServiceInfo` / `BowireMethodInfo` / `InvokeResult` signatures.

## v1.1.0 — Extension-point templates (2026-05-25)

Bowire 1.6.0 documents four extension contracts (`IBowireProtocol`, `IBowireCliCommand`, `IBowireMockEmitter`, `IBowireUiExtension`). The original template covered only the first; this release adds scaffolders for two more so plugin authors can wire in CLI verbs and recording-replay emitters without copy-pasting from the first-party plugins.

- [x] **`bowire-cli-cmd` item template** — single-file `IBowireCliCommand` scaffold (`MyCommand.cs`) with `--Verb` parameter that replaces the `MY_VERB` token throughout. Pattern matches the first-party `ScanCliCommand`.
- [x] **`bowire-mock-emit` item template** — single-file `IBowireMockEmitter` scaffold (`MyMockEmitter.cs`) with `--EmitterId` parameter. Includes the shared payload-decoding convention (`ResponseBinary` first, `Body` fallback), `ReplaySpeed` timing-multiplier handling, and `Loop` outer loop. Matches the in-tree Kafka / AMQP / TacticalAPI emitter shape.
- [x] **README + ROADMAP** updated for the two new templates; quickstart shows all three.

## Maintenance

- [x] **2026-05-25** — bumped `--BowireSdkVersion` default in `template.json` from `1.3.*` to `1.6.0` (tracking the current `Kuestenlogik.Bowire` release line + compatibility-matrix floor).
- [x] **2026-05-13** — bumped `--BowireSdkVersion` default in `template.json` from `0.9.*` to `1.3.*` (tracking current Bowire 1.3.x release line); updated the matching comment in `Directory.Packages.props` and the parameter table in `docs/BowirePluginTemplate.md`.

## Blocked on external

- [ ] **Publish `v0.1.0` to nuget.org** — waiting on `Kuestenlogik.Bowire` being published so `dotnet restore` in the generated plugin actually resolves the SDK reference.
- [ ] **Validate the CI workflow against cloud GitHub runners** — `pr_validation.yml` currently assumes a local Bowire feed at `C:\Projekte\KL\Bowire\artifacts\packages`. Once `Kuestenlogik.Bowire` is on nuget.org the CI runs without that detour.

## Planned / nice-to-have

- [ ] **Bowire-SDK-version auto-bump** — cron-driven GitHub Actions workflow that checks nuget.org for a newer `Kuestenlogik.Bowire` version and opens a PR bumping the `--BowireSdkVersion` default in `template.json` + the matching `PackageVersion` entries.
- [ ] **ID-prefix reservation for `KL.*`** — request from nuget.org once the first package in the namespace is published.
- [ ] **Author-signing** — optional; only if a Sectigo / DigiCert cert or Azure Trusted Signing subscription is available. Repository-signature via nuget.org covers authenticity in the meantime.
- [ ] **Broader Bowire auto-discovery** (upstream, not in this repo) — Bowire currently only scans assemblies whose name contains `"Bowire"`. A PR against the main Bowire repo to also accept an `[assembly: BowirePluginAssembly]` attribute (or similar marker) would let plugin authors pick any package name without losing auto-registration. Documented in the integration-tests scaffold as a known sharp edge until then.

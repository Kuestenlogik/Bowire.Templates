#### 1.1.0 May 25th 2026 ####

* New item template `bowire-cli-cmd` — scaffolds an `IBowireCliCommand` implementation that contributes a `bowire <verb>` subcommand. Drop into any plugin project that references `Kuestenlogik.Bowire.Cli`.
* New item template `bowire-mock-emit` — scaffolds an `IBowireMockEmitter` implementation that replays `BowireRecording` steps over a wire protocol. Matches the in-tree Kafka / AMQP / TacticalAPI emitter shape (binary-first payload precedence, `ReplaySpeed` + `Loop` honoured).
* `bowire-plugin` template's generated `Directory.Packages.props` floor bumped to Bowire 1.6.0 (was 1.5.0). Matches the current compatibility-matrix baseline.

#### 1.0.1 May 3rd 2026 ####

* Initial release.
* Scaffolds a Bowire protocol plugin with `IBowireProtocol` stub, `.slnx` solution, xunit tests, and optional GitHub Actions CI.
* `--IncludeDuplexChannel` flag scaffolds a full `IBowireChannel` echo demo for bidirectional/duplex protocols.
* `--Preset none|rest|mqtt|websocket|grpc|signalr` pre-fills `DiscoverAsync`/`InvokeAsync` with transport-specific starter code (HTTP+OpenAPI probe; MQTTnet connect+publish; ClientWebSocket roundtrip; GrpcChannel connect; SignalR HubConnection invoke).
* `--IncludeIntegrationTests` scaffolds a second test project that hosts the plugin inside an ASP.NET Core `TestServer` and exercises the real Bowire HTTP API.
* `--ProtocolId` is lowercased and stripped to `[a-z0-9_-]` automatically.
* `--IconSvg` parameter embeds custom SVG markup in the generated `IconSvg` property (default keeps the current circle demo).
* `--Minimal` shortcut combines `ProjectOnly=true + IncludeCI=false + IncludeIntegrationTests=false` for the smallest possible output.

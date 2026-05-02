#### 1.0.1 May 3rd 2026 ####

* Initial release.
* Scaffolds a Bowire protocol plugin with `IBowireProtocol` stub, `.slnx` solution, xunit tests, and optional GitHub Actions CI.
* `--IncludeDuplexChannel` flag scaffolds a full `IBowireChannel` echo demo for bidirectional/duplex protocols.
* `--Preset none|rest|mqtt|websocket|grpc|signalr` pre-fills `DiscoverAsync`/`InvokeAsync` with transport-specific starter code (HTTP+OpenAPI probe; MQTTnet connect+publish; ClientWebSocket roundtrip; GrpcChannel connect; SignalR HubConnection invoke).
* `--IncludeIntegrationTests` scaffolds a second test project that hosts the plugin inside an ASP.NET Core `TestServer` and exercises the real Bowire HTTP API.
* `--ProtocolId` is lowercased and stripped to `[a-z0-9_-]` automatically.
* `--IconSvg` parameter embeds custom SVG markup in the generated `IconSvg` property (default keeps the current circle demo).
* `--Minimal` shortcut combines `ProjectOnly=true + IncludeCI=false + IncludeIntegrationTests=false` for the smallest possible output.

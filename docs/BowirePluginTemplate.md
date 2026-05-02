# Bowire Plugin Template (`dotnet new bowire-plugin`)

Scaffolds a working [Bowire](https://github.com/Kuestenlogik/Bowire) protocol plugin project: a `PackageType=BowirePlugin` NuGet package that implements `IBowireProtocol` and is auto-discovered by Bowire via assembly scanning.

## Quickstart

```bash
dotnet new install Kuestenlogik.Bowire.Templates
dotnet new bowire-plugin -n Contoso.Bowire.Protocol.Foo
cd Contoso.Bowire.Protocol.Foo
dotnet test
```

## What you get

```
Contoso.Bowire.Protocol.Foo/
├── .github/workflows/ci.yml                             (when --IncludeCI true)
├── Directory.Build.props
├── Directory.Packages.props
├── Contoso.Bowire.Protocol.Foo.slnx
├── src/Contoso.Bowire.Protocol.Foo/
│   ├── Contoso.Bowire.Protocol.Foo.csproj              PackageType=BowirePlugin
│   ├── MyProtocol.cs                                    implements IBowireProtocol
│   └── MyProtocolChannel.cs                             (when --IncludeDuplexChannel true)
└── tests/Contoso.Bowire.Protocol.Foo.Tests/
    ├── Contoso.Bowire.Protocol.Foo.Tests.csproj
    └── MyProtocolTests.cs                               xunit smoke tests
```

`dotnet build` / `dotnet test` / `dotnet pack` all work immediately without editing anything.

## Parameters

| Parameter                | Default            | Description                                                                 |
|--------------------------|--------------------|-----------------------------------------------------------------------------|
| `-n, --name`             | `Bowire.Plugin1`  | Project name. Drives namespace, `PackageId`, folder names, `.slnx` paths.   |
| `--DisplayName`          | `My Protocol`      | Human-readable name shown on the Bowire protocol tab (e.g. `"gRPC"`, `"MQTT"`). |
| `--ProtocolId`           | `myproto`          | Short identifier used by Bowire internally. Lowercased and stripped to `[a-z0-9_-]` automatically (so `"My Proto!"` → `"myproto"`). |
| `--PluginClassName`      | `MyProtocol`       | C# class name that implements `IBowireProtocol`.                           |
| `--Author`               | `""`               | NuGet package `Authors` field.                                              |
| `--Company`              | `""`               | NuGet package `Company` field.                                              |
| `--Preset`               | `none`             | Seed `DiscoverAsync`/`InvokeAsync` with a realistic starting point for a specific transport. One of `none` / `rest` / `mqtt` / `websocket` / `grpc` / `signalr`. See [Presets](#presets). |
| `--IconSvg`              | demo circle        | Raw SVG markup embedded in the generated `IconSvg` property (what Bowire shows on the protocol tab). Pass your own `<svg>...</svg>` string. |
| `--Minimal`              | `false`            | Shortcut: implies `--ProjectOnly true --IncludeCI false --IncludeIntegrationTests false`. Produces the smallest possible plugin output (plugin csproj + unit-test csproj, no solution, no build-props, no CI). |
| `--BowireSdkVersion`    | `0.1.*`            | Version range of the `Kuestenlogik.Bowire` NuGet package (the Bowire SDK) this plugin references. |
| `--IncludeCI`            | `true`             | Include a GitHub Actions workflow that builds, tests, and packs the plugin. |
| `--IncludeDuplexChannel` | `false`            | Scaffold a full `IBowireChannel` echo demo for bidirectional / duplex protocols. |
| `--IncludeIntegrationTests` | `false`         | Scaffold a second test project (`<Name>.IntegrationTests`) that hosts the plugin in an ASP.NET Core `TestServer` with `AddBowire()` / `MapBowire()` and hits `/bowire/api/protocols` + `/bowire/api/services` over HTTP. Requires the plugin project name to contain `Bowire` (e.g. `Contoso.Bowire.Protocol.Foo`) — Bowire's auto-discovery only scans assemblies whose name contains that substring. |
| `--ProjectOnly`          | `false`            | Emit only `src/` and `tests/` — skip `.slnx`, `Directory.Build.props`, `Directory.Packages.props`, `.gitignore`, `README.md` and `.github/`. See [Adding to an existing monorepo](#adding-to-an-existing-monorepo). |

## What the scaffold does out of the box

- **`DiscoverAsync`** — returns a single `DemoService.Echo` method so your plugin shows up in the Bowire UI on the first run.
- **`InvokeAsync`** — echoes the first JSON message back with `Status="OK"`, `DurationMs=0`. Replace with a real invocation.
- **`InvokeStreamAsync`** — yields three demo messages with 100 ms spacing so the streaming pane shows activity.
- **`OpenChannelAsync`** — returns `null` by default, or an echo `MyProtocolChannel` instance when `--IncludeDuplexChannel true`.

The xunit test project exercises every method so `dotnet test` proves the skeleton is wired up correctly before you touch anything.

## Presets

`--Preset <value>` pre-fills `DiscoverAsync` and `InvokeAsync` with code for a specific transport so you don't start from a blank `HttpClient` or `IMqttClient`. The defaults are wire-level sensible; you still need to fill in the schema parsing and error handling.

| Preset       | What the scaffold does                                                                                             | Extra NuGet refs             |
|--------------|--------------------------------------------------------------------------------------------------------------------|------------------------------|
| `none`       | Generic echo demo — `DiscoverAsync` returns a `DemoService.Echo` method, `InvokeAsync` echoes the first payload.   | —                            |
| `rest`       | Probes `serverUrl/openapi/v1.json` with a static `HttpClient`; `InvokeAsync` POSTs JSON and wraps the response.    | — (uses `System.Net.Http`)   |
| `mqtt`       | `InvokeAsync` connects via `MqttClientFactory`, publishes to `service/method`, disconnects.                        | `MQTTnet`                    |
| `websocket`  | `InvokeAsync` opens a transient `ClientWebSocket`, sends a text frame, reads one response frame, closes.           | — (uses `System.Net.WebSockets`) |
| `grpc`       | `InvokeAsync` opens a `GrpcChannel.ForAddress(serverUrl)` and calls `ConnectAsync`. Real invokes need a generated client or a Server Reflection path — the scaffold proves connectivity only. | `Grpc.Net.Client` |
| `signalr`    | `InvokeAsync` builds a `HubConnection`, starts it, invokes the hub method with the payload as argument, disposes.  | `Microsoft.AspNetCore.SignalR.Client` |

Pair `--Preset mqtt` or `--Preset websocket` with `--IncludeDuplexChannel true` for protocols that need interactive duplex — the preset fills `InvokeAsync` for unary calls, the duplex channel handles subscriptions / long-lived connections. For stateless HTTP-style protocols, `--Preset rest` without the duplex channel is usually enough.

## Adding to an existing monorepo

If you already have a repo with its own `.slnx`, `Directory.Build.props`, `Directory.Packages.props` and CI, drop `--ProjectOnly true` and point `-o` at the folder where the new plugin should live:

```bash
dotnet new bowire-plugin \
    -n Contoso.Bowire.Protocol.Foo \
    -o src/Contoso.Bowire.Protocol.Foo \
    --ProjectOnly true
```

You get just the two csproj files plus their sources:

```
src/Contoso.Bowire.Protocol.Foo/
├── src/Contoso.Bowire.Protocol.Foo/
│   ├── Contoso.Bowire.Protocol.Foo.csproj
│   └── MyProtocol.cs
└── tests/Contoso.Bowire.Protocol.Foo.Tests/
    ├── Contoso.Bowire.Protocol.Foo.Tests.csproj
    └── MyProtocolTests.cs
```

In this mode the generated `.csproj` files are self-contained: they set `TargetFramework`/`LangVersion`/`Nullable` inline and pin `Kuestenlogik.Bowire` + xunit package versions directly, so they build even if the parent repo has no `Directory.Build.props` or `Directory.Packages.props` at all. If the parent repo does provide them, the inline values simply override / get overridden by MSBuild's usual precedence rules.

Optionally add the two new projects to your existing `.slnx` by hand.

## Publishing your plugin

```bash
dotnet pack -c Release
dotnet nuget push artifacts/packages/Contoso.Bowire.Protocol.Foo.*.nupkg \
    --source https://api.nuget.org/v3/index.json \
    --api-key $NUGET_API_KEY
```

Users install your plugin via `bowire plugin install Contoso.Bowire.Protocol.Foo` or reference the package directly in an embedded host.

### Make it discoverable

- **NuGet `<PackageTags>`** in your `.csproj`: include `bowire` and `bowire-plugin` so your package shows up in Bowire's in-app marketplace and on nuget.org's tag pages.

  ```xml
  <PackageTags>bowire bowire-plugin foo-protocol</PackageTags>
  ```

- **GitHub repo topics**: tag your source repo with `bowire-plugin` (plus `bowire` and `dotnet`). The official protocol plugins under `Kuestenlogik/Bowire.Protocol.*` use the same convention, so a [topic search for `bowire-plugin`](https://github.com/topics/bowire-plugin) lists all of them alongside yours.

See the [custom-protocol guide](https://kuestenlogik.github.io/Bowire/docs/protocols/custom.html) in the Bowire docs for the full `IBowireProtocol` contract.

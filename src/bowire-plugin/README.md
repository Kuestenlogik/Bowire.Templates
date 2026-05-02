# Bowire.Plugin1

A [Bowire](https://github.com/Kuestenlogik/Bowire) protocol plugin for **MY_PROTOCOL_DISPLAY_NAME**.

This project was scaffolded with `dotnet new bowire-plugin`. It implements
`IBowireProtocol` and ships as a NuGet package of type `BowirePlugin` so
Bowire discovers it via the in-app marketplace and `bowire plugin install`.

## Layout

```
Bowire.Plugin1/
├── src/Bowire.Plugin1/            # the plugin itself
│   ├── Bowire.Plugin1.csproj      # PackageType=BowirePlugin
│   └── MyProtocol.cs               # implements IBowireProtocol
└── tests/Bowire.Plugin1.Tests/    # xunit smoke tests
```

## Build & test

```bash
dotnet restore
dotnet build
dotnet test
```

## Pack

```bash
dotnet pack -c Release
```

The resulting `.nupkg` lands in `artifacts/packages/` and can be pushed to
nuget.org or any private feed:

```bash
dotnet nuget push artifacts/packages/Bowire.Plugin1.*.nupkg \
    --source https://api.nuget.org/v3/index.json \
    --api-key $NUGET_API_KEY
```

## Try it in Bowire

Once published, users install your plugin with:

```bash
bowire plugin install Bowire.Plugin1
```

Or, during development, reference the local package directly:

```bash
dotnet add package Bowire.Plugin1 --source ./artifacts/packages
```

## Implement your protocol

Open `src/Bowire.Plugin1/MyProtocol.cs` and replace the demo bodies:

- `DiscoverAsync` — return real services/methods from your schema source.
- `InvokeAsync` — call the target and map the response to `InvokeResult`.
- `InvokeStreamAsync` — yield JSON messages for server-streaming calls.
- `OpenChannelAsync` — return an `IBowireChannel` if you support duplex.
  If you scaffolded with `--IncludeDuplexChannel true`, `MyProtocolChannel.cs`
  already wires up a working echo channel you can replace with a real
  transport (WebSocket, MQTT, SignalR, ...).

See the [custom-protocol guide](https://kuestenlogik.github.io/Bowire/docs/protocols/custom.html)
for the full interface contract.

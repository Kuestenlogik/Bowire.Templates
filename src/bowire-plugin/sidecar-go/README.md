# Bowire.Plugin1

A Bowire **sidecar plugin** written in Go — `MY_PROTOCOL_DISPLAY_NAME`. Ships as a zip the Bowire host spawns over JSON-RPC, no .NET required.

## Local dev

```bash
go mod tidy
go build -o MY_PROTOCOL_ID

# stdio mode — what the Bowire host spawns:
./MY_PROTOCOL_ID
```

## Test

```bash
go test ./...
```

## Ship to Bowire

Package the manifest + the compiled binary into a zip and install it:

```bash
go build -o MY_PROTOCOL_ID
zip Bowire.Plugin1.zip sidecar.json MY_PROTOCOL_ID

bowire plugin install --file Bowire.Plugin1.zip
# Or, from an OCI registry:
# bowire plugin install --file oci://ghcr.io/your-org/MY_PROTOCOL_ID:0.1.0
```

The Bowire host extracts the zip into `~/.bowire/plugins/Bowire.Plugin1/`, reads `sidecar.json`, spawns `./MY_PROTOCOL_ID` as the sidecar, and starts brokering JSON-RPC requests against your `myProtocol` instance.

## Wire contract

The Bowire.Sdk.Go SDK speaks the [Bowire sidecar JSON-RPC contract](https://bowire.io/docs/architecture/sidecar-plugins.html) — `initialize` / `ping` / `shutdown` / `discover` / `invoke` / `invokeStream` / `openChannel` with NDJSON framing over stdio, or POST + long-lived SSE over HTTP. You implement the protocol-side semantics in `main.go`; the SDK handles the wire.

## Switching from stdio to HTTP

For long-running multi-tenant deployments where Bowire doesn't own the process, flip `sidecar.json` to:

```jsonc
{
  "transport": "http",
  "url": "http://127.0.0.1:8770"
}
```

…and replace `plugin.Run(ctx, myProtocol{})` in `main.go` with `plugin.RunHTTP(ctx, myProtocol{}, "127.0.0.1", 8770)`. Same plugin code, same SDK, different wire.

## What `myProtocol` implements

| Method        | When called | Status       |
|---------------|-------------|--------------|
| `Discover`    | Bowire sidebar | **Implemented** — stub `DemoService.Echo` |
| `Invoke`      | "Invoke" button on a Unary method | **Implemented** — parrots the request back |
| Streaming    | "Invoke" button on a ServerStreaming method | Implement `plugin.StreamingPlugin` interface |
| Duplex channels | Duplex methods (WebSocket-style) | Implement `plugin.ChannelPlugin` interface |
| Settings     | The plugin's settings dialog in the workbench | Implement `plugin.SettingsPlugin` interface |

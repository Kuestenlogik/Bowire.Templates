# Bowire.Plugin1

A Bowire **sidecar plugin** written in Rust — `MY_PROTOCOL_DISPLAY_NAME`. Ships as a zip the Bowire host spawns over JSON-RPC, no .NET required.

## Local dev

```bash
cargo build --release

# stdio mode — what the Bowire host spawns:
./target/release/MY_RUST_CRATE_NAME
```

## Test

```bash
cargo test
```

## Ship to Bowire

Package the manifest + the release binary into a zip and install it:

```bash
cargo build --release
mkdir -p dist
cp target/release/MY_RUST_CRATE_NAME dist/MY_RUST_CRATE_NAME
cp sidecar.json dist/sidecar.json
cd dist && zip -r ../Bowire.Plugin1.zip . && cd ..

bowire plugin install --file Bowire.Plugin1.zip
# Or, from an OCI registry:
# bowire plugin install --file oci://ghcr.io/your-org/MY_PROTOCOL_ID:0.1.0
```

The Bowire host extracts the zip into `~/.bowire/plugins/Bowire.Plugin1/`, reads `sidecar.json`, spawns `./MY_RUST_CRATE_NAME` as the sidecar, and starts brokering JSON-RPC requests against your `MyProtocol` instance.

## Wire contract

The `bowire-plugin` SDK speaks the [Bowire sidecar JSON-RPC contract](https://bowire.io/docs/architecture/sidecar-plugins.html) — `initialize` / `ping` / `shutdown` / `discover` / `invoke` / `invokeStream` / `openChannel` with NDJSON framing over stdio, or POST + long-lived SSE over HTTP (opt in via `features = ["http"]`). You implement the protocol-side semantics in `src/main.rs`; the SDK handles the wire.

## Switching from stdio to HTTP

For long-running multi-tenant deployments where Bowire doesn't own the process, flip `sidecar.json` to:

```jsonc
{
  "transport": "http",
  "url": "http://127.0.0.1:8770"
}
```

…enable `features = ["http"]` on the `bowire-plugin` dependency in `Cargo.toml`, and replace `run(MyProtocol)` in `src/main.rs` with `run_http(MyProtocol, "127.0.0.1", 8770)`. Same plugin code, same SDK, different wire.

## What `MyProtocol` implements

| Method        | When called | Status       |
|---------------|-------------|--------------|
| `discover`    | Bowire sidebar | **Implemented** — stub `DemoService.Echo` |
| `invoke`      | "Invoke" button on a Unary method | **Implemented** — parrots the request back |
| `invoke_stream` | "Invoke" button on a ServerStreaming method | Override when you add streaming methods |
| `open_channel` | Duplex methods (WebSocket-style) | Override when you add duplex methods |
| `settings`    | The plugin's settings dialog in the workbench | Optional |

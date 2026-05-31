# Bowire.Plugin1

A Bowire **sidecar plugin** written in Node.js / TypeScript — `MY_PROTOCOL_DISPLAY_NAME`. Ships as a zip the Bowire host spawns over JSON-RPC, no .NET required.

## Local dev

```bash
npm install
npm run build

# stdio mode — what the Bowire host spawns:
node dist/index.js

# HTTP/SSE mode (optional — needs sidecar.json's transport: "http"):
# swap `run` for `runHttp` in src/index.ts and set BOWIRE_HTTP_PORT.
```

## Test

```bash
npm test
```

## Ship to Bowire

Package the manifest + built bundle into a zip and install it:

```bash
npm run build
zip -r Bowire.Plugin1.zip sidecar.json dist/ node_modules/ package.json

bowire plugin install --file Bowire.Plugin1.zip
# Or, from an OCI registry:
# bowire plugin install --file oci://ghcr.io/your-org/MY_PROTOCOL_ID:0.1.0
```

The Bowire host extracts the zip into `~/.bowire/plugins/Bowire.Plugin1/`, reads `sidecar.json`, spawns `node dist/index.js` as the sidecar, and starts brokering JSON-RPC requests against your `MyProtocol` instance.

## Wire contract

The `@bowire/plugin` SDK speaks the [Bowire sidecar JSON-RPC contract](https://bowire.io/docs/architecture/sidecar-plugins.html) — `initialize` / `ping` / `shutdown` / `discover` / `invoke` / `invokeStream` / `openChannel` with NDJSON framing over stdio, or POST + long-lived SSE over HTTP. You implement the protocol-side semantics in `src/plugin.ts`; the SDK handles the wire.

## Switching from stdio to HTTP

For long-running multi-tenant deployments where Bowire doesn't own the process, flip `sidecar.json` to:

```jsonc
{
  "transport": "http",
  "url": "http://127.0.0.1:8770"
}
```

…and replace `run(new MyProtocol())` in `src/index.ts` with `runHttp(new MyProtocol(), "127.0.0.1", 8770)`. Same plugin code, same SDK, different wire.

## What `MyProtocol` implements

| Method        | When called | Status       |
|---------------|-------------|--------------|
| `discover`    | Bowire sidebar | **Implemented** — stub `DemoService.Echo` |
| `invoke`      | "Invoke" button on a Unary method | **Implemented** — parrots the request back |
| `invokeStream` | "Invoke" button on a ServerStreaming method | Override when you add streaming methods |
| `openChannel` | Duplex methods (WebSocket-style) | Override when you add duplex methods |
| `settings`    | The plugin's settings dialog in the workbench | Optional |

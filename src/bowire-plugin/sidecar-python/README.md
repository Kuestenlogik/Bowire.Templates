# Bowire.Plugin1

A Bowire **sidecar plugin** written in Python — `MY_PROTOCOL_DISPLAY_NAME`. Ships as a zip the Bowire host spawns over JSON-RPC, no .NET required.

## Local dev

```bash
pip install -e .[dev]

# stdio mode — what the Bowire host spawns:
python -m MY_PYTHON_PACKAGE_NAME

# HTTP/SSE mode (optional — needs sidecar.json's transport: "http"):
python -c "from bowire_plugin import run_http; from MY_PYTHON_PACKAGE_NAME import MyProtocol; run_http(MyProtocol(), host='127.0.0.1', port=8770)"
```

## Test

```bash
pytest
```

## Ship to Bowire

Package the manifest + the installed wheel into a zip and install it:

```bash
pip wheel . -w dist/
zip -j Bowire.Plugin1.zip sidecar.json dist/*.whl

bowire plugin install --file Bowire.Plugin1.zip
# Or, from an OCI registry:
# bowire plugin install --file oci://ghcr.io/your-org/MY_PROTOCOL_ID:0.1.0
```

The Bowire host extracts the zip into `~/.bowire/plugins/Bowire.Plugin1/`, reads `sidecar.json`, spawns `python -m MY_PYTHON_PACKAGE_NAME` as the sidecar, and starts brokering JSON-RPC requests against your `MyProtocol` instance.

## Wire contract

The `bowire-plugin` SDK speaks the [Bowire sidecar JSON-RPC contract](https://bowire.io/docs/architecture/sidecar-plugins.html) — `initialize` / `ping` / `shutdown` / `discover` / `invoke` / `invokeStream` / `openChannel` with NDJSON framing over stdio, or POST + long-lived SSE over HTTP. You implement the protocol-side semantics in `plugin.py`; the SDK handles the wire.

## Switching from stdio to HTTP

For long-running multi-tenant deployments where Bowire doesn't own the process, flip `sidecar.json` to:

```jsonc
{
  "transport": "http",
  "url": "http://127.0.0.1:8770"
}
```

…and replace `run(MyProtocol())` in `__main__.py` with `run_http(MyProtocol(), host='127.0.0.1', port=8770)`. Same plugin code, same SDK, different wire.

## What `MyProtocol` implements

| Method        | When called | Status       |
|---------------|-------------|--------------|
| `discover`    | Bowire sidebar | **Implemented** — stub `DemoService.Echo` |
| `invoke`      | "Invoke" button on a Unary method | **Implemented** — parrots the request back |
| `invoke_stream` | "Invoke" button on a ServerStreaming method | Override in `plugin.py` when you add streaming methods |
| `open_channel` | Duplex methods (WebSocket-style) | Override in `plugin.py` when you add duplex methods |
| `settings`    | The plugin's settings dialog in the workbench | Optional — declare config knobs the SDK surfaces as form fields |

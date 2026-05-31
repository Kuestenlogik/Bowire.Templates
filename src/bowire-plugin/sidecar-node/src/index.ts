// Entry point — `node dist/index.js` boots the plugin over stdio.
//
// The Bowire host spawns this as a subprocess (see sidecar.json);
// we hand the plugin instance to `run`, which speaks JSON-RPC 2.0
// over NDJSON on stdin/stdout. For HTTP/SSE-mode sidecars swap
// `run` for `runHttp(plugin, host, port)` and flip `transport` to
// `"http"` in sidecar.json.

import { run } from "@bowire/plugin";
import { MyProtocol } from "./plugin.js";

await run(new MyProtocol());

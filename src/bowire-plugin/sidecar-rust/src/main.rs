//! Entry point — `./MY_RUST_CRATE_NAME` boots the plugin over stdio.
//!
//! The Bowire host spawns this binary as a subprocess (see
//! sidecar.json); we hand the plugin instance to [`bowire_plugin::run`],
//! which speaks JSON-RPC 2.0 over NDJSON on stdin/stdout. For
//! HTTP/SSE-mode sidecars swap `run` for `run_http(plugin, host, port)`
//! and flip `transport` to `"http"` in sidecar.json.

use std::collections::HashMap;

use async_trait::async_trait;
use bowire_plugin::{
    run, BowirePlugin, FieldInfo, InvokeResult, MessageInfo, MethodInfo, ServiceInfo,
};

struct MyProtocol;

#[async_trait]
impl BowirePlugin for MyProtocol {
    fn id(&self) -> &str {
        "MY_PROTOCOL_ID"
    }

    fn name(&self) -> &str {
        "MY_PROTOCOL_DISPLAY_NAME"
    }

    /// Return the topology Bowire renders in the sidebar. In a real
    /// plugin this would parse a schema, scan a broker, call a
    /// server's reflection endpoint, &c. The stub below exposes one
    /// `DemoService.Echo` method so the workbench has something to
    /// click as soon as you install the sidecar.
    async fn discover(&self, _endpoint: &str, _show_internal: bool) -> Vec<ServiceInfo> {
        vec![ServiceInfo::new("DemoService").with_methods([MethodInfo::unary("Echo")
            .with_input(
                MessageInfo::new("EchoRequest", "MY_PROTOCOL_ID.EchoRequest").with_fields([
                    FieldInfo::string("message")
                        .required()
                        .with_description("Anything you want echoed back."),
                ]),
            )
            .with_output(
                MessageInfo::new("EchoReply", "MY_PROTOCOL_ID.EchoReply")
                    .with_fields([FieldInfo::string("echoed")]),
            )])]
    }

    /// Dispatch a unary call. `body` is the request list (one entry
    /// for unary methods, multiple for client-streaming). Replace the
    /// stand-in echo below with your wire call.
    async fn invoke(
        &self,
        _endpoint: &str,
        _service: &str,
        _method: &str,
        body: Vec<String>,
        _streaming: bool,
        _metadata: HashMap<String, String>,
    ) -> InvokeResult {
        let payload = body
            .into_iter()
            .next()
            .unwrap_or_else(|| "{}".to_string());
        InvokeResult::ok(format!(r#"{{"echoed":{payload}}}"#))
    }
}

#[tokio::main]
async fn main() -> std::io::Result<()> {
    run(MyProtocol).await
}

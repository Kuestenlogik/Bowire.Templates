// MY_PROTOCOL_DISPLAY_NAME — Bowire sidecar plugin (Go).
//
// Entry point — `./MY_PROTOCOL_ID` boots the plugin over stdio.
//
// The Bowire host spawns this binary as a subprocess (see
// sidecar.json); we hand the plugin instance to plugin.Run, which
// speaks JSON-RPC 2.0 over NDJSON on stdin/stdout. For HTTP/SSE-mode
// sidecars swap Run for RunHTTP(ctx, plugin, host, port) and flip
// transport to "http" in sidecar.json.
package main

import (
	"context"
	"log"

	"github.com/Kuestenlogik/Bowire.Sdk.Go/plugin"
)

type myProtocol struct{}

func (myProtocol) ID() string   { return "MY_PROTOCOL_ID" }
func (myProtocol) Name() string { return "MY_PROTOCOL_DISPLAY_NAME" }

// Return the topology Bowire renders in the sidebar. In a real
// plugin this would parse a schema, scan a broker, call a server's
// reflection endpoint, &c. The stub below exposes one
// `DemoService.Echo` method so the workbench has something to click
// as soon as you install the sidecar.
func (myProtocol) Discover(_ context.Context, _ string, _ bool) ([]plugin.ServiceInfo, error) {
	return []plugin.ServiceInfo{
		plugin.NewServiceInfo("DemoService").WithMethods(
			plugin.UnaryMethod("Echo").
				WithInput(plugin.NewMessageInfo("EchoRequest", "MY_PROTOCOL_ID.EchoRequest").WithFields(
					plugin.String("message").MarkRequired().WithDescription("Anything you want echoed back."),
				)).
				WithOutput(plugin.NewMessageInfo("EchoReply", "MY_PROTOCOL_ID.EchoReply").WithFields(
					plugin.String("echoed"),
				)),
		),
	}, nil
}

// Dispatch a unary call. `req.Body` is the request list (one entry
// for unary methods, multiple for client-streaming). Replace the
// stand-in echo below with your wire call.
func (myProtocol) Invoke(_ context.Context, req plugin.InvokeRequest) (plugin.InvokeResult, error) {
	payload := "{}"
	if len(req.Body) > 0 {
		payload = req.Body[0]
	}
	return plugin.NewOKResult(`{"echoed":` + payload + `}`), nil
}

func main() {
	if err := plugin.Run(context.Background(), myProtocol{}); err != nil {
		log.Fatal(err)
	}
}

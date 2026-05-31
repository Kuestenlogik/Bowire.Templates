// MyProtocol — your sidecar's BowirePluginBase subclass.
//
// Override the methods you need (discover / invoke / invokeStream /
// openChannel / settings); the SDK's `run` (stdio) / `runHttp` drive
// the JSON-RPC wire for you.

import {
  BowirePluginBase,
  FieldInfo,
  InvokeResult,
  MessageInfo,
  MethodInfo,
  ServiceInfo,
  type Metadata,
} from "@bowire/plugin";

export class MyProtocol extends BowirePluginBase {
  override id(): string {
    return "MY_PROTOCOL_ID";
  }
  override name(): string {
    return "MY_PROTOCOL_DISPLAY_NAME";
  }

  // Return the topology Bowire renders in the sidebar. In a real
  // plugin this would parse a schema, scan a broker, call a server's
  // reflection endpoint, &c. The stub below exposes one
  // `DemoService.Echo` method so the workbench has something to
  // click as soon as you install the sidecar.
  override discover(): ServiceInfo[] {
    return [
      new ServiceInfo("DemoService").withMethods([
        MethodInfo.unary("Echo")
          .withInput(
            new MessageInfo("EchoRequest", "MY_PROTOCOL_ID.EchoRequest").withFields([
              FieldInfo.string("message")
                .makeRequired()
                .withDescription("Anything you want echoed back."),
            ]),
          )
          .withOutput(
            new MessageInfo("EchoReply", "MY_PROTOCOL_ID.EchoReply").withFields([
              FieldInfo.string("echoed"),
            ]),
          ),
      ]),
    ];
  }

  // Dispatch a unary call. `body` is the request list (one entry for
  // unary methods, multiple for client-streaming). Replace the
  // stand-in echo below with your wire call.
  override invoke(
    _endpoint: string,
    _service: string,
    _method: string,
    body: string[],
    _streaming: boolean,
    _metadata: Metadata,
  ): InvokeResult {
    const payload = body[0] ?? "{}";
    return InvokeResult.ok(`{"echoed":${payload}}`);
  }
}

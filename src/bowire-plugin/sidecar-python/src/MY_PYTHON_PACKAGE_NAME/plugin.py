"""MyProtocol — your sidecar's :class:`BowirePlugin` subclass.

Override the methods you need (``discover`` / ``invoke`` /
``invoke_stream`` / ``open_channel`` / ``settings``); the SDK's
:func:`bowire_plugin.run` drives the JSON-RPC wire for you.
"""

from bowire_plugin import (
    BowirePlugin,
    FieldInfo,
    InvokeResult,
    MessageInfo,
    MethodInfo,
    ServiceInfo,
)


class MyProtocol(BowirePlugin):
    """Sample sidecar plugin — replace the stub topology with your wire calls."""

    id = "MY_PROTOCOL_ID"
    name = "MY_PROTOCOL_DISPLAY_NAME"

    def discover(self, server_url: str, show_internal: bool):
        """Return the topology Bowire renders in the sidebar.

        In a real plugin this would parse a schema, scan a broker,
        call a server's reflection endpoint, &c. The stub below
        exposes one ``DemoService.Echo(input)`` method so the
        workbench has something to click as soon as you install the
        sidecar.
        """
        echo_input = MessageInfo(
            name="EchoRequest",
            full_name="MY_PROTOCOL_ID.EchoRequest",
            fields=[
                FieldInfo(name="message", type="string", description="Anything you want echoed back."),
            ],
        )
        echo_output = MessageInfo(
            name="EchoReply",
            full_name="MY_PROTOCOL_ID.EchoReply",
            fields=[
                FieldInfo(name="echoed", type="string"),
            ],
        )
        return [
            ServiceInfo(
                name="DemoService",
                methods=[
                    MethodInfo(
                        name="Echo",
                        method_type="Unary",
                        input_type=echo_input,
                        output_type=echo_output,
                    ),
                ],
            )
        ]

    def invoke(self, server_url, service, method, json_messages,
               show_internal, metadata):
        """Dispatch a unary call. ``json_messages`` is the request list
        (one entry for unary methods, multiple for client-streaming)."""
        payload = json_messages[0] if json_messages else "{}"
        # Stand-in echo: parrots the input back. Replace with your wire
        # call — HTTP, MQTT, NATS, anything Python can talk to.
        return InvokeResult(
            response=f'{{"echoed":{payload}}}',
            status="OK",
        )

"""Sanity tests for the scaffolded sidecar plugin. Verifies that the
SDK-required shape is in place — discover returns at least one
service, invoke returns an InvokeResult with an OK status.
"""

from MY_PYTHON_PACKAGE_NAME import MyProtocol


def test_discover_returns_at_least_one_service():
    plugin = MyProtocol()
    services = plugin.discover(server_url="local://", show_internal=False)
    assert len(services) >= 1
    assert services[0].name == "DemoService"


def test_invoke_echoes_request_payload_with_ok_status():
    plugin = MyProtocol()
    result = plugin.invoke(
        server_url="local://",
        service="DemoService",
        method="Echo",
        json_messages=['{"message":"ping"}'],
        show_internal=False,
        metadata={},
    )
    assert result.status == "OK"
    assert "ping" in (result.response or "")

// Copyright 2026 Küstenlogik
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
#if (Preset == "rest")
using System.Net.Http;
#elif (Preset == "mqtt")
using MQTTnet;
#elif (Preset == "websocket")
using System.Net.WebSockets;
using System.Text;
#elif (Preset == "grpc")
using Grpc.Net.Client;
#elif (Preset == "signalr")
using Microsoft.AspNetCore.SignalR.Client;
#endif
using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Models;

namespace Bowire.Plugin1;

/// <summary>
/// Bowire protocol plugin for MY_PROTOCOL_DISPLAY_NAME. Bowire auto-discovers
/// this class because it implements <see cref="IBowireProtocol"/>.
/// </summary>
public sealed class MyProtocol : IBowireProtocol
{
    private IServiceProvider? _serviceProvider;

#if (Preset == "rest")
    // One HttpClient for the lifetime of the plugin; 30 s matches the rest
    // of the Bowire stack.
    private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(30) };
#endif

    public string Name => "MY_PROTOCOL_DISPLAY_NAME";

    public string Id => "MY_PROTOCOL_ID";

    public string IconSvg => """ICON_SVG_PLACEHOLDER""";

    public void Initialize(IServiceProvider? serviceProvider)
    {
        // In embedded mode the IServiceProvider lets you resolve host services
        // (e.g. EndpointDataSource) so you can discover schemas in-process
        // without HTTP. In standalone mode this is null.
        _serviceProvider = serviceProvider;
    }

    public async Task<List<BowireServiceInfo>> DiscoverAsync(
        string serverUrl, bool showInternalServices, CancellationToken ct = default)
    {
#if (Preset == "rest")
        // REST / OpenAPI — fetch the schema and build one BowireServiceInfo
        // per tag. The scaffold just probes the default OpenAPI URL and
        // returns a static demo; replace the body with a real OpenAPI parse
        // (e.g. via Microsoft.OpenApi.Readers).
        if (!string.IsNullOrEmpty(serverUrl))
        {
            try
            {
                using var response = await s_http.GetAsync(
                    $"{serverUrl.TrimEnd('/')}/openapi/v1.json", ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                _ = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                // TODO: parse the OpenAPI document here and map paths ->
                // BowireServiceInfo / BowireMethodInfo.
            }
            catch (HttpRequestException)
            {
                // Fall through to the demo below.
            }
        }
#elif (Preset == "mqtt")
        // MQTT — there is no wire-level discovery protocol, so plugins
        // typically expose a configurable topic list. The scaffold returns
        // one demo topic; replace it with your own config source.
        await Task.CompletedTask.ConfigureAwait(false);
#elif (Preset == "websocket")
        // WebSocket — no schema on the wire; the scaffold exposes a single
        // demo endpoint. Swap in a real config source (appsettings, an env
        // var, ...) or query an optional sidecar endpoint for the endpoint
        // list.
        await Task.CompletedTask.ConfigureAwait(false);
#elif (Preset == "grpc")
        // gRPC — real discovery uses Server Reflection
        // (Grpc.Reflection.V1Alpha.ServerReflection). The scaffold just
        // returns the demo service; swap this out for a reflection client
        // that enumerates services + methods from the target.
        await Task.CompletedTask.ConfigureAwait(false);
#elif (Preset == "signalr")
        // SignalR — hubs don't publish a schema, so plugins typically ship
        // with a configurable hub/method list. The scaffold returns one
        // demo method; replace with your own config source.
        await Task.CompletedTask.ConfigureAwait(false);
#else
        // TODO: discover real services from `serverUrl` (fetch a schema,
        // enumerate endpoints, etc.) and map them into BowireServiceInfo /
        // BowireMethodInfo.
        await Task.CompletedTask.ConfigureAwait(false);
#endif

        var echoInput = new BowireMessageInfo(
            Name: "EchoRequest",
            FullName: "DemoService.EchoRequest",
            Fields: []);

        var echoOutput = new BowireMessageInfo(
            Name: "EchoResponse",
            FullName: "DemoService.EchoResponse",
            Fields: []);

        var echo = new BowireMethodInfo(
            Name: "Echo",
            FullName: "DemoService/Echo",
            ClientStreaming: false,
            ServerStreaming: false,
            InputType: echoInput,
            OutputType: echoOutput,
            MethodType: "Unary");

        var service = new BowireServiceInfo(
            Name: "DemoService",
            Package: Id,
            Methods: [echo]);

        return [service];
    }

    public async Task<InvokeResult> InvokeAsync(
        string serverUrl, string service, string method,
        List<string> jsonMessages, bool showInternalServices,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var payload = jsonMessages.Count > 0 ? jsonMessages[0] : "{}";

#if (Preset == "rest")
        // REST — build an HttpRequestMessage from the method descriptor
        // (method -> HTTP verb, service -> path). The scaffold POSTs the raw
        // payload to serverUrl as JSON and wraps the response.
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{serverUrl.TrimEnd('/')}/{service}/{method}")
        {
            Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json"),
        };
        using var response = await s_http.SendAsync(request, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        return new InvokeResult(
            Response: body,
            DurationMs: 0,
            Status: response.IsSuccessStatusCode ? "OK" : response.StatusCode.ToString(),
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal));
#elif (Preset == "mqtt")
        // MQTT — one-shot publish to `service/method`; no response comes
        // back on the same call (reply-topic pattern goes through
        // OpenChannelAsync instead). The scaffold connects, publishes, and
        // disconnects; a real plugin should pool the client.
        var client = new MqttClientFactory().CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(new Uri(serverUrl).Host, new Uri(serverUrl).IsDefaultPort ? 1883 : new Uri(serverUrl).Port)
            .Build();
        await client.ConnectAsync(options, ct).ConfigureAwait(false);
        try
        {
            await client.PublishStringAsync($"{service}/{method}", payload, cancellationToken: ct).ConfigureAwait(false);
        }
        finally
        {
            await client.DisconnectAsync(cancellationToken: ct).ConfigureAwait(false);
        }

        return new InvokeResult(
            Response: """{ "published": true }""",
            DurationMs: 0,
            Status: "OK",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal));
#elif (Preset == "websocket")
        // WebSocket — open a transient connection, send the payload as a
        // text frame, wait for one response frame, close. For interactive
        // duplex use the IBowireChannel path in OpenChannelAsync instead.
        using var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri($"{serverUrl.TrimEnd('/')}/{service}/{method}"), ct).ConfigureAwait(false);

        var sendBytes = Encoding.UTF8.GetBytes(payload);
        await socket.SendAsync(sendBytes, WebSocketMessageType.Text, endOfMessage: true, ct).ConfigureAwait(false);

        var buffer = new byte[4096];
        var result = await socket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
        var body = Encoding.UTF8.GetString(buffer, 0, result.Count);

        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", ct).ConfigureAwait(false);

        return new InvokeResult(
            Response: body,
            DurationMs: 0,
            Status: "OK",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal));
#elif (Preset == "grpc")
        // gRPC — a real invoke needs either a generated client or a Server
        // Reflection + dynamic proto path. The scaffold just opens the
        // channel to prove connectivity; swap in your own invoke logic.
        using var channel = GrpcChannel.ForAddress(serverUrl);
        await channel.ConnectAsync(ct).ConfigureAwait(false);

        return new InvokeResult(
            Response: $$"""{ "connected": true, "service": "{{service}}", "method": "{{method}}" }""",
            DurationMs: 0,
            Status: "OK",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal));
#elif (Preset == "signalr")
        // SignalR — open a hub connection, invoke the method with the
        // payload as a single argument, tear down. A real plugin should
        // pool the connection instead of opening one per call.
        var connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .Build();
        await connection.StartAsync(ct).ConfigureAwait(false);
        try
        {
            var response = await connection.InvokeAsync<string>(method, payload, ct).ConfigureAwait(false);
            return new InvokeResult(
                Response: response,
                DurationMs: 0,
                Status: "OK",
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal));
        }
        finally
        {
            await connection.DisposeAsync().ConfigureAwait(false);
        }
#else
        // TODO: invoke the real target. The scaffold echoes the first
        // request back unchanged so you can see a successful round-trip in
        // the UI.
        await Task.CompletedTask.ConfigureAwait(false);
        return new InvokeResult(
            Response: payload,
            DurationMs: 0,
            Status: "OK",
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal));
#endif
    }

    public async IAsyncEnumerable<string> InvokeStreamAsync(
        string serverUrl, string service, string method,
        List<string> jsonMessages, bool showInternalServices,
        Dictionary<string, string>? metadata = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // TODO: stream real responses. The scaffold emits three demo messages
        // so the Bowire streaming pane shows activity immediately, regardless
        // of preset.
        for (var i = 1; i <= 3 && !ct.IsCancellationRequested; i++)
        {
            await Task.Delay(100, ct).ConfigureAwait(false);
            yield return $$"""{ "index": {{i}}, "message": "demo" }""";
        }
    }

    public Task<IBowireChannel?> OpenChannelAsync(
        string serverUrl, string service, string method,
        bool showInternalServices, Dictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
#if (IncludeDuplexChannel)
        // TODO: replace with a real transport (WebSocket handshake, MQTT
        // subscribe, SignalR connection, ...). The scaffold returns an echo
        // channel so the Bowire UI shows live duplex traffic immediately.
        return Task.FromResult<IBowireChannel?>(new MyProtocolChannel());
#else
        // TODO: return an IBowireChannel for interactive duplex/client-streaming.
        // Returning null means the plugin does not support interactive channels;
        // unary and server-streaming calls still work.
        return Task.FromResult<IBowireChannel?>(null);
#endif
    }
}

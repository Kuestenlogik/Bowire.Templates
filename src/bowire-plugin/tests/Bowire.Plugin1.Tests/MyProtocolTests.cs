// Copyright 2026 Küstenlogik
// SPDX-License-Identifier: Apache-2.0

using Bowire.Plugin1;
using Xunit;

namespace Bowire.Plugin1.Tests;

public class MyProtocolTests
{
    [Fact]
    public void Name_and_Id_are_populated()
    {
        var protocol = new MyProtocol();

        Assert.False(string.IsNullOrWhiteSpace(protocol.Name));
        Assert.False(string.IsNullOrWhiteSpace(protocol.Id));
        Assert.False(string.IsNullOrWhiteSpace(protocol.IconSvg));
    }

    [Fact]
    public async Task DiscoverAsync_returns_at_least_one_service()
    {
        var protocol = new MyProtocol();
        protocol.Initialize(serviceProvider: null);

        var services = await protocol.DiscoverAsync(
            serverUrl: string.Empty,
            showInternalServices: false,
            ct: TestContext.Current.CancellationToken);

        Assert.NotEmpty(services);
        Assert.All(services, s => Assert.NotEmpty(s.Methods));
    }

#if (Preset == "none")
    [Fact]
    public async Task InvokeAsync_echoes_the_request_payload()
    {
        // The default scaffold echoes the request unchanged. Presets (rest,
        // mqtt, websocket) replace the body with a real transport call, so
        // this test is gated to the "none" preset. Add your own transport-
        // specific InvokeAsync test when you wire up a real target.
        var protocol = new MyProtocol();
        const string payload = """{ "hello": "world" }""";

        var result = await protocol.InvokeAsync(
            serverUrl: string.Empty,
            service: "DemoService",
            method: "Echo",
            jsonMessages: [payload],
            showInternalServices: false,
            ct: TestContext.Current.CancellationToken);

        Assert.Equal("OK", result.Status);
        Assert.Equal(payload, result.Response);
    }
#endif

#if (!IncludeDuplexChannel)
    [Fact]
    public async Task OpenChannelAsync_returns_null_by_default()
    {
        var protocol = new MyProtocol();

        var channel = await protocol.OpenChannelAsync(
            serverUrl: string.Empty,
            service: "DemoService",
            method: "Echo",
            showInternalServices: false,
            ct: TestContext.Current.CancellationToken);

        Assert.Null(channel);
    }
#else
    [Fact]
    public async Task OpenChannelAsync_returns_an_echo_channel()
    {
        var protocol = new MyProtocol();

        await using var channel = await protocol.OpenChannelAsync(
            serverUrl: string.Empty,
            service: "DemoService",
            method: "Echo",
            showInternalServices: false,
            ct: TestContext.Current.CancellationToken);

        Assert.NotNull(channel);

        const string payload = """{ "hello": "duplex" }""";
        Assert.True(await channel!.SendAsync(payload, TestContext.Current.CancellationToken));
        Assert.Equal(1, channel.SentCount);

        await channel.CloseAsync(TestContext.Current.CancellationToken);
        Assert.True(channel.IsClosed);

        var received = new List<string>();
        await foreach (var msg in channel.ReadResponsesAsync(TestContext.Current.CancellationToken))
        {
            received.Add(msg);
        }

        Assert.Equal([payload], received);
    }
#endif
}

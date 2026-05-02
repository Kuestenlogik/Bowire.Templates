// Copyright 2026 Küstenlogik
// SPDX-License-Identifier: Apache-2.0

using Bowire.Plugin1;
using Kuestenlogik.Bowire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Bowire.Plugin1.IntegrationTests;

/// <summary>
/// Hosts the plugin inside an ASP.NET Core test server with
/// <c>AddBowire()</c> / <c>MapBowire()</c> wired up, so we can exercise the
/// plugin over real HTTP just like the Bowire workbench does in production.
/// </summary>
/// <remarks>
/// Bowire's auto-discovery only scans assemblies whose name contains
/// <c>"Bowire"</c>. If you rename the plugin project to something without
/// that substring (e.g. <c>Contoso.Nats.Plugin</c>), <c>/bowire/api/protocols</c>
/// will return <c>[]</c> and these tests fail. Either keep <c>Bowire</c> in
/// the project name (the recommended <c>&lt;Vendor&gt;.Bowire.Protocol.&lt;Name&gt;</c>
/// convention) or register the protocol manually via an internal Bowire
/// registry helper.
/// </remarks>
public sealed class PluginIntegrationTests : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient? _client;

    public async ValueTask InitializeAsync()
    {
        // Force-load the plugin assembly. Without this, a fresh xunit test
        // runner may not have touched the plugin DLL yet, so it's absent from
        // AppDomain.CurrentDomain.GetAssemblies() when AddBowire()'s
        // reflection scan runs — and /bowire/api/protocols returns [].
        _ = typeof(MyProtocol).Assembly;

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddBowire();

        _app = builder.Build();
        _app.UseRouting();
        _app.MapBowire();

        await _app.StartAsync().ConfigureAwait(false);
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.StopAsync().ConfigureAwait(false);
            await _app.DisposeAsync().ConfigureAwait(false);
        }
    }

    [Fact]
    public async Task Bowire_api_protocols_lists_this_plugin()
    {
        // The plugin is auto-discovered via assembly scanning because it
        // implements IBowireProtocol and lives next to Bowire's loader.
        // /bowire/api/protocols returns the registered protocols as JSON;
        // the plugin's Id must be in there.
        var ct = TestContext.Current.CancellationToken;
        using var response = await _client!.GetAsync("/bowire/api/protocols", ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        Assert.Contains("MY_PROTOCOL_ID", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Bowire_api_services_returns_the_demo_service()
    {
        // DiscoverAsync in MyProtocol.cs returns a single "DemoService". The
        // services endpoint surfaces everything discovered across plugins,
        // so the demo must show up in the response body.
        // TODO: replace with assertions specific to your real services once
        // DiscoverAsync points at a real backend.
        var ct = TestContext.Current.CancellationToken;
        using var response = await _client!.GetAsync("/bowire/api/services", ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        Assert.Contains("DemoService", body, StringComparison.Ordinal);
    }
}

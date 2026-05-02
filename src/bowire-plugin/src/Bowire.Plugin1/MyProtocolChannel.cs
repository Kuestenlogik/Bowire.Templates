// Copyright 2026 Küstenlogik
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Kuestenlogik.Bowire;

namespace Bowire.Plugin1;

/// <summary>
/// Demo duplex channel for MY_PROTOCOL_DISPLAY_NAME. Every message passed to
/// <see cref="SendAsync"/> is echoed straight back through
/// <see cref="ReadResponsesAsync"/> so the Bowire streaming pane shows a
/// live round-trip on both sides. Replace the internals with a real
/// transport (WebSocket, MQTT, SignalR, ...).
/// </summary>
public sealed class MyProtocolChannel : IBowireChannel
{
    private readonly Channel<string> _incoming = Channel.CreateUnbounded<string>();
    private readonly long _startedTicks = Environment.TickCount64;
    private int _sentCount;
    private bool _isClosed;

    public string Id { get; } = Guid.NewGuid().ToString("N");

    public bool IsClientStreaming => true;

    public bool IsServerStreaming => true;

    public int SentCount => _sentCount;

    public bool IsClosed => _isClosed;

    public long ElapsedMs => Environment.TickCount64 - _startedTicks;

    public Task<bool> SendAsync(string jsonMessage, CancellationToken ct = default)
    {
        if (_isClosed)
        {
            return Task.FromResult(false);
        }

        Interlocked.Increment(ref _sentCount);
        // Echo the inbound message back so the UI sees a response without a
        // real server. Swap this for a transport-side write in real code.
        return Task.FromResult(_incoming.Writer.TryWrite(jsonMessage));
    }

    public Task CloseAsync(CancellationToken ct = default)
    {
        if (_isClosed)
        {
            return Task.CompletedTask;
        }

        _isClosed = true;
        _incoming.Writer.TryComplete();
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> ReadResponsesAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var msg in _incoming.Reader.ReadAllAsync(ct).ConfigureAwait(false))
        {
            yield return msg;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
    }
}

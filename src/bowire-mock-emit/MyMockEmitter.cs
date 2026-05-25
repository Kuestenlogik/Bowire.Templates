// Bowire mock-emitter scaffold.
//
// IBowireMockEmitter is the replay backend that `bowire mock` uses
// to re-emit a captured BowireRecording over a real wire protocol.
// The mock-server host polls every registered emitter via CanEmit
// when a recording loads, hands the recording to the one that
// claims it, and StartAsync runs for the lifetime of the mock
// server.
//
// Payload-decoding convention shared with every first-party
// emitter (Kafka, AMQP, DIS, UDP, TacticalAPI): prefer
// step.ResponseBinary (base64) so binary payloads round-trip
// byte-for-byte; fall back to step.Body (JSON/text). Honour
// MockEmitterOptions.ReplaySpeed (timing multiplier) and
// MockEmitterOptions.Loop (re-emit after the last step).
//
// See https://github.com/Kuestenlogik/Bowire/blob/main/docs/architecture/plugin-architecture.md#ibowiremockemitter

using Kuestenlogik.Bowire.Mocking;
using Microsoft.Extensions.Logging;

namespace Bowire.Plugin1;

/// <summary>
/// Replays a <see cref="BowireRecording"/> over MY_EMITTER_ID. The mock
/// host discovers this type via the same assembly scan that finds
/// <c>IBowireProtocol</c> implementations, so dropping the assembly
/// in <c>~/.bowire/plugins/</c> is enough — no manual registration.
/// </summary>
public sealed class MyMockEmitter : IBowireMockEmitter
{
    public string Id => "MY_EMITTER_ID";

    /// <summary>
    /// Decide whether this emitter can replay the given recording.
    /// Match on whatever uniquely identifies your protocol —
    /// typically <c>recording.Protocol</c> (the protocol id captured
    /// at record time).
    /// </summary>
    public bool CanEmit(BowireRecording recording)
        => string.Equals(recording.Protocol, Id, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Replay the recording. Runs until <paramref name="ct"/> is
    /// cancelled (mock server shutdown) or — if
    /// <see cref="MockEmitterOptions.Loop"/> is false — the last
    /// step has been emitted.
    /// </summary>
    public async Task StartAsync(
        BowireRecording recording,
        MockEmitterOptions options,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation(
            "MY_EMITTER_ID mock-emitter starting: {Steps} step(s), speed={Speed}x, loop={Loop}",
            recording.Steps.Count, options.ReplaySpeed, options.Loop);

        // TODO: open the producer/connection/socket for your protocol
        // here so a failure surfaces *before* the first step.

        do
        {
            DateTimeOffset? prevTimestamp = null;

            foreach (var step in recording.Steps)
            {
                ct.ThrowIfCancellationRequested();

                if (prevTimestamp is { } prev && step.Timestamp > prev)
                {
                    var gap = (step.Timestamp - prev) / options.ReplaySpeed;
                    if (gap > TimeSpan.Zero)
                    {
                        await Task.Delay(gap, ct);
                    }
                }
                prevTimestamp = step.Timestamp;

                // Payload-decoding convention: prefer binary if the
                // recording captured one; fall back to text body.
                var payload = step.ResponseBinary is { Length: > 0 } bin
                    ? bin
                    : System.Text.Encoding.UTF8.GetBytes(step.Body ?? string.Empty);

                // TODO: emit `payload` over your transport. Examples
                // from the in-tree emitters:
                //   - KafkaMockEmitter:    producer.ProduceAsync(topic, …)
                //   - AmqpMockEmitter:     channel.BasicPublishAsync(exchange, routingKey, …)
                //   - UdpMockEmitter:      udpClient.SendAsync(payload, payload.Length, endpoint)
                //   - TacticalApiMockEmitter: grpc-call(method, payload)
                logger.LogTrace("MY_EMITTER_ID emit: {Bytes} byte(s)", payload.Length);
            }
        } while (options.Loop && !ct.IsCancellationRequested);
    }

    public ValueTask DisposeAsync()
    {
        // TODO: close producer/connection/socket here.
        return ValueTask.CompletedTask;
    }
}

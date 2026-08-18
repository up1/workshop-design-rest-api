using System.Text.Json.Serialization;

namespace Messaging.Shared.Messages;

/// <summary>W3C Trace Context headers carried on the OrderCreated message (see components.messages.OrderCreated.headers in asyncapi.yaml).</summary>
public sealed record OrderCreatedHeaders
{
    [JsonPropertyName("traceparent")]
    public string? Traceparent { get; init; }

    [JsonPropertyName("tracestate")]
    public string? Tracestate { get; init; }
}

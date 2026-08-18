using System.Text.Json.Serialization;

namespace Messaging.Shared.Messages;

/// <summary>Payload for the OrderCreated event (see components.schemas.OrderCreatedPayload in asyncapi.yaml).</summary>
public sealed record OrderCreatedPayload
{
    [JsonPropertyName("order_id")]
    public required Guid OrderId { get; init; }

    [JsonPropertyName("total_price")]
    public required long TotalPrice { get; init; }

    [JsonPropertyName("customer_id")]
    public required int CustomerId { get; init; }

    [JsonPropertyName("product_id")]
    public required int ProductId { get; init; }

    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }
}

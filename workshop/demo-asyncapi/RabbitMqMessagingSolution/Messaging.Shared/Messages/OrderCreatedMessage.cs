namespace Messaging.Shared.Messages;

/// <summary>OrderCreated event: published to the "orders" exchange and consumed from the "report" queue (see asyncapi.yaml).</summary>
public sealed record OrderCreatedMessage
{
    public OrderCreatedHeaders? Headers { get; init; }

    public required OrderCreatedPayload Payload { get; init; }

    // correlationId location per asyncapi.yaml: $message.payload#/order_id
    public Guid CorrelationId => Payload.OrderId;
}

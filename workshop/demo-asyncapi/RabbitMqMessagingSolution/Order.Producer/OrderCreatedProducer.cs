using System.Text.Json;
using Messaging.Shared.Messages;
using RabbitMQ.Client;

namespace Order.Producer;

/// <summary>Publishes OrderCreated events to the "orders" exchange (see channels.orders / operations.publishOrderCreated in asyncapi.yaml).</summary>
public sealed class OrderCreatedProducer : IAsyncDisposable
{
    // Mirrors channels.orders.bindings.amqp.exchange in asyncapi.yaml
    private const string ExchangeName = "orders";
    private const bool ExchangeDurable = true;
    private const bool ExchangeAutoDelete = false;

    // Mirrors operations.publishOrderCreated.bindings.amqp.deliveryMode (2 = persistent)
    private const DeliveryModes MessageDeliveryMode = DeliveryModes.Persistent;

    private const string MessageContentType = "application/json";

    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private OrderCreatedProducer(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<OrderCreatedProducer> CreateAsync(string hostName = "rabbitmq", CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: ExchangeDurable,
            autoDelete: ExchangeAutoDelete,
            cancellationToken: cancellationToken);

        return new OrderCreatedProducer(connection, channel);
    }

    public async Task PublishAsync(OrderCreatedMessage message, CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(message.Payload);

        var properties = new BasicProperties
        {
            ContentType = MessageContentType,
            DeliveryMode = MessageDeliveryMode,
            CorrelationId = message.CorrelationId.ToString(),
            Headers = new Dictionary<string, object?>
            {
                ["traceparent"] = message.Headers?.Traceparent,
                ["tracestate"] = message.Headers?.Tracestate,
            },
        };

        // fanout exchange ignores the routing key, but the channel address ("orders") is used per the spec's binding
        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

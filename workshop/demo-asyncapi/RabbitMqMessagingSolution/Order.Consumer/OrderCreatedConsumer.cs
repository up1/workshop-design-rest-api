using System.Text;
using System.Text.Json;
using Messaging.Shared.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Order.Consumer;

/// <summary>Consumes OrderCreated events from the "report" queue (see channels.report / operations.subscribeOrderCreated in asyncapi.yaml).</summary>
public sealed class OrderCreatedConsumer : IAsyncDisposable
{
    // Mirrors channels.orders.bindings.amqp.exchange in asyncapi.yaml
    private const string ExchangeName = "orders";

    // Mirrors channels.report.bindings.amqp.queue in asyncapi.yaml
    private const string QueueName = "report";
    private const bool QueueDurable = true;
    private const bool QueueExclusive = false;
    private const bool QueueAutoDelete = false;

    // Mirrors operations.subscribeOrderCreated.bindings.amqp.ack (manual acknowledgement)
    private const bool AutoAck = false;

    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private OrderCreatedConsumer(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<OrderCreatedConsumer> CreateAsync(string hostName = "rabbitmq", CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: QueueDurable,
            exclusive: QueueExclusive,
            autoDelete: QueueAutoDelete,
            cancellationToken: cancellationToken);

        // fanout exchange, so the routing key used for the bind is irrelevant
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        return new OrderCreatedConsumer(connection, channel);
    }

    public async Task ConsumeAsync(Func<OrderCreatedMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken = default)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var payload = JsonSerializer.Deserialize<OrderCreatedPayload>(ea.Body.Span)
                ?? throw new InvalidOperationException("Failed to deserialize OrderCreated payload.");

            var message = new OrderCreatedMessage
            {
                Headers = new OrderCreatedHeaders
                {
                    Traceparent = GetHeaderString(ea.BasicProperties.Headers, "traceparent"),
                    Tracestate = GetHeaderString(ea.BasicProperties.Headers, "tracestate"),
                },
                Payload = payload,
            };

            try
            {
                await onMessage(message, cancellationToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
            }
            catch
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken);
                throw;
            }
        };

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: AutoAck, consumer: consumer, cancellationToken: cancellationToken);
    }

    private static string? GetHeaderString(IDictionary<string, object?>? headers, string key)
    {
        if (headers is null || !headers.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : value.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

using Messaging.Shared.Messages;
using Order.Producer;

var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq";

await using var producer = await OrderCreatedProducer.CreateAsync(hostName);

var message = new OrderCreatedMessage
{
    Headers = new OrderCreatedHeaders
    {
        Traceparent = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    },
    Payload = new OrderCreatedPayload
    {
        OrderId = Guid.NewGuid(),
        TotalPrice = 1000,
        CustomerId = 1,
        ProductId = 1,
        CreatedAt = DateTimeOffset.UtcNow,
    },
};

await producer.PublishAsync(message);

Console.WriteLine($"Published OrderCreated event {message.CorrelationId}");

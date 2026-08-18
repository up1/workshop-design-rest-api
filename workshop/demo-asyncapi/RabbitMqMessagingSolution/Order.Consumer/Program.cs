using Order.Consumer;

var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq";

await using var consumer = await OrderCreatedConsumer.CreateAsync(hostName);

await consumer.ConsumeAsync((message, _) =>
{
    Console.WriteLine($"Received OrderCreated event {message.CorrelationId} (traceparent={message.Headers?.Traceparent})");
    return Task.CompletedTask;
});

Console.WriteLine("Listening for OrderCreated events. Press any key to exit.");
Console.ReadKey();

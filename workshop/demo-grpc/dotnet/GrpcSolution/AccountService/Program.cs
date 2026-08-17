using AccountService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for high-performance HTTP/2 local benchmarking
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen for gRPC on port 5001 explicitly using HTTP/2 without TLS
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
});

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<MyAccountService>();
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

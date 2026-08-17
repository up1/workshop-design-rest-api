# gRPC with .NET 10

## Create Solution
```
$mkdir GrpcSolution
$cd GrpcSolution
$dotnet new sln -n GrpcSolution
```

## Create gRPC server project
```
$dotnet new grpc -n AccountService
$dotnet sln GrpcSolution.slnx add AccountService/AccountService.csproj
```

## Create gRPC client project
```
$dotnet new console -n AccountClient
$dotnet sln GrpcSolution.slnx add AccountClient/AccountClient.csproj
```

## Generate code from proto file
```
$dotnet build 
```
Output in folder `obj/`

## Create `MyAccountService.cs` to implement gRPC service
```
using Grpc.Core;

namespace AccountService.Services;

public class MyAccountService(ILogger<MyAccountService> logger) : Account.AccountBase
{
    // Overrided method from Account.AccountBase
    public override Task<GetAccountResponse> GetAccount(GetAccountRequest request, ServerCallContext context)
    {
        logger.LogInformation("GetAccount called with id {Id}", request.Id);

        var response = new GetAccountResponse
        {
            Id = request.Id,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Address = "123 Main St"
        };

        return Task.FromResult(response);
    }
}
```

Register service in `Program.cs`
```
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


```

## Build project and Testing
```
$dotnet clean
$dotnet build
$dotnet run
```

Testing
```
$grpcurl -plaintext localhost:5278 list
$grpcurl -d '{"id":123}' -plaintext localhost:5278 account.Account/GetAccount
```

## Implement gRPC client in `Program.cs`
```
using AccountService;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5278");
var client = new Account.AccountClient(channel);

var id = args.Length > 0 && int.TryParse(args[0], out var parsedId) ? parsedId : 1;

var response = await client.GetAccountAsync(new GetAccountRequest { Id = id });

Console.WriteLine($"Id: {response.Id}");
Console.WriteLine($"Name: {response.Name}");
Console.WriteLine($"Email: {response.Email}");
Console.WriteLine($"Address: {response.Address}");
```

Run client
```
$cd AccountClient
$dotnet run 123
```

## Load testing with k6
```
$cd k6
$k6 run grpc-test.js
```
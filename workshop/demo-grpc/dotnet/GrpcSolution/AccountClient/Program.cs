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

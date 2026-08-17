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

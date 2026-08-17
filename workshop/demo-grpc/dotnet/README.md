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
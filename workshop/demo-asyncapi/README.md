# Workshop with AsyncAPI
* .NET
* Classic RabbitMQ

## Design message flow with AsyncAPI


## Generate .NET with AsyncAPI
* Producer
* Consumer

Create projects
```
# Create the root solution directory and enter it
$mkdir RabbitMqMessagingSolution
$cd RabbitMqMessagingSolution

# Create a blank .NET solution file
dotnet new sln -n RabbitMqMessagingSolution

# 1. Create the Shared Contracts Class Library
dotnet new classlib -n Messaging.Shared

# 2. Create the Producer Console App
dotnet new console -n Order.Producer

# 3. Create the Consumer Console App
dotnet new console -n Order.Consumer

# Link all three projects into the parent solution file
dotnet sln add Messaging.Shared/Messaging.Shared.csproj
dotnet sln add Order.Producer/Order.Producer.csproj
dotnet sln add Order.Consumer/Order.Consumer.csproj

# Add the NuGet RabbitMQ dependency to both executable apps
dotnet add Order.Producer/Order.Producer.csproj package RabbitMQ.Client
dotnet add Order.Consumer/Order.Consumer.csproj package RabbitMQ.Client

# Add a project reference so the Producer and Consumer can see the Shared contracts
dotnet add Order.Producer/Order.Producer.csproj reference Messaging.Shared/Messaging.Shared.csproj
dotnet add Order.Consumer/Order.Consumer.csproj reference Messaging.Shared/Messaging.Shared.csproj

```
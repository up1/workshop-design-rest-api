# Workshop with AsyncAPI
* .NET
* Classic RabbitMQ

## Design message flow with AsyncAPI


## Create a solution with three projects
* Producer
* Consumer
* Shared Contracts

Create projects
```
# Create the root solution directory and enter it
$mkdir RabbitMqMessagingSolution
$cd RabbitMqMessagingSolution

# Create a blank .NET solution file
$dotnet new sln -n RabbitMqMessagingSolution

# 1. Create the Shared Contracts Class Library
$dotnet new classlib -n Messaging.Shared

# 2. Create the Producer Console App
$dotnet new console -n Order.Producer

# 3. Create the Consumer Console App
$dotnet new console -n Order.Consumer

# Link all three projects into the parent solution file
$dotnet sln add Messaging.Shared/Messaging.Shared.csproj
$dotnet sln add Order.Producer/Order.Producer.csproj
$dotnet sln add Order.Consumer/Order.Consumer.csproj

# Add the NuGet RabbitMQ dependency to both executable apps
$dotnet add Order.Producer/Order.Producer.csproj package RabbitMQ.Client
$dotnet add Order.Consumer/Order.Consumer.csproj package RabbitMQ.Client

# Add a project reference so the Producer and Consumer can see the Shared contracts
$dotnet add Order.Producer/Order.Producer.csproj reference Messaging.Shared/Messaging.Shared.csproj
$dotnet add Order.Consumer/Order.Consumer.csproj reference Messaging.Shared/Messaging.Shared.csproj

```

## Generate code from AsyncAPI specification

Install the [AsyncAPI CLI](https://www.asyncapi.com/tools/cli)
```
$npm install -g @asyncapi/cli
or 
$brew install asyncapi
```

Generate code from the AsyncAPI specification
```
$asyncapi validate asyncapi.yaml

$asyncapi generate fromTemplate asyncapi.yaml @asyncapi/dotnet-rabbitmq-template -o Demo --force-write --debug
```

## Generated code with AI Agent
```
Try to generate event/message POCO class to @attachment:Messaging.Shared that read config from file @sym:asyncapi


Try to generate Producer class to @attachment:Order.Producer that read config from file @sym:asyncapi


Try to generate Consumer class to @attachment:Order.Consumer that read config from file @sym:asyncapi


```

## Start Producer and Consumer
Start rabiitmq server with docker
```
$docker run -d  -p 5672:5672 -p 15672:15672 rabbitmq:3-management 
$docker container ps
```
Go to http://localhost:15672/ and login with guest/guest


Start producer
```
$cd Order.Producer
$RABBITMQ_HOST=localhost dotnet run
```

Start consumer
```
$cd Order.Consumer
$RABBITMQ_HOST=localhost dotnet run
```

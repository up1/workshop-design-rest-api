# Workshop :: REST API vs gRPC
* Java and Spring Boot
* .NET Core
* Performance testing of REST API vs gRPC

## Workflow
Flow 1
```
Client -> REST API with Spring Boot
```

Flow 2
```
Client -> gRPC with Spring Boot
```

## REST API Specification
### GET /v1/account/{id}
- Description: Get account by ID
- Request:
```
GET /v1/account/123 HTTP/1.1
Host: localhost:8080
```
- Response:
```
HTTP/1.1 200 OK
Content-Type: application/json
{
  "id": 123,
  "name": "John Doe",
  "email": "john.doe@example.com",
  "address": "123 Main St, Anytown, USA"
}
```

## [gRPC Specification](https://grpc.io/)
* Create service definition using Protocol Buffers
* Define service methods and message types
* Generate server and client code using the Protocol Buffers compiler (protoc)
* Implement server and client logic in [Java using Spring Boot](https://docs.spring.io/spring-boot/reference/io/grpc.html)


### Service Definition
```proto
syntax = "proto3";
option java_multiple_files = true;
package com.example.accountgrpc.account;
service AccountService {
  rpc GetAccount (GetAccountRequest) returns (GetAccountResponse) {}
}

message GetAccountRequest {
  int32 id = 1;
}

message GetAccountResponse {
  int32 id = 1;
  string name = 2;
  string email = 3;
  string address = 4;
}
```

### Generated Code
* Use the [Protocol Buffers compiler (protoc)](https://grpc.io/docs/protoc-installation/) to generate server and client code in Java
  * [Installation Guide](https://protobuf.dev/installation/)

Steps
```
$protoc --version
$protoc --java_out=src/main/java --grpc-java_out=src/main/java -I=src/main/proto src/main/proto/account.proto
```

Or use `mvnw clean protobuf:generate` to generate the code automatically during the build process.

### Implementation
* Implement the server logic in Java using Spring Boot

```
@GrpcService
public class AccountService extends AccountServiceImplBase {
    @Override
    public void getAccount(GetAccountRequest request, StreamObserver<GetAccountResponse> responseObserver) {
        super.getAccount(request, responseObserver);
    }
}
```

Default port = 9090
```
spring.grpc.server.port=9090
```

Testing with [grpcurl](https://github.com/fullstorydev/grpcurl)
```
$grpcurl -plaintext localhost:9090 list
$grpcurl -d '{"id":123}' -plaintext localhost:9090 com.example.accountgrpc.account.AccountService/GetAccount
```

## Performance Testing
* Use a performance testing tool like [Apache JMeter](https://jmeter.apache.org/) and K6 to test the performance of REST API and gRPC

### Test Scenarios
1. Test the response time of REST API and gRPC for a single request
2. Test the throughput of REST API and gRPC for multiple concurrent requests


### REST API Performance Test
* Use K6 to create a test plan for the REST API

```
$ulimit -n
$k6 run rest-api-test.js
```

File rest-api-test.js
```javascript
import http from 'k6/http';
import { check } from 'k6';
// Workload configuration
const options = {
  vus: 10,
  duration: '30s',
};

export { options };

export default function () {
  const response = http.get('http://localhost:8080/v1/accounts/123');
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response has id': (r) => r.json().id === 123,
  });
}
```


### gRPC Performance Test
* Use K6 to create a test plan for the gRPC service

```
$ulimit -n
$k6 run grpc-test.js
```

File grpc-test.js
```javascript
import grpc from 'k6/net/grpc';
import { check } from 'k6';

// Workload configuration
const options = {
  vus: 10,
  duration: '30s',
};

const client = new grpc.Client();
client.load(['proto'], 'account.proto');
client.connect('localhost:9090', grpc.credentials.createInsecure());

export default function () {
  __ENV.options = options;
  const response = client.invoke('account.AccountService/GetAccount', { id: 123 });
  check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
    'response has id': (r) => r && r.message && r.message.id === 123,
  });
}
```

### gRPC Performance Test with ghz
* [ghz](https://github.com/bojand/ghz) is a load-testing tool built specifically for gRPC, avoiding any JS-side marshalling overhead a scripted client (like k6) introduces
* [Installation](https://ghz.sh/docs/install)
```
$brew install ghz
```

Run a benchmark against `AccountService/GetAccount`, using the same proto file as the service definition:
```
$cd k6
$ghz --insecure \
  --proto ./proto/accounts.proto \
  --call com.example.accountgrpc.account.AccountService/GetAccount \
  -d '{"id":123}' \
  -c 200 \
  --connections 50 \
  -z 30s \
  --duration-stop=wait \
  localhost:9090
```
- `--insecure`: use plaintext (no TLS), matching the server's `plaintext: true` setup
- `--proto`: path to the `.proto` file defining the service (no reflection needed)
- `--call`: fully-qualified `package.Service/Method` to invoke
- `-d`: request payload as JSON
- `-c`: total concurrency (number of concurrent request workers, analogous to k6 `vus`)
- `--connections`: number of underlying client connections the `-c` workers are spread across (defaults to 1, i.e. all workers share a single HTTP/2 connection)
- `-z`: total test duration (analogous to k6 `duration`)
- `--duration-stop=wait`: when `-z` expires, wait for in-flight requests to finish instead of the default `close`, which force-closes connections and can surface `use of closed network connection` errors for whatever was still in flight

ghz prints latency percentiles (p50/p90/p99), requests/sec, and status code breakdown directly, giving a client-overhead-free comparison point against the k6 REST results.

#### Troubleshooting: `rpc error: code = Unavailable ... use of closed network connection`
This means the TCP connection was torn down while ghz was still reading a response. Common causes:
- **`-z` duration expiring mid-request (most common on macOS runs with a handful of errors)**: ghz's default `--duration-stop=close` force-closes all connections the instant the duration elapses, failing whatever request was still in flight with this exact error. Use `--duration-stop=wait` (as above) so in-flight requests are allowed to finish instead of being cut off.
- **`--connections` too low (or unset)**: with the default of 1 connection, every `-c` worker multiplexes over a single HTTP/2 connection; hitting the server's `maxConcurrentCallsPerConnection` limit or any transient connection hiccup fails every in-flight request on that connection at once. Set `--connections` to spread load across multiple connections (as above).
- **The server process was stopped/restarted mid-test**: check it's still listening before and after the run:
  ```
  $lsof -nP -iTCP:9090 -sTCP:LISTEN
  ```
  Run the server with something that outlives the terminal session (e.g. `nohup ./mvnw spring-boot:run &` and `disown`), not a terminal that may be closed/cleaned up while the benchmark is running.
- **Low open-file-descriptor limit**: macOS defaults `ulimit -n` to 256 in a fresh shell, which can starve high-concurrency runs (`-c`/`--connections`) of sockets. Check with `ulimit -n` and raise it for the session if low: `ulimit -n 65536`.

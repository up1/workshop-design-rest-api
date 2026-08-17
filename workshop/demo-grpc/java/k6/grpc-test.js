import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';

// Workload configuration
export const options = {
  vus: 200,
  duration: '30s',
};

const client = new grpc.Client();
client.load(['proto'], 'accounts.proto');

let connected = false; // module state persists per VU across iterations

export default function () {
  if (!connected) {
    client.connect('localhost:9090', { plaintext: true });
    connected = true;
  }
  // discardResponseMessage skips deserializing the response into a JS object,
  // avoiding k6 client-side marshalling overhead that would otherwise skew the benchmark
  const response = client.invoke('com.example.accountgrpc.account.AccountService/GetAccount', { id: 123 }, {
    discardResponseMessage: true,
  });
  check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
  });
}

export function teardown(data) {
  client.close();
}
import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';

// Workload configuration
export const options = {
  vus: 100,
  duration: '30s',
};

const client = new grpc.Client();
client.load(['proto'], 'accounts.proto');

export default function () {
  // connect once per VU to avoid exhausting ephemeral ports each iteration
  if (__ITER === 0) {
    client.connect('localhost:9090', { plaintext: true });
  }
  const response = client.invoke('com.example.accountgrpc.account.AccountService/GetAccount', { id: 123 });
  check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
    'response has id': (r) => r && r.message && r.message.id === 123,
  });
  sleep(1);
}
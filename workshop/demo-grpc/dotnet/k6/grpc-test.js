import grpc from 'k6/net/grpc';
import { check } from 'k6';

// Workload configuration
const options = {
  vus: 400,
  duration: '30s',
};

export { options };

const client = new grpc.Client();
client.load(['proto'], 'accounts.proto');

export default function () {

  // Connect ONLY on the first iteration of this specific Virtual User
  if (__ITER === 0) {
    client.connect('localhost:5001', { plaintext: true });
  }

  const response = client.invoke(
    'account.Account/GetAccount',
    { id: 123 },
    { discardResponseMessage: true, timeout: '10s' }
  );

  check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
  });
}
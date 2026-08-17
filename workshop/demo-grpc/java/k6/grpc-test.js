import grpc from 'k6/net/grpc';
import exec from 'k6/execution';
import { check } from 'k6';

const HOST = __ENV.GRPC_HOST || 'localhost:9090';
const VUS = Number(__ENV.VUS || 200);
const DURATION = __ENV.DURATION || '30s';
// Each gRPC client owns one HTTP/2 connection, so all of a VU's streams
// serialise on a single socket. A small pool per VU spreads them out.
const CONNS_PER_VU = Number(__ENV.CONNS_PER_VU || 4);

// Workload configuration
export const options = {
  scenarios: {
    load: {
      executor: 'constant-vus',
      vus: VUS,
      duration: DURATION,
      gracefulStop: '5s',
    },
  },
  thresholds: {
    grpc_req_duration: ['p(95)<200'],
    checks: ['rate>0.99'],
  },
  systemTags: ['scenario', 'status', 'error_code'],
};

// Init context: proto parsing runs once per VU, before the test starts.
const clients = Array.from({ length: CONNS_PER_VU }, () => {
  const c = new grpc.Client();
  c.load(['proto'], 'accounts.proto');
  return c;
});

let connected = false; // module state persists per VU across iterations

export default function () {
  if (!connected) {
    for (const c of clients) {
      c.connect(HOST, { plaintext: true, reflect: false, timeout: '10s' });
    }
    connected = true;
  }

  const client = clients[exec.vu.iterationInInstance % CONNS_PER_VU];

  // discardResponseMessage skips deserializing the response into a JS object,
  // avoiding k6 client-side marshalling overhead that would otherwise skew the benchmark
  const response = client.invoke(
    'com.example.accountgrpc.account.AccountService/GetAccount',
    { id: 123 },
    { discardResponseMessage: true, timeout: '10s' }
  );

  check(response, {
    'status is OK': (r) => r && r.status === grpc.StatusOK,
  });
}
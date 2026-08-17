import http from 'k6/http';
import { check } from 'k6';
// Workload configuration
const options = {
  vus: 200,
  duration: '30s',
};

export { options };

export default function () {
  const response = http.get('http://localhost:8080/v1/accounts/123');
  check(response, {
    'status is 200': (r) => r.status === 200
  });
}
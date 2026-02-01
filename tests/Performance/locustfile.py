from locust import HttpUser, task, between
import os, time, random
from datetime import datetime, timezone

SERVER_HOST = os.getenv("SERVER_HOST", "http://localhost:5140")
AUTH_ENDPOINT = os.getenv("AUTH_ENDPOINT", "/api/v1/agent/auth")
REPORT_ENDPOINT = os.getenv("REPORT_ENDPOINT", "/api/v1/agent/report")

TEST_SECRET_KEY = os.getenv("TEST_SECRET_KEY", "test-secret-key")

METRICS_COUNT = int(os.getenv("METRICS_COUNT", "20"))
WAIT_INTERVAL = float(os.getenv("WAIT_INTERVAL", "10"))
SUCCESS_RATE = float(os.getenv("SUCCESS_RATE", "0.75"))

def create_test_metric():
  return {
    "type": "performance",
    "name": f"cpu_{random.randint(1, 4)}",
    "value": round(random.uniform(0, 100), 2),
    "unit": "percent",
    "timestampUtc": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "Z",
    "metadata": {
      "host": f"test-host-{random.randint(1, 100)}",
      "core": str(random.randint(1, 8))
    }
  }

def make_result(instruction_id: int):
  ok = (random.random() < SUCCESS_RATE)
  return {
    "associatedId": instruction_id,
    "success": ok,
    "output": "ok" if ok else None,
    "error": None if ok else "exit code 1"
  }

class AgentLoadTest(HttpUser):
  host = SERVER_HOST
  wait_time = between(WAIT_INTERVAL, WAIT_INTERVAL)

  def on_start(self):
    self.headers = {"Content-Type": "application/json"}
    self.pending_ids = []

    agent_num = random.randint(1, 1000)
    self.agent_name = f"agent-{agent_num:06d}"

    auth_request = {"agentName": self.agent_name, "secretKey": TEST_SECRET_KEY}

    r = self.client.post(AUTH_ENDPOINT, json=auth_request, headers=self.headers, name="agent_auth", timeout=10)
    if r.status_code != 200:
      raise RuntimeError(f"Auth failed {r.status_code}: {r.text[:200]}")

    auth_data = r.json()
    self.headers["Authorization"] = f"Bearer {auth_data['authToken']}"

  @task
  def report(self):
    metrics = [create_test_metric() for _ in range(METRICS_COUNT)]
    results = [make_result(i) for i in self.pending_ids]
    payload = {"metrics": metrics, "instructionResults": results}
    r = self.client.post(REPORT_ENDPOINT, json=payload, headers=self.headers, name="agent_report", timeout=15)

    if r.status_code == 204:
      self.pending_ids = []
      return

    if r.status_code != 200:
      r.raise_for_status()

    data = r.json()
    instr = data.get("instructions", [])
    self.pending_ids = [x["associatedId"] for x in instr]

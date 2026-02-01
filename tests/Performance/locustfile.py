from locust import HttpUser, task, between
import os, time, random, json
from datetime import datetime, timezone

SERVER_HOST = "http://localhost:5140"
REPORT_ENDPOINT = "/api/v1/agent/report"
TEST_SECRET_KEY = "test-secret-key"
METRICS_COUNT = 20
RESULTS_COUNT = 5
WAIT_INTERVAL = 2.0
TOTAL_AGENTS = 1000
SUCCESS_RATE = 0.75

print("="*60)
print("PERFORMANCE TEST CONFIGURATION:")
print(f"  Target Server: {SERVER_HOST}")
print(f"  Secret Key: {TEST_SECRET_KEY}")
print(f"  Metrics per Report: {METRICS_COUNT}")
print(f"  Command Results per Report: {RESULTS_COUNT}")
print(f"  Report Interval: {WAIT_INTERVAL} seconds")
print(f"  Agent Pool Size: {TOTAL_AGENTS}")
print(f"  Command Success Rate: {int(SUCCESS_RATE * 100)}%")
print("="*60)
print("To change test parameters, edit the values at the top of locustfile.py")
print("="*60)

def create_test_metric():
    """Create a metric message matching MetricMessage structure"""
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

def create_test_result(instruction_id: int):
    """Create an instruction result message matching InstructionResultMessage structure"""
    is_successful = random.choice([True, True, True, False])  # 75% success rate
    return {
        "associatedId": instruction_id,
        "success": is_successful,
        "output": "Command executed successfully" if is_successful else None,
        "error": None if is_successful else "Command failed with exit code 1"
    }

class AgentLoadTest(HttpUser):
    host = SERVER_HOST
    wait_time = between(WAIT_INTERVAL, WAIT_INTERVAL)

    def on_start(self):
        """Authenticate and get JWT token"""
        self.headers = {"Content-Type": "application/json"}
        self.base_instruction_id = random.randint(1, 200000)  # Random instruction ID from seeded data
        self.agent_name = f"agent-{random.randint(1, 1000):06d}"  # Use seeded agent names

        print(f"Starting agent {self.agent_name} with host {self.host}")

        # Authenticate to get JWT token using the known test secret key
        auth_request = {
            "agentName": self.agent_name,
            "secretKey": TEST_SECRET_KEY
        }

        with self.client.post("/api/v1/agent/auth", json=auth_request, headers=self.headers, catch_response=True) as response:
            if response.status_code == 200:
                try:
                    auth_data = response.json()
                    self.headers["Authorization"] = f"Bearer {auth_data['authToken']}"
                    print(f"Authenticated as {self.agent_name}")
                    response.success()
                except Exception as e:
                    print(f"Auth response parse failed for {self.agent_name}: {e}")
                    response.failure("Failed to parse auth response")
            else:
                print(f"Auth failed for {self.agent_name}: {response.status_code} - {response.text[:100]}")
                response.failure(f"Authentication failed: {response.status_code}")

    @task
    def send_agent_report(self):
        """Send report with metrics and instruction results"""
        test_metrics = [create_test_metric() for _ in range(METRICS_COUNT)]

        test_results = [
            create_test_result(self.base_instruction_id + i)
            for i in range(RESULTS_COUNT)
        ]

        report_data = {
            "metrics": test_metrics,
            "instructionResults": test_results
        }

        with self.client.post(REPORT_ENDPOINT, json=report_data, headers=self.headers, name="agent_report", timeout=15, catch_response=True) as response:
            if response.status_code in (200, 204):
                try:
                    response_data = response.json()
                    instructions_received = len(response_data.get("instructions", []))
                    print(f"Agent {self.agent_name} received {instructions_received} new instructions")
                    response.success()
                except Exception as e:
                    print(f"Response parse issue: {e}")
                    response.success()
            else:
                error_msg = f"Status {response.status_code}: {response.text[:200]}"
                print(f"Report failed for {self.agent_name}: {error_msg}")
                response.failure(error_msg)

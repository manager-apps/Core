// Runtime environment configuration
// This file is overwritten by Docker entrypoint script in production
window.__ENV__ = {
  API_URL: "http://localhost:5000/api/v1"
};

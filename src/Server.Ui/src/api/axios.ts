import axios from 'axios';

// Runtime config from window or build-time env variable
const getApiBaseUrl = (): string => {
  // Check for runtime config (injected by Docker entrypoint)
  if (typeof window !== 'undefined' && (window as any).__ENV__?.API_URL) {
    return (window as any).__ENV__.API_URL;
  }
  // Fallback to build-time env variable or default
  return import.meta.env.VITE_API_URL || "http://localhost:5000/api/v1";
};

const api = axios.create({
  baseURL: getApiBaseUrl(),
  headers: {
    'Content-Type': 'application/json'
  }
});

api.interceptors.response.use(
  response => response,
  error => {
    if (error.message === "Network Error") {
      console.error("Backend unreachable:", error);
    }
    return Promise.reject(error);
  }
);

export default api;

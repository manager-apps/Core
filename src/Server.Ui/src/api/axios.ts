import axios from 'axios';

export const getGrafanaBaseUrl = (): string => {
  if (typeof window !== 'undefined' && (window as any).__ENV__?.GRAFANA_URL) {
    return (window as any).__ENV__.GRAFANA_URL;
  }

  return import.meta.env.VITE_GRAFANA_URL || "http://localhost:3000";
}

export const getApiBaseUrl = (): string => {
  if (typeof window !== 'undefined' && (window as any).__ENV__?.API_URL) {
    return (window as any).__ENV__.API_URL;
  }
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

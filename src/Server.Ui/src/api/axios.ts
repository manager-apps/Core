import axios from 'axios';

const api = axios.create({
  baseURL: "http://localhost:5140/api/v1",
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

import axios from "axios";

export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5242";

export const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api/v1`,
  headers: {
    "Content-Type": "application/json",
  },
});

// Request interceptor to attach tenant and token headers
apiClient.interceptors.request.use((config) => {
  if (typeof window !== "undefined") {
    const tenantId = localStorage.getItem("current_tenant_id");
    if (tenantId) {
      config.headers["X-Tenant-Id"] = tenantId;
    }
    const token = localStorage.getItem("auth_token");
    if (token) {
      config.headers["Authorization"] = `Bearer ${token}`;
    }
  }
  return config;
});

// Response interceptor to format RFC 7807 errors
apiClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    if (error.response?.data) {
      const problemDetails = error.response.data;
      const message = problemDetails.detail || problemDetails.title || "An error occurred";
      return Promise.reject(new Error(message));
    }
    return Promise.reject(error);
  }
);

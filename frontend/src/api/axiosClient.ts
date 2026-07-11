import axios from 'axios';
import { useAuthStore } from '../store/authStore';

// Every api/*.ts call site already prefixes its path with /api/... (and /uploads/...
// for images), which nginx/the Vite dev proxy route to the backend. baseURL is only
// needed when talking to a backend on a different origin (VITE_API_BASE_URL set);
// otherwise it must stay empty or paths end up double-prefixed (/api/api/...).
const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '',
});

axiosClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default axiosClient;

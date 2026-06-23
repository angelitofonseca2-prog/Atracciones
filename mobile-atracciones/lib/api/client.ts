import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';
import * as SecureStore from 'expo-secure-store';
import { API_URL } from '@/constants/Config';

export const TOKEN_KEY = 'auth_token';

let _onUnauthorized: (() => void) | null = null;

export function setUnauthorizedHandler(fn: () => void) {
  _onUnauthorized = fn;
}

function newCorrelationId(): string {
  return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

const apiClient: AxiosInstance = axios.create({
  baseURL: API_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor: inyecta JWT y X-Correlation-ID
apiClient.interceptors.request.use(async (config) => {
  const token = await SecureStore.getItemAsync(TOKEN_KEY);
  if (token) {
    config.headers = config.headers ?? {};
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  config.headers = config.headers ?? {};
  config.headers['X-Correlation-ID'] = newCorrelationId();
  return config;
});

// Response interceptor: maneja 401 → logout
apiClient.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error?.response?.status === 401) {
      SecureStore.deleteItemAsync(TOKEN_KEY).catch(() => {});
      _onUnauthorized?.();
    }
    return Promise.reject(error);
  },
);

export default apiClient;

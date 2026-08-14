import axios from 'axios';
import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';
import { router } from 'expo-router';

const API_URL = process.env.EXPO_PUBLIC_API_URL || 'http://localhost:5000';
const TOKEN_KEY = 'controlefacil_token';

export const api = axios.create({ baseURL: API_URL });

// expo-secure-store não tem implementação web (só iOS/Android). No target real
// (mobile) o token sempre passa pelo Keychain/Keystore via SecureStore, como a
// especificação exige; o fallback para localStorage só existe para permitir rodar
// `expo start --web` em desenvolvimento — nunca é o caminho usado em iOS/Android.
export const tokenStorage = Platform.OS === 'web'
  ? {
    get: async () => (typeof localStorage !== 'undefined' ? localStorage.getItem(TOKEN_KEY) : null),
    set: async (token) => { if (typeof localStorage !== 'undefined') localStorage.setItem(TOKEN_KEY, token); },
    remove: async () => { if (typeof localStorage !== 'undefined') localStorage.removeItem(TOKEN_KEY); },
  }
  : {
    get: () => SecureStore.getItemAsync(TOKEN_KEY),
    set: (token) => SecureStore.setItemAsync(TOKEN_KEY, token),
    remove: () => SecureStore.deleteItemAsync(TOKEN_KEY),
  };

api.interceptors.request.use(async (config) => {
  const token = await tokenStorage.get();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401 && error.config?.url !== '/api/auth/login') {
      await tokenStorage.remove();
      router.replace('/login');
    }
    return Promise.reject(error);
  }
);

export default api;

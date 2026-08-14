import { createContext, useContext, useEffect, useState } from 'react';
import { router } from 'expo-router';
import { authService } from '@/services/authService';
import { tokenStorage } from '@/services/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      const token = await tokenStorage.get();
      if (!token) {
        setLoading(false);
        return;
      }
      try {
        const res = await authService.getCurrentUser();
        setUser(res.data);
      } catch {
        await tokenStorage.remove();
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const login = async (token) => {
    await tokenStorage.set(token);
    const res = await authService.getCurrentUser();
    setUser(res.data);
    router.replace('/transactions');
  };

  const logout = async () => {
    await tokenStorage.remove();
    setUser(null);
    router.replace('/login');
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}

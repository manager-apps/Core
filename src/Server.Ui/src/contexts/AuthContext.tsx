import { createContext, useContext, useState, type ReactNode } from 'react';

interface User {
  name: string;
  email: string;
  avatarUrl?: string | null;
}

interface AuthContextValue {
  token: string | null;
  user: User | null;
  login: (token: string, user: User) => void;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextValue>(null!);

const STORAGE_KEY = 'manager_auth';

export function AuthProvider({ children }: { children: ReactNode }) {
  const getStoredAuth = () => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  };

  const initial = getStoredAuth();
  const [token, setToken] = useState<string | null>(initial?.token ?? null);
  const [user, setUser] = useState<User | null>(initial?.user ?? null);

  const login = (token: string, user: User) => {
    setToken(token);
    setUser(user);
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ token, user }));
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem(STORAGE_KEY);
  };

  return (
    <AuthContext.Provider value={{ token, user, login, logout, isAuthenticated: !!token }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);

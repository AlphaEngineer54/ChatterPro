import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { setToken as persistToken, getToken } from '../api/client';
import * as authApi from '../api/auth';

const USER_KEY = 'chatterpro.user';

const AuthContext = createContext(null);

function loadStoredUser() {
  try {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadStoredUser);
  const [token, setTokenState] = useState(() => getToken());

  const applySession = useCallback((nextUser, nextToken) => {
    setUser(nextUser);
    setTokenState(nextToken);
    persistToken(nextToken);
    if (nextUser) localStorage.setItem(USER_KEY, JSON.stringify(nextUser));
    else localStorage.removeItem(USER_KEY);
  }, []);

  const logout = useCallback(() => {
    applySession(null, null);
  }, [applySession]);

  const login = useCallback(
    async (credentials) => {
      const { user: u, jwtToken } = await authApi.login(credentials);
      applySession(u, jwtToken);
      return u;
    },
    [applySession]
  );

  const signup = useCallback(
    async (payload) => {
      const { user: u, jwtToken } = await authApi.signUp(payload);
      applySession(u, jwtToken);
      return u;
    },
    [applySession]
  );

  // Déconnexion automatique si le backend répond 401 (token expiré/invalide).
  useEffect(() => {
    const handler = () => logout();
    window.addEventListener('auth:unauthorized', handler);
    return () => window.removeEventListener('auth:unauthorized', handler);
  }, [logout]);

  const value = useMemo(
    () => ({ user, token, isAuthenticated: Boolean(token), login, signup, logout }),
    [user, token, login, signup, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth doit être utilisé dans un <AuthProvider>');
  return ctx;
}

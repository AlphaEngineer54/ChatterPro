import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { setToken as persistToken, getToken } from '../api/client';
import * as authApi from '../api/auth';

const USER_KEY = 'chatterpro.user';

const AuthContext = createContext(null);

// Garantit que l'id est bien un entier (le backend peut le renvoyer en string
// selon la sérialisation) → évite tout souci de parsing côté requêtes/URL.
function normalizeUser(user) {
  if (!user) return null;
  const id = Number(user.id);
  return { ...user, id: Number.isNaN(id) ? user.id : id };
}

function loadStoredUser() {
  try {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? normalizeUser(JSON.parse(raw)) : null;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadStoredUser);
  const [token, setTokenState] = useState(() => getToken());

  const applySession = useCallback((nextUser, nextToken) => {
    const normalized = normalizeUser(nextUser);
    setUser(normalized);
    setTokenState(nextToken);
    persistToken(nextToken);
    if (normalized) localStorage.setItem(USER_KEY, JSON.stringify(normalized));
    else localStorage.removeItem(USER_KEY);
  }, []);

  const logout = useCallback(() => {
    applySession(null, null);
  }, [applySession]);

  // Synchronise le profil local (email/pseudo) après une mise à jour réussie,
  // sans toucher au token : évite d'afficher des infos périmées jusqu'à la
  // reconnexion.
  const updateUser = useCallback((patch) => {
    setUser((prev) => {
      const next = normalizeUser({ ...prev, ...patch });
      if (next) localStorage.setItem(USER_KEY, JSON.stringify(next));
      return next;
    });
  }, []);

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
    () => ({ user, token, isAuthenticated: Boolean(token), login, signup, logout, updateUser }),
    [user, token, login, signup, logout, updateUser]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth doit être utilisé dans un <AuthProvider>');
  return ctx;
}

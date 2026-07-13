import { beforeEach, describe, expect, it, vi } from 'vitest';
import { act, renderHook, waitFor } from '@testing-library/react';

vi.mock('../api/auth', () => ({
  login: vi.fn(),
  signUp: vi.fn(),
}));

import * as authApi from '../api/auth';
import { AuthProvider, useAuth } from './AuthContext.jsx';

const wrapper = ({ children }) => <AuthProvider>{children}</AuthProvider>;

describe('context/AuthContext', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  it('démarre non authentifié', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
  });

  it('login enregistre la session et la persiste dans le localStorage', async () => {
    authApi.login.mockResolvedValue({ user: { id: 1, email: 'a@b.c' }, jwtToken: 'jwt-1' });
    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.login({ email: 'a@b.c', password: 'secret' });
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user).toEqual({ id: 1, email: 'a@b.c' });
    expect(localStorage.getItem('chatterpro.token')).toBe('jwt-1');
    expect(JSON.parse(localStorage.getItem('chatterpro.user'))).toEqual({ id: 1, email: 'a@b.c' });
  });

  it('logout efface la session', async () => {
    authApi.login.mockResolvedValue({ user: { id: 1, email: 'a@b.c' }, jwtToken: 'jwt-1' });
    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.login({ email: 'a@b.c', password: 'secret' });
    });
    act(() => result.current.logout());

    expect(result.current.isAuthenticated).toBe(false);
    expect(localStorage.getItem('chatterpro.token')).toBeNull();
    expect(localStorage.getItem('chatterpro.user')).toBeNull();
  });

  it('se déconnecte automatiquement sur l’événement auth:unauthorized', async () => {
    authApi.login.mockResolvedValue({ user: { id: 1, email: 'a@b.c' }, jwtToken: 'jwt-1' });
    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.login({ email: 'a@b.c', password: 'secret' });
    });
    expect(result.current.isAuthenticated).toBe(true);

    act(() => {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    });

    await waitFor(() => expect(result.current.isAuthenticated).toBe(false));
  });

  it('restaure la session depuis le localStorage au montage', () => {
    localStorage.setItem('chatterpro.token', 'jwt-persisted');
    localStorage.setItem('chatterpro.user', JSON.stringify({ id: 9, email: 'z@z.z' }));

    const { result } = renderHook(() => useAuth(), { wrapper });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user).toEqual({ id: 9, email: 'z@z.z' });
  });
});

import { beforeEach, describe, expect, it, vi } from 'vitest';

// On mocke le wrapper HTTP : login() et signUp() passent tous deux par apiFetch.
vi.mock('./client', () => ({ apiFetch: vi.fn() }));

import { apiFetch } from './client';
import { login, signUp } from './auth';

describe('api/auth', () => {
  beforeEach(() => {
    apiFetch.mockReset();
  });

  it('login appelle POST /auth/login sans authentification', async () => {
    apiFetch.mockResolvedValue({ user: { id: 1, email: 'a@b.c' }, jwtToken: 'jwt' });

    const res = await login({ email: 'a@b.c', password: 'secret' });

    expect(apiFetch).toHaveBeenCalledWith('/auth/login', {
      method: 'POST',
      auth: false,
      body: { email: 'a@b.c', password: 'secret' },
    });
    expect(res.jwtToken).toBe('jwt');
  });

  it('signUp utilise directement le token retourné par /auth/signUp', async () => {
    apiFetch.mockResolvedValueOnce({ user: { id: 2, email: 'x@y.z' }, jwtToken: 'jwt-signup' });

    const res = await signUp({ userName: 'x', email: 'x@y.z', password: 'secret' });

    expect(apiFetch).toHaveBeenCalledTimes(1);
    expect(apiFetch).toHaveBeenCalledWith('/auth/signUp', expect.objectContaining({ method: 'POST' }));
    expect(res.jwtToken).toBe('jwt-signup');
  });

  it('signUp bascule sur login si la réponse ne contient pas de token', async () => {
    apiFetch
      .mockResolvedValueOnce(null) // signUp : corps vide
      .mockResolvedValueOnce({ user: { id: 3, email: 'x@y.z' }, jwtToken: 'jwt-login' }); // login

    const res = await signUp({ userName: 'x', email: 'x@y.z', password: 'secret' });

    expect(apiFetch).toHaveBeenCalledTimes(2);
    expect(apiFetch.mock.calls[0][0]).toBe('/auth/signUp');
    expect(apiFetch.mock.calls[1][0]).toBe('/auth/login');
    expect(res.jwtToken).toBe('jwt-login');
  });
});

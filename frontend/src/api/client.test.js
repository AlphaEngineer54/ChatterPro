import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { apiFetch, ApiError, getToken, setToken } from './client';

/** Fabrique une réponse fetch factice. */
function fakeResponse({ status = 200, ok, body = '' }) {
  return {
    status,
    ok: ok ?? (status >= 200 && status < 300),
    text: () =>
      Promise.resolve(typeof body === 'string' ? body : JSON.stringify(body)),
  };
}

describe('api/client', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('injecte le header Authorization Bearer quand un token est présent', async () => {
    setToken('jwt-123');
    const fetchMock = vi.fn().mockResolvedValue(fakeResponse({ body: { ok: true } }));
    vi.stubGlobal('fetch', fetchMock);

    await apiFetch('/conversation?limit=50');

    const [, options] = fetchMock.mock.calls[0];
    expect(options.headers.Authorization).toBe('Bearer jwt-123');
  });

  it("n'ajoute pas de token quand auth vaut false", async () => {
    setToken('jwt-123');
    const fetchMock = vi.fn().mockResolvedValue(fakeResponse({ body: {} }));
    vi.stubGlobal('fetch', fetchMock);

    await apiFetch('/auth/login', { method: 'POST', auth: false, body: { a: 1 } });

    const [, options] = fetchMock.mock.calls[0];
    expect(options.headers.Authorization).toBeUndefined();
    expect(options.headers['Content-Type']).toBe('application/json');
    expect(options.body).toBe(JSON.stringify({ a: 1 }));
  });

  it('renvoie null sur une réponse vide (204)', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(fakeResponse({ status: 204, body: '' })));
    const result = await apiFetch('/conversation/1', { method: 'DELETE' });
    expect(result).toBeNull();
  });

  it('lève une ApiError avec le message du corps sur réponse non-ok', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(fakeResponse({ status: 409, body: { message: 'Email déjà utilisé' } }))
    );

    await expect(apiFetch('/auth/signUp', { method: 'POST', auth: false, body: {} })).rejects.toMatchObject({
      name: 'ApiError',
      status: 409,
      message: 'Email déjà utilisé',
    });
  });

  it('émet un événement auth:unauthorized sur 401', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(fakeResponse({ status: 401, body: { message: 'expired' } }))
    );
    const handler = vi.fn();
    window.addEventListener('auth:unauthorized', handler);

    await expect(apiFetch('/message')).rejects.toBeInstanceOf(ApiError);
    expect(handler).toHaveBeenCalledOnce();

    window.removeEventListener('auth:unauthorized', handler);
  });

  it('transforme une erreur réseau en ApiError status 0', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));
    await expect(apiFetch('/message')).rejects.toMatchObject({ name: 'ApiError', status: 0 });
  });

  it('setToken / getToken persistent dans le localStorage', () => {
    expect(getToken()).toBeNull();
    setToken('abc');
    expect(getToken()).toBe('abc');
    setToken(null);
    expect(getToken()).toBeNull();
  });
});

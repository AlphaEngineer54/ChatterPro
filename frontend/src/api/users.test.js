import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('./client', () => ({ apiFetch: vi.fn() }));

import { apiFetch } from './client';
import { getUser, getUserByUsername, updateUser } from './users';

describe('api/users', () => {
  beforeEach(() => apiFetch.mockReset());

  it('getUser renvoie null sur 404', async () => {
    apiFetch.mockRejectedValueOnce({ name: 'ApiError', status: 404 });
    await expect(getUser(1)).resolves.toBeNull();
  });

  it('getUser renvoie l’utilisateur trouvé', async () => {
    apiFetch.mockResolvedValue({ id: 1, userName: 'alice' });
    expect(await getUser(1)).toEqual({ id: 1, userName: 'alice' });
    expect(apiFetch).toHaveBeenCalledWith('/user/1');
  });

  it('getUserByUsername encode le pseudo dans l’URL', async () => {
    apiFetch.mockResolvedValue({ id: 2, userName: 'bob smith' });
    await getUserByUsername('bob smith');
    expect(apiFetch).toHaveBeenCalledWith('/user/by-username/bob%20smith');
  });

  it('updateUser envoie un PUT avec id + champs', async () => {
    apiFetch.mockResolvedValue(null);
    await updateUser(5, { userName: 'neo', email: 'n@o.c', password: 'secret' });
    expect(apiFetch).toHaveBeenCalledWith('/user/5', {
      method: 'PUT',
      body: { id: 5, userName: 'neo', email: 'n@o.c', password: 'secret' },
    });
  });
});

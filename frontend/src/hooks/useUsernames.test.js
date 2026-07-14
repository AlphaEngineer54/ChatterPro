import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';

vi.mock('../api/users', () => ({ getUser: vi.fn() }));

import { getUser } from '../api/users';
import { useUsernames, __clearUsernameCache } from './useUsernames';

describe('hooks/useUsernames', () => {
  beforeEach(() => {
    __clearUsernameCache();
    getUser.mockReset();
  });

  it('résout les ids en pseudos via UserService', async () => {
    getUser.mockImplementation((id) =>
      Promise.resolve({ id, userName: String(id) === '1' ? 'alice' : 'bob' })
    );

    const { result } = renderHook(() => useUsernames([1, 2]));

    await waitFor(() => {
      expect(result.current['1']).toBe('alice');
      expect(result.current['2']).toBe('bob');
    });
  });

  it('déduplique les ids et ne fait qu’un appel par utilisateur', async () => {
    getUser.mockResolvedValue({ id: 7, userName: 'carol' });

    const { result } = renderHook(() => useUsernames([7, 7, 7]));

    await waitFor(() => expect(result.current['7']).toBe('carol'));
    expect(getUser).toHaveBeenCalledTimes(1);
  });

  it('accepte la sérialisation PascalCase (UserName)', async () => {
    getUser.mockResolvedValue({ Id: 3, UserName: 'dave' });

    const { result } = renderHook(() => useUsernames([3]));

    await waitFor(() => expect(result.current['3']).toBe('dave'));
  });

  it('ne casse pas si un utilisateur est introuvable (null)', async () => {
    getUser.mockResolvedValue(null);

    const { result } = renderHook(() => useUsernames([99]));

    // On laisse le temps à l'effet de s'exécuter ; l'id reste non résolu.
    await waitFor(() => expect(getUser).toHaveBeenCalledWith('99'));
    expect(result.current['99']).toBeUndefined();
  });

  it('utilise le cache module-level entre deux montages', async () => {
    getUser.mockResolvedValue({ id: 5, userName: 'erin' });

    const first = renderHook(() => useUsernames([5]));
    await waitFor(() => expect(first.result.current['5']).toBe('erin'));
    expect(getUser).toHaveBeenCalledTimes(1);

    // Deuxième montage : l'id est déjà en cache → aucun nouvel appel réseau.
    const second = renderHook(() => useUsernames([5]));
    await waitFor(() => expect(second.result.current['5']).toBe('erin'));
    expect(getUser).toHaveBeenCalledTimes(1);
  });
});

import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('./client', () => ({ apiFetch: vi.fn() }));

import { apiFetch } from './client';
import {
  createConversation,
  getConversation,
  listConversationsByUser,
  leaveConversation,
} from './conversations';

describe('api/conversations', () => {
  beforeEach(() => {
    apiFetch.mockReset();
  });

  it('createConversation poste le titre et le userId', async () => {
    apiFetch.mockResolvedValue({ id: 10, title: 'Team', joinCode: 'ABCD' });

    const res = await createConversation({ title: 'Team', userId: 7 });

    expect(apiFetch).toHaveBeenCalledWith('/conversation', {
      method: 'POST',
      body: { title: 'Team', userId: 7 },
    });
    expect(res.id).toBe(10);
  });

  it('listConversationsByUser passe le limit obligatoire dans l’URL', async () => {
    apiFetch.mockResolvedValue([{ id: 1 }]);

    await listConversationsByUser(7);

    expect(apiFetch).toHaveBeenCalledWith('/conversation/by-user-id/7?limit=50');
  });

  it('listConversationsByUser renvoie [] quand le backend répond 404', async () => {
    apiFetch.mockRejectedValue({ name: 'ApiError', status: 404 });

    const res = await listConversationsByUser(99);

    expect(res).toEqual([]);
  });

  it('listConversationsByUser propage les erreurs non-404', async () => {
    apiFetch.mockRejectedValue({ name: 'ApiError', status: 500 });
    await expect(listConversationsByUser(1)).rejects.toMatchObject({ status: 500 });
  });

  it('getConversation charge l’historique avec un limit', async () => {
    apiFetch.mockResolvedValue({ id: 5, messages: [] });
    await getConversation(5);
    expect(apiFetch).toHaveBeenCalledWith('/conversation/5?limit=50');
  });

  it('leaveConversation envoie un DELETE sur /members/{userId}', async () => {
    apiFetch.mockResolvedValue(null);
    await leaveConversation(5, 7);
    expect(apiFetch).toHaveBeenCalledWith('/conversation/5/members/7', { method: 'DELETE' });
  });
});

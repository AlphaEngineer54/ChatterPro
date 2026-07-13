import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('./client', () => ({ apiFetch: vi.fn() }));

import { apiFetch } from './client';
import { updateMessage, deleteMessage } from './messages';

describe('api/messages', () => {
  beforeEach(() => apiFetch.mockReset());

  it('updateMessage envoie un PUT avec le DTO attendu', async () => {
    apiFetch.mockResolvedValue({ id: 3, content: 'edité' });

    await updateMessage(3, { content: 'edité', status: 'sent', senderId: 7, conversationId: 42 });

    expect(apiFetch).toHaveBeenCalledWith('/message/3', {
      method: 'PUT',
      body: { content: 'edité', status: 'sent', senderId: 7, conversationId: 42 },
    });
  });

  it('deleteMessage envoie un DELETE', async () => {
    apiFetch.mockResolvedValue(null);
    await deleteMessage(9);
    expect(apiFetch).toHaveBeenCalledWith('/message/9', { method: 'DELETE' });
  });
});

import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('./client', () => ({ apiFetch: vi.fn() }));

import { apiFetch } from './client';
import {
  listNotifications,
  deleteNotification,
  clearAllNotifications,
} from './notifications';

describe('api/notifications', () => {
  beforeEach(() => apiFetch.mockReset());

  it('listNotifications renvoie [] sur 404', async () => {
    apiFetch.mockRejectedValueOnce({ name: 'ApiError', status: 404 });
    await expect(listNotifications(7)).resolves.toEqual([]);
  });

  it('listNotifications renvoie la liste', async () => {
    apiFetch.mockResolvedValue([{ id: 1, message: 'hi' }]);
    expect(await listNotifications(7)).toHaveLength(1);
    expect(apiFetch).toHaveBeenCalledWith('/notification/7');
  });

  it('deleteNotification cible la bonne route', async () => {
    apiFetch.mockResolvedValue(null);
    await deleteNotification(3);
    expect(apiFetch).toHaveBeenCalledWith('/notification/3', { method: 'DELETE' });
  });

  it('clearAllNotifications cible /all/{userId}', async () => {
    apiFetch.mockResolvedValue(null);
    await clearAllNotifications(7);
    expect(apiFetch).toHaveBeenCalledWith('/notification/all/7', { method: 'DELETE' });
  });
});

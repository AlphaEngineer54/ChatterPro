import { apiFetch } from './client';

/**
 * Liste les notifications d'un utilisateur (REST via la gateway).
 * Renvoie [] si aucune (le backend répond 404 quand la liste est vide).
 */
export async function listNotifications(userId) {
  try {
    return await apiFetch(`/notification/${userId}`);
  } catch (err) {
    if (err.status === 404) return [];
    throw err;
  }
}

/** Supprime une notification par son id. */
export function deleteNotification(notificationId) {
  return apiFetch(`/notification/${notificationId}`, { method: 'DELETE' });
}

/** Supprime toutes les notifications d'un utilisateur. */
export function clearAllNotifications(userId) {
  return apiFetch(`/notification/all/${userId}`, { method: 'DELETE' });
}

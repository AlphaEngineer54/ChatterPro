import { apiFetch } from './client';
import { DEFAULT_LIMIT } from '../config';

/**
 * Crée une nouvelle conversation (groupe).
 * @returns ConversationResponseDTO { id, ownerId, title, date, joinCode }
 */
export function createConversation({ title, userId }) {
  return apiFetch('/conversation', {
    method: 'POST',
    body: { title, userId },
  });
}

/**
 * Liste les conversations auxquelles appartient un utilisateur.
 * `limit` est obligatoire côté backend (sinon 0 → 404), on renvoie [] sur 404.
 * @returns ConversationResponseDTO[]
 */
export async function listConversationsByUser(userId, limit = DEFAULT_LIMIT) {
  try {
    return await apiFetch(`/conversation/by-user-id/${userId}?limit=${limit}`);
  } catch (err) {
    if (err.status === 404) return [];
    throw err;
  }
}

/**
 * Récupère une conversation avec son historique de messages.
 * @returns ConversationResponseWithMessageDTO { id, title, date, joinCode, ownerId, messages: [] }
 */
export function getConversation(conversationId, limit = DEFAULT_LIMIT) {
  return apiFetch(`/conversation/${conversationId}?limit=${limit}`);
}

import { apiFetch } from './client';

/**
 * Met à jour un message existant (REST via la gateway, PUT).
 * @returns MessageResponseDTO
 */
export function updateMessage(messageId, { content, status, senderId, conversationId }) {
  return apiFetch(`/message/${messageId}`, {
    method: 'PUT',
    body: { content, status, senderId, conversationId },
  });
}

/** Supprime un message (204 No Content). */
export function deleteMessage(messageId) {
  return apiFetch(`/message/${messageId}`, { method: 'DELETE' });
}

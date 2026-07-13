import { GATEWAY_URL, DEFAULT_LIMIT } from '../config';
import { getToken, ApiError } from './client';
import { getConversation } from './conversations';

// Le DataExportService valide le statut d'un message avec ^(read|sent|delivred)$.
// On mappe le statut MessageService ("delivered") vers l'orthographe attendue.
function mapStatus(status) {
  if (status === 'delivered') return 'delivred';
  if (status === 'read' || status === 'sent') return status;
  return 'sent';
}

/**
 * Construit le corps Conversation attendu par le DataExportService à partir
 * d'une conversation MessageService (mapping senderId -> userId, statut).
 */
function toExportConversation(conversation) {
  return {
    id: conversation.id,
    title: conversation.title,
    date: conversation.date ?? new Date().toISOString(),
    messages: (conversation.messages ?? []).map((m) => ({
      id: m.id,
      content: m.content,
      date: m.date,
      userId: m.senderId,
      status: mapStatus(m.status),
    })),
  };
}

const EXTENSIONS = { pdf: 'pdf', csv: 'csv', json: 'json' };

/**
 * Exporte une conversation dans le format demandé et déclenche le téléchargement.
 * Récupère au besoin l'historique complet avant l'export.
 * @param {object} conversation conversation courante (peut déjà contenir messages)
 * @param {'pdf'|'csv'|'json'} format
 */
export async function exportConversation(conversation, format) {
  // S'assurer d'avoir les messages (recharge si absent).
  let full = conversation;
  if (!full.messages) {
    full = await getConversation(conversation.id, DEFAULT_LIMIT);
  }

  const body = toExportConversation(full);
  const token = getToken();

  const response = await fetch(`${GATEWAY_URL}/dataexport/export-data?option=${format}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify(body),
  });

  if (response.status === 401) {
    window.dispatchEvent(new CustomEvent('auth:unauthorized'));
  }
  if (!response.ok) {
    throw new ApiError(`Échec de l'export (${response.status})`, response.status, null);
  }

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `conversation-${conversation.id}.${EXTENSIONS[format] ?? 'txt'}`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

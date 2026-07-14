import { useState } from 'react';

function formatTime(dateStr) {
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleString('fr-FR', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/**
 * Bulle de message : alignée à droite (accent) si émise, à gauche sinon.
 * Les messages émis exposent au survol des actions Éditer / Supprimer.
 */
export default function MessageBubble({ message, isOwn, onEdit, onDelete }) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(message.content);
  const [busy, setBusy] = useState(false);

  async function saveEdit() {
    const content = draft.trim();
    if (!content || content === message.content) {
      setEditing(false);
      return;
    }
    setBusy(true);
    try {
      await onEdit(message, content);
      setEditing(false);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className={`group flex ${isOwn ? 'justify-end' : 'justify-start'}`}>
      <div className="flex max-w-[75%] flex-col">
        <div
          className={`rounded-2xl px-4 py-2 text-[15px] shadow-sm ${
            isOwn
              ? 'rounded-br-md bg-accent-600 text-white'
              : 'rounded-bl-md bg-slate-100 text-slate-800 dark:bg-slate-800 dark:text-slate-100'
          }`}
        >
          {!isOwn && (
            <p className="mb-0.5 text-xs font-medium text-slate-500 dark:text-slate-400">
              Utilisateur #{message.senderId}
            </p>
          )}

          {editing ? (
            <div className="space-y-2">
              <textarea
                className="w-full rounded-lg bg-white/90 px-2 py-1 text-slate-800 focus:outline-none"
                rows={2}
                maxLength={250}
                value={draft}
                onChange={(e) => setDraft(e.target.value)}
                autoFocus
              />
              <div className="flex justify-end gap-2 text-xs">
                <button
                  type="button"
                  className="rounded px-2 py-1 text-white/90 hover:bg-white/20"
                  onClick={() => {
                    setDraft(message.content);
                    setEditing(false);
                  }}
                  disabled={busy}
                >
                  Annuler
                </button>
                <button
                  type="button"
                  className="rounded bg-white px-2 py-1 font-medium text-accent-700 hover:bg-slate-100"
                  onClick={saveEdit}
                  disabled={busy}
                >
                  {busy ? '…' : 'Enregistrer'}
                </button>
              </div>
            </div>
          ) : (
            <p className="whitespace-pre-wrap break-words">{message.content}</p>
          )}

          <p
            className={`mt-1 text-right text-[11px] ${
              isOwn ? 'text-accent-100' : 'text-slate-400'
            }`}
          >
            {formatTime(message.date)}
          </p>
        </div>

        {isOwn && !editing && (onEdit || onDelete) && (
          <div className="mt-0.5 flex justify-end gap-3 pr-1 text-[11px] text-slate-400 opacity-0 transition group-hover:opacity-100">
            {onEdit && (
              <button type="button" className="hover:text-slate-600" onClick={() => setEditing(true)}>
                Éditer
              </button>
            )}
            {onDelete && (
              <button type="button" className="hover:text-red-500" onClick={() => onDelete(message)}>
                Supprimer
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

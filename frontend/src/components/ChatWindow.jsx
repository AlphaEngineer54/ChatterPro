import { useEffect, useRef, useState } from 'react';
import MessageBubble from './MessageBubble.jsx';
import MessageComposer from './MessageComposer.jsx';
import ExportMenu from './ExportMenu.jsx';

/** Panneau principal : en-tête de la conversation, fil de messages, composer. */
export default function ChatWindow({
  conversation,
  currentUserId,
  loading,
  onSend,
  onExport,
  onEditMessage,
  onDeleteMessage,
  onLeave,
}) {
  const bottomRef = useRef(null);
  const [copied, setCopied] = useState(false);

  const messages = conversation?.messages ?? [];

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages.length, conversation?.id]);

  async function copyJoinCode() {
    if (!conversation?.joinCode) return;
    try {
      await navigator.clipboard.writeText(conversation.joinCode);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      /* clipboard indisponible : on ignore silencieusement */
    }
  }

  if (!conversation) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center bg-slate-50 text-center dark:bg-slate-950">
        <div className="max-w-sm px-6">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-accent-50 text-2xl dark:bg-accent-600/15">
            💬
          </div>
          <h2 className="text-lg font-semibold text-slate-800 dark:text-slate-100">Bienvenue sur ChatterPro</h2>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
            Sélectionnez une conversation à gauche, créez-en une nouvelle ou rejoignez un
            groupe avec un code d'invitation.
          </p>
        </div>
      </div>
    );
  }

  let body;
  if (loading) {
    body = <p className="py-8 text-center text-sm text-slate-400 dark:text-slate-500">Chargement des messages…</p>;
  } else if (messages.length === 0) {
    body = (
      <p className="py-8 text-center text-sm text-slate-400 dark:text-slate-500">
        Aucun message pour l'instant. Lancez la conversation !
      </p>
    );
  } else {
    body = messages.map((m) => (
      <MessageBubble
        key={m.id}
        message={m}
        isOwn={m.senderId === currentUserId}
        onEdit={onEditMessage}
        onDelete={onDeleteMessage}
      />
    ));
  }

  return (
    <div className="flex flex-1 flex-col bg-slate-50 dark:bg-slate-950">
      <header className="flex items-center justify-between border-b border-slate-200 bg-white px-6 py-4 dark:border-slate-800 dark:bg-slate-900">
        <div>
          <h2 className="font-semibold text-slate-900 dark:text-slate-100">{conversation.title}</h2>
          <p className="text-xs text-slate-400 dark:text-slate-500">Conversation #{conversation.id}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={copyJoinCode}
            className="btn-ghost text-sm"
            title="Copier le code d'invitation"
          >
            {copied ? 'Copié !' : `Code : ${conversation.joinCode?.slice(0, 9)}…`}
          </button>
          <ExportMenu onExport={onExport} />
          <button
            type="button"
            onClick={() => onLeave?.(conversation)}
            className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm font-medium text-slate-500 transition hover:border-red-200 hover:bg-red-50 hover:text-red-600 dark:border-slate-700 dark:text-slate-400 dark:hover:border-red-500/40 dark:hover:bg-red-500/10 dark:hover:text-red-400"
            title="Quitter la conversation"
          >
            Quitter
          </button>
        </div>
      </header>

      <div className="flex-1 space-y-3 overflow-y-auto px-6 py-4">
        {body}
        <div ref={bottomRef} />
      </div>

      <MessageComposer onSend={onSend} disabled={loading} />
    </div>
  );
}

import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext.jsx';
import { chatHub } from '../realtime/chatHub.js';
import {
  createConversation,
  getConversation,
  listConversationsByUser,
} from '../api/conversations.js';
import { updateMessage, deleteMessage } from '../api/messages.js';
import { exportConversation } from '../api/export.js';
import ConversationSidebar from '../components/ConversationSidebar.jsx';
import ChatWindow from '../components/ChatWindow.jsx';
import NewConversationModal from '../components/NewConversationModal.jsx';
import JoinConversationModal from '../components/JoinConversationModal.jsx';

const JOIN_TIMEOUT_MS = 8000;

export default function ChatPage() {
  const { user, logout } = useAuth();

  const [conversations, setConversations] = useState([]);
  const [active, setActive] = useState(null);
  const [loadingList, setLoadingList] = useState(true);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [modal, setModal] = useState(null); // 'new' | 'join' | null
  const [toast, setToast] = useState(null);

  const flash = useCallback((text) => {
    setToast(text);
    setTimeout(() => setToast(null), 3500);
  }, []);

  const reloadConversations = useCallback(async () => {
    const list = await listConversationsByUser(user.id);
    setConversations(list);
    return list;
  }, [user.id]);

  // Démarrage du hub + abonnements globaux + chargement initial de la liste.
  useEffect(() => {
    let mounted = true;

    const offReceive = chatHub.on('ReceiveMessage', (msg) => {
      // Une seule connexion = un seul groupe actif → le message appartient à la
      // conversation ouverte. On déduplique par id (recouvrement historique).
      setActive((prev) => {
        if (!prev) return prev;
        if (prev.messages?.some((m) => m.id === msg.id)) return prev;
        return { ...prev, messages: [...(prev.messages ?? []), msg] };
      });
    });
    const offError = chatHub.on('Error', (text) => flash(String(text)));
    const offValidation = chatHub.on('ValidationError', (errors) =>
      flash(Array.isArray(errors) ? errors.join(' ') : String(errors))
    );

    chatHub.start().catch(() =>
      flash("Connexion temps réel indisponible (MessageService / hub sur 5003).")
    );

    (async () => {
      try {
        await reloadConversations();
      } catch (err) {
        if (mounted) flash(err.message);
      } finally {
        if (mounted) setLoadingList(false);
      }
    })();

    return () => {
      mounted = false;
      offReceive();
      offError();
      offValidation();
      chatHub.stop();
    };
  }, [reloadConversations, flash]);

  const openConversation = useCallback(
    async (conversation) => {
      setLoadingMessages(true);
      setActive({ ...conversation, messages: [] });
      try {
        const detailed = await getConversation(conversation.id);
        setActive({ ...detailed, messages: detailed.messages ?? [] });
        await chatHub.switchToGroup(conversation.id);
      } catch (err) {
        flash(err.message);
      } finally {
        setLoadingMessages(false);
      }
    },
    [flash]
  );

  const handleCreate = useCallback(
    async (title) => {
      const conversation = await createConversation({ title, userId: user.id });
      await reloadConversations();
      return conversation;
    },
    [user.id, reloadConversations]
  );

  // Rejoindre via code : le hub répond par JoinedGroup (succès) ou Error (échec),
  // on transforme cet aller-retour événementiel en promesse.
  const handleJoin = useCallback(
    (joinCode) =>
      new Promise((resolve, reject) => {
        let offJoined, offError, offValidation, timer;
        const cleanup = () => {
          clearTimeout(timer);
          offJoined?.();
          offError?.();
          offValidation?.();
        };
        offJoined = chatHub.on('JoinedGroup', (conv) => {
          cleanup();
          resolve(conv);
        });
        offError = chatHub.on('Error', (msg) => {
          cleanup();
          reject(new Error(String(msg)));
        });
        offValidation = chatHub.on('ValidationError', (errors) => {
          cleanup();
          reject(new Error(Array.isArray(errors) ? errors.join(' ') : String(errors)));
        });
        timer = setTimeout(() => {
          cleanup();
          reject(new Error('Délai dépassé, réessayez.'));
        }, JOIN_TIMEOUT_MS);

        chatHub
          .start()
          .then(() => chatHub.joinByCode({ userId: user.id, joinCode }))
          .catch((err) => {
            cleanup();
            reject(err);
          });
      }).then(async (conv) => {
        const list = await reloadConversations();
        const joined = list.find((c) => c.id === conv?.id);
        if (joined) openConversation(joined);
        flash('Conversation rejointe.');
      }),
    [user.id, reloadConversations, openConversation, flash]
  );

  const handleSend = useCallback(
    async (content) => {
      if (!active) return;
      try {
        await chatHub.sendToGroup({ content, senderId: user.id, conversationId: active.id });
      } catch (err) {
        flash(err.message || "Échec de l'envoi.");
      }
    },
    [active, user.id, flash]
  );

  // Export de la conversation courante (PDF/CSV/JSON).
  const handleExport = useCallback(
    async (format) => {
      if (!active) return;
      try {
        await exportConversation(active, format);
      } catch (err) {
        flash(err.message || "Échec de l'export.");
      }
    },
    [active, flash]
  );

  // Édition d'un message (REST). Met à jour l'affichage local ; la propagation
  // temps réel aux autres membres n'est pas assurée par le hub actuel.
  const handleEditMessage = useCallback(
    async (message, content) => {
      try {
        await updateMessage(message.id, {
          content,
          status: message.status ?? 'sent',
          senderId: user.id,
          conversationId: active.id,
        });
        setActive((prev) =>
          prev
            ? {
                ...prev,
                messages: prev.messages.map((m) =>
                  m.id === message.id ? { ...m, content } : m
                ),
              }
            : prev
        );
      } catch (err) {
        flash(err.message || "Échec de la modification.");
      }
    },
    [active, user.id, flash]
  );

  // Suppression d'un message (REST) + retrait de l'affichage local.
  const handleDeleteMessage = useCallback(
    async (message) => {
      try {
        await deleteMessage(message.id);
        setActive((prev) =>
          prev ? { ...prev, messages: prev.messages.filter((m) => m.id !== message.id) } : prev
        );
      } catch (err) {
        flash(err.message || 'Échec de la suppression.');
      }
    },
    [flash]
  );

  return (
    <div className="flex h-full">
      <ConversationSidebar
        conversations={conversations}
        activeId={active?.id}
        loading={loadingList}
        user={user}
        onSelect={openConversation}
        onNew={() => setModal('new')}
        onJoin={() => setModal('join')}
        onLogout={logout}
      />

      <ChatWindow
        conversation={active}
        currentUserId={user.id}
        loading={loadingMessages}
        onSend={handleSend}
        onExport={handleExport}
        onEditMessage={handleEditMessage}
        onDeleteMessage={handleDeleteMessage}
      />

      {modal === 'new' && (
        <NewConversationModal onClose={() => setModal(null)} onCreate={handleCreate} />
      )}
      {modal === 'join' && (
        <JoinConversationModal onClose={() => setModal(null)} onJoin={handleJoin} />
      )}

      {toast && (
        <div className="fixed bottom-6 left-1/2 z-[60] -translate-x-1/2 rounded-xl bg-slate-900 px-4 py-2.5 text-sm text-white shadow-lg">
          {toast}
        </div>
      )}
    </div>
  );
}

import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { CHAT_HUB_URL } from '../config';
import { getToken } from '../api/client';

/**
 * Enveloppe autour d'une connexion SignalR unique vers le ChatHub de MessageService.
 *
 * Le hub n'est pas authentifié : l'identité de l'utilisateur est transmise
 * explicitement dans les DTO (senderId, userId). La connexion se fait en direct
 * sur MessageService (5003) car Ocelot ne route pas les WebSockets.
 */
class ChatHubClient {
  constructor() {
    this.connection = null;
    this._starting = null;
  }

  _build() {
    this.connection = new HubConnectionBuilder()
      // Le JWT est transmis à la connexion (cohérent avec l'usage obligatoire
      // du token côté backend) via la query string access_token gérée par SignalR.
      .withUrl(CHAT_HUB_URL, { accessTokenFactory: () => getToken() ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();
  }

  /** Démarre la connexion (idempotent). */
  async start() {
    if (!this.connection) this._build();
    if (this.connection.state === HubConnectionState.Connected) return;
    if (this._starting) return this._starting;

    this._starting = this.connection
      .start()
      .finally(() => {
        this._starting = null;
      });
    return this._starting;
  }

  async stop() {
    if (this.connection) {
      await this.connection.stop();
    }
  }

  isConnected() {
    return this.connection?.state === HubConnectionState.Connected;
  }

  /** Abonnement à un événement serveur → renvoie une fonction de désabonnement. */
  on(event, handler) {
    if (!this.connection) this._build();
    this.connection.on(event, handler);
    return () => this.connection?.off(event, handler);
  }

  /** Rejoint le groupe temps réel d'une conversation déjà connue (par son id). */
  connectToGroup(conversationId) {
    return this.connection.invoke('ConnectToGroup', Number(conversationId));
  }

  /**
   * Bascule sur une conversation en garantissant l'appartenance à un seul groupe.
   * L'événement ReceiveMessage ne porte pas le conversationId : on repart donc
   * d'une connexion propre (les appartenances aux groupes sont liées au
   * connectionId) pour éviter tout mélange de messages entre conversations.
   */
  async switchToGroup(conversationId) {
    if (this.connection && this.connection.state === HubConnectionState.Connected) {
      await this.connection.stop();
    }
    await this.start();
    await this.connectToGroup(conversationId);
  }

  /** Rejoint une conversation via son code d'invitation. */
  joinByCode({ userId, joinCode }) {
    return this.connection.invoke('JoinGroup', { userId, joinCode });
  }

  /**
   * Envoie un message à tous les membres d'une conversation.
   * receiverId est requis par le DTO même en groupe → placeholder 0.
   */
  sendToGroup({ content, senderId, conversationId }) {
    return this.connection.invoke('SendMessageToGroup', {
      content,
      status: 'sent',
      senderId,
      receiverId: 0,
      conversationId,
    });
  }
}

// Singleton partagé par toute l'application.
export const chatHub = new ChatHubClient();

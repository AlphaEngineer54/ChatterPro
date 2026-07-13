import { beforeEach, describe, expect, it, vi } from 'vitest';

// Connexion factice partagée, renvoyée par le builder mocké.
const fakeConnection = {
  state: 'Disconnected',
  start: vi.fn().mockResolvedValue(undefined),
  stop: vi.fn().mockResolvedValue(undefined),
  on: vi.fn(),
  off: vi.fn(),
  invoke: vi.fn().mockResolvedValue(undefined),
};

vi.mock('@microsoft/signalr', () => {
  const builder = {
    withUrl: vi.fn().mockReturnThis(),
    withAutomaticReconnect: vi.fn().mockReturnThis(),
    configureLogging: vi.fn().mockReturnThis(),
    build: vi.fn(() => fakeConnection),
  };
  return {
    HubConnectionState: { Disconnected: 'Disconnected', Connected: 'Connected' },
    LogLevel: { Warning: 3 },
    HubConnectionBuilder: vi.fn(() => builder),
  };
});

import { chatHub } from './chatHub';

describe('realtime/chatHub', () => {
  beforeEach(() => {
    // Réinitialise le singleton et les espions entre les tests.
    chatHub.connection = null;
    chatHub._starting = null;
    fakeConnection.state = 'Disconnected';
    fakeConnection.start.mockClear();
    fakeConnection.stop.mockClear();
    fakeConnection.on.mockClear();
    fakeConnection.off.mockClear();
    fakeConnection.invoke.mockClear();
  });

  it('start() démarre la connexion', async () => {
    await chatHub.start();
    expect(fakeConnection.start).toHaveBeenCalledOnce();
  });

  it('on() enregistre un handler et renvoie une fonction de désabonnement', () => {
    const handler = vi.fn();
    const off = chatHub.on('ReceiveMessage', handler);

    expect(fakeConnection.on).toHaveBeenCalledWith('ReceiveMessage', handler);
    off();
    expect(fakeConnection.off).toHaveBeenCalledWith('ReceiveMessage', handler);
  });

  it('sendToGroup() envoie un NewMessageDTO valide (status "sent", receiverId 0)', async () => {
    await chatHub.start();
    await chatHub.sendToGroup({ content: 'salut', senderId: 7, conversationId: 42 });

    expect(fakeConnection.invoke).toHaveBeenCalledWith('SendMessageToGroup', {
      content: 'salut',
      status: 'sent',
      senderId: 7,
      receiverId: 0,
      conversationId: 42,
    });
  });

  it('joinByCode() invoque JoinGroup avec userId et joinCode', async () => {
    await chatHub.start();
    await chatHub.joinByCode({ userId: 7, joinCode: 'ABCD-1234' });

    expect(fakeConnection.invoke).toHaveBeenCalledWith('JoinGroup', {
      userId: 7,
      joinCode: 'ABCD-1234',
    });
  });

  it('connectToGroup() invoque ConnectToGroup avec un id numérique', async () => {
    await chatHub.start();
    await chatHub.connectToGroup('42');

    expect(fakeConnection.invoke).toHaveBeenCalledWith('ConnectToGroup', 42);
  });

  it('switchToGroup() arrête une connexion active avant de rejoindre le nouveau groupe', async () => {
    await chatHub.start();
    fakeConnection.state = 'Connected';

    await chatHub.switchToGroup(5);

    expect(fakeConnection.stop).toHaveBeenCalledOnce();
    expect(fakeConnection.invoke).toHaveBeenCalledWith('ConnectToGroup', 5);
  });
});

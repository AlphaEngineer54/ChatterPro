import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import MessageBubble from './MessageBubble.jsx';

const baseMessage = {
  id: 1,
  content: 'Bonjour tout le monde',
  date: '2026-07-13T10:30:00',
  senderId: 7,
  status: 'sent',
};

describe('MessageBubble', () => {
  it('affiche le contenu du message', () => {
    render(<MessageBubble message={baseMessage} isOwn={false} />);
    expect(screen.getByText('Bonjour tout le monde')).toBeInTheDocument();
  });

  it('affiche l’expéditeur (repli sur l’id) pour un message reçu', () => {
    render(<MessageBubble message={baseMessage} isOwn={false} />);
    expect(screen.getByText('Utilisateur #7')).toBeInTheDocument();
  });

  it('affiche le pseudo (UserService) quand il est fourni', () => {
    render(<MessageBubble message={baseMessage} isOwn={false} senderName="alice" />);
    expect(screen.getByText('alice')).toBeInTheDocument();
    expect(screen.queryByText('Utilisateur #7')).not.toBeInTheDocument();
  });

  it('masque l’expéditeur pour un message émis par soi', () => {
    render(<MessageBubble message={baseMessage} isOwn={true} />);
    expect(screen.queryByText('Utilisateur #7')).not.toBeInTheDocument();
  });
});

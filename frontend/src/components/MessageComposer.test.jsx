import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import MessageComposer from './MessageComposer.jsx';

describe('MessageComposer', () => {
  it('envoie le message (nettoyé) à la touche Entrée puis vide le champ', async () => {
    const user = userEvent.setup();
    const onSend = vi.fn();
    render(<MessageComposer onSend={onSend} disabled={false} />);

    const textarea = screen.getByPlaceholderText('Écrire un message…');
    await user.type(textarea, '  hello  ');
    await user.keyboard('{Enter}');

    expect(onSend).toHaveBeenCalledWith('hello');
    expect(textarea).toHaveValue('');
  });

  it("n'envoie pas avec Shift+Entrée (retour à la ligne)", async () => {
    const user = userEvent.setup();
    const onSend = vi.fn();
    render(<MessageComposer onSend={onSend} disabled={false} />);

    const textarea = screen.getByPlaceholderText('Écrire un message…');
    await user.type(textarea, 'ligne1');
    await user.keyboard('{Shift>}{Enter}{/Shift}');

    expect(onSend).not.toHaveBeenCalled();
  });

  it("n'envoie pas un message vide via le bouton", async () => {
    const user = userEvent.setup();
    const onSend = vi.fn();
    render(<MessageComposer onSend={onSend} disabled={false} />);

    // Bouton désactivé tant que le champ est vide.
    const button = screen.getByRole('button', { name: 'Envoyer' });
    expect(button).toBeDisabled();
    await user.click(button);
    expect(onSend).not.toHaveBeenCalled();
  });

  it('est désactivé quand aucune conversation n’est sélectionnée', () => {
    render(<MessageComposer onSend={vi.fn()} disabled={true} />);
    expect(screen.getByPlaceholderText('Sélectionnez une conversation…')).toBeDisabled();
  });
});

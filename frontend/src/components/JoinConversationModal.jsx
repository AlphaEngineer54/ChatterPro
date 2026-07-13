import { useState } from 'react';
import Modal from './Modal.jsx';

/** Rejoint une conversation existante via son code d'invitation (hub JoinGroup). */
export default function JoinConversationModal({ onClose, onJoin }) {
  const [joinCode, setJoinCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await onJoin(joinCode.trim().toUpperCase());
      onClose();
    } catch (err) {
      setError(err.message || 'Impossible de rejoindre cette conversation.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <Modal title="Rejoindre une conversation" onClose={onClose}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="mb-1.5 block text-sm font-medium text-slate-700">
            Code d'invitation
          </label>
          <input
            type="text"
            className="input font-mono"
            placeholder="XXXX-XXXX-XXXX"
            value={joinCode}
            onChange={(e) => setJoinCode(e.target.value)}
            maxLength={40}
            required
            autoFocus
          />
          <p className="mt-1 text-xs text-slate-400">
            Demandez le code au créateur de la conversation.
          </p>
        </div>

        {error && (
          <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-600">{error}</p>
        )}

        <button type="submit" className="btn-primary w-full" disabled={loading || !joinCode.trim()}>
          {loading ? 'Connexion…' : 'Rejoindre'}
        </button>
      </form>
    </Modal>
  );
}

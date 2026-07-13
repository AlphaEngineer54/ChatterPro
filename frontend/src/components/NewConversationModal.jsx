import { useState } from 'react';
import Modal from './Modal.jsx';

/** Crée une conversation puis affiche le code d'invitation à partager. */
export default function NewConversationModal({ onClose, onCreate }) {
  const [title, setTitle] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [created, setCreated] = useState(null);
  const [copied, setCopied] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const conversation = await onCreate(title.trim());
      setCreated(conversation);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function copyCode() {
    try {
      await navigator.clipboard.writeText(created.joinCode);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      /* ignore */
    }
  }

  return (
    <Modal title="Nouvelle conversation" onClose={onClose}>
      {created ? (
        <div className="space-y-4">
          <p className="text-sm text-slate-600">
            La conversation <span className="font-medium">« {created.title} »</span> a été
            créée. Partagez ce code d'invitation pour que d'autres puissent la rejoindre :
          </p>
          <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5">
            <code className="flex-1 break-all text-sm text-slate-700">{created.joinCode}</code>
            <button type="button" onClick={copyCode} className="btn-ghost px-3 py-1.5 text-xs">
              {copied ? 'Copié !' : 'Copier'}
            </button>
          </div>
          <button type="button" className="btn-primary w-full" onClick={onClose}>
            Terminé
          </button>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700">Titre</label>
            <input
              type="text"
              className="input"
              placeholder="Ex : Équipe projet"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              maxLength={30}
              required
              autoFocus
            />
            <p className="mt-1 text-xs text-slate-400">30 caractères maximum.</p>
          </div>

          {error && (
            <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-600">{error}</p>
          )}

          <button type="submit" className="btn-primary w-full" disabled={loading || !title.trim()}>
            {loading ? 'Création…' : 'Créer'}
          </button>
        </form>
      )}
    </Modal>
  );
}

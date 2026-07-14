import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { getUser, getUserByUsername, updateUser as apiUpdateUser, deleteUser } from '../api/users.js';
import ThemeToggle from '../components/ThemeToggle.jsx';

export default function ProfilePage() {
  const { user, logout, updateUser } = useAuth();
  const navigate = useNavigate();

  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState(user?.email ?? '');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [feedback, setFeedback] = useState(null); // { type, text }

  // Recherche d'utilisateur (tient lieu de "contacts").
  const [search, setSearch] = useState('');
  const [searchResult, setSearchResult] = useState(undefined); // undefined = pas cherché
  const [searching, setSearching] = useState(false);

  // Suppression de compte
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState(null);

  useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        const info = await getUser(user.id);
        if (mounted && info) setUserName(info.userName ?? info.UserName ?? '');
      } catch {
        /* ignore */
      } finally {
        if (mounted) setLoading(false);
      }
    })();
    return () => {
      mounted = false;
    };
  }, [user.id]);

  async function handleSave(e) {
    e.preventDefault();
    setFeedback(null);
    setSaving(true);
    try {
      await apiUpdateUser(user.id, { userName, email, password });
      // Synchronise le client : le pseudo/email affichés (sidebar, en-tête profil)
      // reflètent immédiatement les nouvelles valeurs sans reconnexion.
      updateUser({ userName, email });
      setPassword('');
      setFeedback({ type: 'ok', text: 'Profil mis à jour.' });
    } catch (err) {
      setFeedback({ type: 'error', text: err.message || 'Échec de la mise à jour.' });
    } finally {
      setSaving(false);
    }
  }

  async function handleSearch(e) {
    e.preventDefault();
    const q = search.trim();
    if (!q) return;
    setSearching(true);
    setSearchResult(undefined);
    try {
      const found = await getUserByUsername(q);
      setSearchResult(found);
    } catch {
      setSearchResult(null);
    } finally {
      setSearching(false);
    }
  }

  async function handleDeleteAccount() {
    setDeleteError(null);
    setDeleting(true);
    try {
      await deleteUser(user.id);
      logout();
      navigate('/login', { replace: true });
    } catch (err) {
      setDeleteError(err.message || 'Échec de la suppression du compte.');
      setDeleting(false);
    }
  }

  return (
    <div className="min-h-full bg-slate-50 dark:bg-slate-950">
      <header className="border-b border-slate-200 bg-white px-6 py-4 dark:border-slate-800 dark:bg-slate-900">
        <div className="mx-auto flex max-w-2xl items-center justify-between">
          <h1 className="text-lg font-semibold text-slate-900 dark:text-slate-100">Mon profil</h1>
          <div className="flex items-center gap-2">
            <ThemeToggle />
            <Link to="/chat" className="btn-ghost text-sm">
              ← Retour au chat
            </Link>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-2xl space-y-6 px-6 py-8">
        {/* Carte profil */}
        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-card dark:border-slate-800 dark:bg-slate-900">
          <div className="mb-6 flex items-center gap-4">
            <span className="flex h-14 w-14 items-center justify-center rounded-2xl bg-accent-600 text-xl font-bold text-white">
              {(userName || user?.email || '?').charAt(0).toUpperCase()}
            </span>
            <div>
              <p className="font-semibold text-slate-900 dark:text-slate-100">{userName || '—'}</p>
              <p className="text-sm text-slate-500 dark:text-slate-400">
                #{user?.id} · {user?.email}
              </p>
            </div>
          </div>

          <form onSubmit={handleSave} className="space-y-4">
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Pseudo</label>
              <input
                className="input"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                maxLength={50}
                required
                disabled={loading}
              />
            </div>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Email</label>
              <input
                type="email"
                className="input"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
                Mot de passe (requis pour enregistrer)
              </label>
              <input
                type="password"
                className="input"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Confirmez avec votre mot de passe"
                minLength={5}
                maxLength={50}
                required
              />
            </div>

            {feedback && (
              <p
                className={`rounded-xl px-3 py-2 text-sm ${
                  feedback.type === 'ok'
                    ? 'bg-green-50 text-green-700 dark:bg-green-500/10 dark:text-green-400'
                    : 'bg-red-50 text-red-600 dark:bg-red-500/10 dark:text-red-400'
                }`}
              >
                {feedback.text}
              </p>
            )}

            <button type="submit" className="btn-primary" disabled={saving}>
              {saving ? 'Enregistrement…' : 'Enregistrer'}
            </button>
          </form>
        </section>

        {/* Recherche d'utilisateurs (contacts) */}
        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-card dark:border-slate-800 dark:bg-slate-900">
          <h2 className="mb-1 font-semibold text-slate-900 dark:text-slate-100">Trouver un utilisateur</h2>
          <p className="mb-4 text-sm text-slate-500 dark:text-slate-400">Recherchez un contact par son pseudo.</p>
          <form onSubmit={handleSearch} className="flex gap-2">
            <input
              className="input"
              placeholder="pseudo exact"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
            <button type="submit" className="btn-ghost" disabled={searching || !search.trim()}>
              {searching ? '…' : 'Rechercher'}
            </button>
          </form>

          {searchResult !== undefined && (
            <div className="mt-4">
              {searchResult ? (
                <div className="flex items-center gap-3 rounded-xl border border-slate-200 px-4 py-3 dark:border-slate-700">
                  <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100 text-sm font-semibold text-slate-600 dark:bg-slate-800 dark:text-slate-300">
                    {(searchResult.userName ?? searchResult.UserName ?? '?').charAt(0).toUpperCase()}
                  </span>
                  <div>
                    <p className="text-sm font-medium text-slate-800 dark:text-slate-200">
                      {searchResult.userName ?? searchResult.UserName}
                    </p>
                    <p className="text-xs text-slate-400 dark:text-slate-500">#{searchResult.id ?? searchResult.Id}</p>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-slate-400 dark:text-slate-500">Aucun utilisateur trouvé.</p>
              )}
            </div>
          )}
        </section>

        {/* Danger zone */}
        <section className="rounded-2xl border border-red-200 bg-red-50/50 p-6 dark:border-red-500/30 dark:bg-red-500/5">
          <h2 className="font-semibold text-red-700 dark:text-red-400">Zone de danger</h2>
          <p className="mt-1 text-sm text-red-600/80 dark:text-red-400/70">
            La suppression de votre compte est <strong>définitive</strong> et irréversible.
          </p>

          {deleteError && (
            <p className="mt-3 rounded-xl bg-red-100 px-3 py-2 text-sm text-red-700 dark:bg-red-500/15 dark:text-red-300">
              {deleteError}
            </p>
          )}

          {!confirmDelete ? (
            <button
              type="button"
              onClick={() => setConfirmDelete(true)}
              className="mt-4 rounded-xl border border-red-300 bg-white px-4 py-2.5 text-sm font-medium text-red-600 transition hover:bg-red-600 hover:text-white dark:border-red-500/40 dark:bg-transparent dark:text-red-400 dark:hover:bg-red-600 dark:hover:text-white"
            >
              Supprimer mon compte
            </button>
          ) : (
            <div className="mt-4 flex flex-wrap items-center gap-3">
              <span className="text-sm font-medium text-red-700 dark:text-red-300">Confirmer la suppression ?</span>
              <button
                type="button"
                onClick={handleDeleteAccount}
                disabled={deleting}
                className="rounded-xl bg-red-600 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-red-700 disabled:opacity-60"
              >
                {deleting ? 'Suppression…' : 'Oui, supprimer définitivement'}
              </button>
              <button
                type="button"
                onClick={() => setConfirmDelete(false)}
                disabled={deleting}
                className="btn-ghost text-sm"
              >
                Annuler
              </button>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

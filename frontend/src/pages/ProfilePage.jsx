import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import { getUser, getUserByUsername, updateUser } from '../api/users.js';

export default function ProfilePage() {
  const { user } = useAuth();

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
      await updateUser(user.id, { userName, email, password });
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

  return (
    <div className="min-h-full bg-slate-50">
      <header className="border-b border-slate-200 bg-white px-6 py-4">
        <div className="mx-auto flex max-w-2xl items-center justify-between">
          <h1 className="text-lg font-semibold text-slate-900">Mon profil</h1>
          <Link to="/chat" className="btn-ghost text-sm">
            ← Retour au chat
          </Link>
        </div>
      </header>

      <main className="mx-auto max-w-2xl space-y-6 px-6 py-8">
        {/* Carte profil */}
        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-card">
          <div className="mb-6 flex items-center gap-4">
            <span className="flex h-14 w-14 items-center justify-center rounded-2xl bg-accent-600 text-xl font-bold text-white">
              {(userName || user?.email || '?').charAt(0).toUpperCase()}
            </span>
            <div>
              <p className="font-semibold text-slate-900">{userName || '—'}</p>
              <p className="text-sm text-slate-500">
                #{user?.id} · {user?.email}
              </p>
            </div>
          </div>

          <form onSubmit={handleSave} className="space-y-4">
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Pseudo</label>
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
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Email</label>
              <input
                type="email"
                className="input"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">
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
                  feedback.type === 'ok' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-600'
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
        <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-card">
          <h2 className="mb-1 font-semibold text-slate-900">Trouver un utilisateur</h2>
          <p className="mb-4 text-sm text-slate-500">Recherchez un contact par son pseudo.</p>
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
                <div className="flex items-center gap-3 rounded-xl border border-slate-200 px-4 py-3">
                  <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100 text-sm font-semibold text-slate-600">
                    {(searchResult.userName ?? searchResult.UserName ?? '?').charAt(0).toUpperCase()}
                  </span>
                  <div>
                    <p className="text-sm font-medium text-slate-800">
                      {searchResult.userName ?? searchResult.UserName}
                    </p>
                    <p className="text-xs text-slate-400">#{searchResult.id ?? searchResult.Id}</p>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-slate-400">Aucun utilisateur trouvé.</p>
              )}
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

import { Link } from 'react-router-dom';
import NotificationBell from './NotificationBell.jsx';
import ThemeToggle from './ThemeToggle.jsx';

const iconBtn =
  'flex h-8 w-8 items-center justify-center rounded-lg text-slate-500 transition ' +
  'hover:bg-slate-100 hover:text-slate-700 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-slate-200';

/** Colonne de gauche : identité, actions et liste des conversations. */
export default function ConversationSidebar({
  conversations,
  activeId,
  loading,
  user,
  onSelect,
  onNew,
  onJoin,
  onLeave,
  onLogout,
}) {
  return (
    <aside className="flex w-80 flex-col border-r border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-900">
      {/* En-tête / identité */}
      <div className="border-b border-slate-200 px-4 py-3 dark:border-slate-800">
        <div className="flex items-center justify-between gap-2">
          <div className="flex min-w-0 items-center gap-2">
            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-accent-600 text-sm font-bold text-white">
              C
            </span>
            <span className="truncate text-sm font-semibold text-slate-900 dark:text-slate-100">
              ChatterPro
            </span>
          </div>
          <div className="flex shrink-0 items-center gap-0.5">
            <ThemeToggle />
            {user?.id != null && <NotificationBell userId={user.id} />}
            <Link to="/profile" className={iconBtn} title="Profil" aria-label="Profil">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.7} stroke="currentColor" className="h-[18px] w-[18px]" aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.501 20.118a7.5 7.5 0 0 1 14.998 0A17.933 17.933 0 0 1 12 21.75c-2.676 0-5.216-.584-7.499-1.632Z" />
              </svg>
            </Link>
            <button type="button" onClick={onLogout} className={iconBtn} title="Déconnexion" aria-label="Déconnexion">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.7} stroke="currentColor" className="h-[18px] w-[18px]" aria-hidden="true">
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6a2.25 2.25 0 0 0-2.25 2.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15m3 0 3-3m0 0-3-3m3 3H9" />
              </svg>
            </button>
          </div>
        </div>
        {user?.email && (
          <p className="mt-1.5 truncate text-xs text-slate-400 dark:text-slate-500">{user.email}</p>
        )}
      </div>

      {/* Actions */}
      <div className="flex gap-2 px-4 py-3">
        <button type="button" onClick={onNew} className="btn-primary flex-1 text-sm">
          + Nouvelle
        </button>
        <button type="button" onClick={onJoin} className="btn-ghost flex-1 text-sm">
          Rejoindre
        </button>
      </div>

      {/* Liste */}
      <div className="flex-1 overflow-y-auto px-2 pb-4">
        {loading ? (
          <p className="px-4 py-6 text-sm text-slate-400 dark:text-slate-500">Chargement…</p>
        ) : conversations.length === 0 ? (
          <p className="px-4 py-6 text-sm text-slate-400 dark:text-slate-500">
            Aucune conversation. Créez-en une pour commencer.
          </p>
        ) : (
          conversations.map((c) => {
            const active = c.id === activeId;
            return (
              <div
                key={c.id}
                className={`group mb-1 flex items-center gap-1 rounded-xl pr-1 transition ${
                  active
                    ? 'bg-accent-50 ring-1 ring-accent-100 dark:bg-accent-600/15 dark:ring-accent-500/30'
                    : 'hover:bg-slate-50 dark:hover:bg-slate-800'
                }`}
              >
                <button
                  type="button"
                  onClick={() => onSelect(c)}
                  className="flex min-w-0 flex-1 items-center gap-3 rounded-xl px-3 py-2.5 text-left"
                >
                  <span
                    className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-sm font-semibold ${
                      active
                        ? 'bg-accent-600 text-white'
                        : 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300'
                    }`}
                  >
                    {c.title?.charAt(0).toUpperCase() || '#'}
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="block truncate text-sm font-medium text-slate-800 dark:text-slate-200">
                      {c.title}
                    </span>
                    <span className="block truncate text-xs text-slate-400 dark:text-slate-500">
                      Conversation #{c.id}
                    </span>
                  </span>
                </button>

                <button
                  type="button"
                  onClick={(e) => {
                    e.stopPropagation();
                    onLeave?.(c);
                  }}
                  className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-slate-400 opacity-0 transition hover:bg-red-50 hover:text-red-500 focus:opacity-100 group-hover:opacity-100 dark:text-slate-500 dark:hover:bg-red-500/10 dark:hover:text-red-400"
                  title="Quitter la conversation"
                  aria-label="Quitter la conversation"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.7} stroke="currentColor" className="h-4 w-4" aria-hidden="true">
                    <path strokeLinecap="round" strokeLinejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.02-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                  </svg>
                </button>
              </div>
            );
          })
        )}
      </div>
    </aside>
  );
}

import { useCallback, useEffect, useRef, useState } from 'react';
import {
  listNotifications,
  deleteNotification,
  clearAllNotifications,
} from '../api/notifications.js';

const POLL_INTERVAL_MS = 20000;

function formatTime(dateStr) {
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleString('fr-FR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
}

/**
 * Cloche de notifications : interroge périodiquement le NotificationService
 * (near real-time par polling — le hub Clients.User ne peut atteindre le
 * navigateur sans IUserIdProvider côté backend).
 */
export default function NotificationBell({ userId }) {
  const [items, setItems] = useState([]);
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  const refresh = useCallback(async () => {
    try {
      const list = await listNotifications(userId);
      // Normalise les champs (Message/message, Id/id, CreatedAt/createdAt).
      setItems(
        list.map((n) => ({
          id: n.id ?? n.Id,
          message: n.message ?? n.Message,
          createdAt: n.createdAt ?? n.CreatedAt,
        }))
      );
    } catch {
      /* silencieux : le service de notifications peut être indisponible */
    }
  }, [userId]);

  useEffect(() => {
    refresh();
    const timer = setInterval(refresh, POLL_INTERVAL_MS);
    return () => clearInterval(timer);
  }, [refresh]);

  useEffect(() => {
    function onClickOutside(e) {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    }
    document.addEventListener('mousedown', onClickOutside);
    return () => document.removeEventListener('mousedown', onClickOutside);
  }, []);

  async function dismiss(id) {
    setItems((prev) => prev.filter((n) => n.id !== id));
    try {
      await deleteNotification(id);
    } catch {
      refresh();
    }
  }

  async function clearAll() {
    setItems([]);
    try {
      await clearAllNotifications(userId);
    } catch {
      refresh();
    }
  }

  const count = items.length;

  return (
    <div className="relative" ref={ref}>
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="relative flex h-8 w-8 items-center justify-center rounded-lg text-slate-500 hover:bg-slate-100"
        title="Notifications"
        aria-label="Notifications"
      >
        <span className="text-lg">🔔</span>
        {count > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-semibold text-white">
            {count > 9 ? '9+' : count}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 z-30 mt-2 w-72 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-lg">
          <div className="flex items-center justify-between border-b border-slate-100 px-3 py-2">
            <span className="text-sm font-semibold text-slate-700">Notifications</span>
            {count > 0 && (
              <button type="button" className="text-xs text-slate-400 hover:text-slate-600" onClick={clearAll}>
                Tout effacer
              </button>
            )}
          </div>
          <div className="max-h-80 overflow-y-auto">
            {count === 0 ? (
              <p className="px-3 py-6 text-center text-sm text-slate-400">Aucune notification.</p>
            ) : (
              items.map((n) => (
                <div key={n.id} className="flex items-start gap-2 border-b border-slate-50 px-3 py-2.5 last:border-0">
                  <div className="min-w-0 flex-1">
                    <p className="break-words text-sm text-slate-700">{n.message}</p>
                    <p className="mt-0.5 text-[11px] text-slate-400">{formatTime(n.createdAt)}</p>
                  </div>
                  <button
                    type="button"
                    className="text-slate-300 hover:text-slate-500"
                    onClick={() => dismiss(n.id)}
                    aria-label="Supprimer"
                  >
                    ✕
                  </button>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}

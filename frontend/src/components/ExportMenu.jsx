import { useEffect, useRef, useState } from 'react';

const FORMATS = [
  { key: 'pdf', label: 'PDF' },
  { key: 'csv', label: 'CSV' },
  { key: 'json', label: 'JSON' },
];

/** Menu déroulant d'export de la conversation courante. */
export default function ExportMenu({ onExport }) {
  const [open, setOpen] = useState(false);
  const [busy, setBusy] = useState(null);
  const ref = useRef(null);

  useEffect(() => {
    function onClickOutside(e) {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    }
    document.addEventListener('mousedown', onClickOutside);
    return () => document.removeEventListener('mousedown', onClickOutside);
  }, []);

  async function handle(format) {
    setBusy(format);
    try {
      await onExport(format);
      setOpen(false);
    } finally {
      setBusy(null);
    }
  }

  return (
    <div className="relative" ref={ref}>
      <button type="button" className="btn-ghost text-sm" onClick={() => setOpen((v) => !v)}>
        Exporter ▾
      </button>
      {open && (
        <div className="absolute right-0 z-20 mt-1 w-40 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-lg dark:border-slate-700 dark:bg-slate-800">
          {FORMATS.map((f) => (
            <button
              key={f.key}
              type="button"
              className="flex w-full items-center justify-between px-4 py-2.5 text-sm text-slate-700 hover:bg-slate-50 disabled:opacity-50 dark:text-slate-200 dark:hover:bg-slate-700"
              onClick={() => handle(f.key)}
              disabled={busy !== null}
            >
              <span>Exporter en {f.label}</span>
              {busy === f.key && <span className="text-xs text-slate-400">…</span>}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

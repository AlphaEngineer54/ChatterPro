import { useState } from 'react';
import { applyTheme, getInitialTheme } from '../theme.js';

/** Bouton de bascule clair / sombre. */
export default function ThemeToggle() {
  const [theme, setTheme] = useState(getInitialTheme);

  function toggle() {
    const next = theme === 'dark' ? 'light' : 'dark';
    applyTheme(next);
    setTheme(next);
  }

  return (
    <button
      type="button"
      onClick={toggle}
      className="flex h-8 w-8 items-center justify-center rounded-lg text-slate-500 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800"
      title={theme === 'dark' ? 'Passer en clair' : 'Passer en sombre'}
      aria-label="Changer de thème"
    >
      <span className="text-lg">{theme === 'dark' ? '☀️' : '🌙'}</span>
    </button>
  );
}

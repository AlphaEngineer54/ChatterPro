const THEME_KEY = 'chatterpro.theme';

/** Renvoie le thème initial : préférence stockée, sinon préférence système. */
export function getInitialTheme() {
  const stored = localStorage.getItem(THEME_KEY);
  if (stored === 'light' || stored === 'dark') return stored;
  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

/** Applique le thème au <html> (classe `dark`) et le persiste. */
export function applyTheme(theme) {
  const root = document.documentElement;
  if (theme === 'dark') root.classList.add('dark');
  else root.classList.remove('dark');
  localStorage.setItem(THEME_KEY, theme);
}

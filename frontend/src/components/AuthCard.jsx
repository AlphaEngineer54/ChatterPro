import { Link } from 'react-router-dom';
import ThemeToggle from './ThemeToggle.jsx';

/** Coquille visuelle des écrans d'authentification : carte centrée. */
export default function AuthCard({ title, subtitle, children, footer }) {
  return (
    <div className="relative flex min-h-full items-center justify-center bg-slate-50 px-4 py-12 dark:bg-slate-950">
      <div className="absolute right-4 top-4">
        <ThemeToggle />
      </div>
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <Link to="/chat" className="inline-flex items-center gap-2">
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent-600 text-lg font-bold text-white">
              C
            </span>
            <span className="text-xl font-semibold text-slate-900 dark:text-slate-100">ChatterPro</span>
          </Link>
        </div>

        <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-card dark:border-slate-800 dark:bg-slate-900">
          <h1 className="text-2xl font-semibold text-slate-900 dark:text-slate-100">{title}</h1>
          {subtitle && <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{subtitle}</p>}
          <div className="mt-6">{children}</div>
        </div>

        {footer && <p className="mt-6 text-center text-sm text-slate-500 dark:text-slate-400">{footer}</p>}
      </div>
    </div>
  );
}

import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import AuthCard from '../components/AuthCard.jsx';
import { useAuth } from '../context/AuthContext.jsx';

export default function SignupPage() {
  const { signup } = useAuth();
  const navigate = useNavigate();
  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await signup({ userName, email, password });
      navigate('/chat', { replace: true });
    } catch (err) {
      if (err.status === 409) setError('Cet email est déjà utilisé.');
      else if (err.status === 400) setError('Vérifiez les champs (pseudo ≥ 3, mot de passe ≥ 5 caractères).');
      else setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthCard
      title="Créer un compte"
      subtitle="Quelques secondes suffisent pour commencer à discuter."
      footer={
        <>
          Déjà inscrit ?{' '}
          <Link to="/login" className="font-medium text-accent-600 hover:text-accent-700">
            Se connecter
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Pseudo</label>
          <input
            type="text"
            className="input"
            placeholder="votre pseudo"
            value={userName}
            onChange={(e) => setUserName(e.target.value)}
            minLength={3}
            maxLength={50}
            required
            autoFocus
          />
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Email</label>
          <input
            type="email"
            className="input"
            placeholder="vous@exemple.com"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Mot de passe</label>
          <input
            type="password"
            className="input"
            placeholder="au moins 5 caractères"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            minLength={5}
            maxLength={50}
            required
          />
        </div>

        {error && (
          <p className="rounded-xl bg-red-50 px-3 py-2 text-sm text-red-600 dark:bg-red-500/10 dark:text-red-400">{error}</p>
        )}

        <button type="submit" className="btn-primary w-full" disabled={loading}>
          {loading ? 'Création…' : 'Créer mon compte'}
        </button>
      </form>
    </AuthCard>
  );
}

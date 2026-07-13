import { apiFetch } from './client';

/**
 * Authentifie l'utilisateur.
 * @returns {{ user: { id: number, email: string }, jwtToken: string }}
 */
export function login({ email, password }) {
  return apiFetch('/auth/login', {
    method: 'POST',
    auth: false,
    body: { email, password },
  });
}

/**
 * Crée un compte. Le backend renvoie le token JWT dans la réponse ({ user, jwtToken }),
 * exactement comme /auth/login. Par sécurité, si la réponse ne contient pas encore
 * le token, on enchaîne un login automatique avec les mêmes identifiants.
 * @returns {{ user: { id: number, email: string }, jwtToken: string }}
 */
export async function signUp({ userName, email, password }) {
  const created = await apiFetch('/auth/signUp', {
    method: 'POST',
    auth: false,
    body: { userName, email, password },
  });

  if (created && created.jwtToken && created.user) {
    return created;
  }

  // Repli défensif si la réponse ne portait pas le token.
  return login({ email, password });
}

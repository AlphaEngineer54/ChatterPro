import { apiFetch } from './client';

/**
 * Récupère un utilisateur par son id.
 * @returns { id, userName } ou null si 404
 */
export async function getUser(userId) {
  try {
    return await apiFetch(`/user/${userId}`);
  } catch (err) {
    if (err.status === 404) return null;
    throw err;
  }
}

/**
 * Recherche un utilisateur par son pseudo (tient lieu d'annuaire/contacts,
 * le UserService n'exposant pas d'endpoint de liste).
 * @returns { id, userName } ou null si introuvable
 */
export async function getUserByUsername(username) {
  try {
    return await apiFetch(`/user/by-username/${encodeURIComponent(username)}`);
  } catch (err) {
    if (err.status === 404) return null;
    throw err;
  }
}

/** Met à jour le profil utilisateur (PUT). Le DTO backend exige userName, email et password. */
export function updateUser(userId, { userName, email, password }) {
  return apiFetch(`/user/${userId}`, {
    method: 'PUT',
    body: { id: userId, userName, email, password },
  });
}

/**
 * Supprime définitivement le compte utilisateur (204).
 * Le UserService publie un événement `user-deleted` → suppression en cascade
 * des identifiants côté AuthService.
 */
export function deleteUser(userId) {
  return apiFetch(`/user/${userId}`, { method: 'DELETE' });
}

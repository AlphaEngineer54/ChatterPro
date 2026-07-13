import { GATEWAY_URL } from '../config';

const TOKEN_KEY = 'chatterpro.token';

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

/** Erreur enrichie du statut HTTP et du corps de réponse. */
export class ApiError extends Error {
  constructor(message, status, body) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.body = body;
  }
}

/**
 * Wrapper fetch centralisé : préfixe l'URL de la gateway, injecte le Bearer JWT,
 * parse le JSON et normalise les erreurs. Sur 401 → émet un event global pour
 * déconnecter l'utilisateur.
 */
export async function apiFetch(path, { method = 'GET', body, auth = true, headers = {} } = {}) {
  const finalHeaders = { ...headers };
  if (body !== undefined) finalHeaders['Content-Type'] = 'application/json';

  const token = getToken();
  if (auth && token) finalHeaders['Authorization'] = `Bearer ${token}`;

  let response;
  try {
    response = await fetch(`${GATEWAY_URL}${path}`, {
      method,
      headers: finalHeaders,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(
      "Impossible de joindre le serveur. Vérifiez que l'API Gateway (5000) est démarrée.",
      0,
      null
    );
  }

  if (response.status === 401) {
    window.dispatchEvent(new CustomEvent('auth:unauthorized'));
  }

  // Corps éventuellement vide (204, ou 201 signUp au corps vide).
  const text = await response.text();
  let data = null;
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = text;
    }
  }

  if (!response.ok) {
    const message =
      (data && (data.message || data.Message)) ||
      `Erreur ${response.status}`;
    throw new ApiError(message, response.status, data);
  }

  return data;
}

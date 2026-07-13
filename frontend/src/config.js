// Configuration centralisée des URLs backend.
// Valeurs surchargées via le fichier .env (préfixe VITE_).
export const GATEWAY_URL =
  import.meta.env.VITE_GATEWAY_URL ?? 'http://localhost:5000';

export const CHAT_HUB_URL =
  import.meta.env.VITE_CHAT_HUB_URL ?? 'http://localhost:5003/hubs/chat';

// Nombre d'éléments demandés par défaut sur les endpoints paginés
// (le backend renvoie 0 résultat si limit est omis).
export const DEFAULT_LIMIT = 50;

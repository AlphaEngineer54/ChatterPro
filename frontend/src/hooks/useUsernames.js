import { useEffect, useMemo, useState } from 'react';
import { getUser } from '../api/users.js';

// Cache module-level (partagé entre montages/onglets de l'app) : un id résolu
// une fois n'est plus re-demandé au UserService. Le UserService n'exposant pas
// d'endpoint « batch », on résout les ids un par un mais on ne le fait qu'une fois.
const cache = new Map(); // id -> userName (string)

// Normalise la réponse du UserService (sérialisation camelCase ou PascalCase).
function readUserName(user) {
  return user?.userName ?? user?.UserName ?? null;
}

/**
 * Résout un ensemble d'ids utilisateur en pseudos via le UserService.
 * @param {Array<number|string>} ids liste (potentiellement redondante) d'ids
 * @returns {Record<string, string>} map { [id]: userName } pour les ids résolus
 */
export function useUsernames(ids) {
  // Clé stable : ids uniques triés → évite de relancer l'effet à chaque rendu
  // quand le contenu logique de la liste n'a pas changé.
  const uniqueKey = useMemo(() => {
    const unique = Array.from(new Set((ids ?? []).filter((id) => id != null).map(String)));
    unique.sort();
    return unique.join(',');
  }, [ids]);

  const [names, setNames] = useState(() => {
    const initial = {};
    for (const id of cache.keys()) initial[id] = cache.get(id);
    return initial;
  });

  useEffect(() => {
    if (!uniqueKey) return;
    let cancelled = false;

    const idsToResolve = uniqueKey.split(',').filter((id) => !cache.has(id));
    if (idsToResolve.length === 0) {
      // Tout est déjà en cache : on s'assure juste que le state reflète le cache.
      setNames((prev) => {
        let changed = false;
        const next = { ...prev };
        for (const id of uniqueKey.split(',')) {
          if (cache.has(id) && next[id] !== cache.get(id)) {
            next[id] = cache.get(id);
            changed = true;
          }
        }
        return changed ? next : prev;
      });
      return;
    }

    (async () => {
      const resolved = await Promise.all(
        idsToResolve.map(async (id) => {
          try {
            const user = await getUser(id);
            const userName = readUserName(user);
            if (userName) cache.set(id, userName);
            return [id, userName];
          } catch {
            return [id, null];
          }
        })
      );
      if (cancelled) return;
      setNames((prev) => {
        const next = { ...prev };
        for (const [id, userName] of resolved) {
          if (userName) next[id] = userName;
        }
        return next;
      });
    })();

    return () => {
      cancelled = true;
    };
  }, [uniqueKey]);

  return names;
}

// Réservé aux tests : vide le cache module-level entre deux cas.
export function __clearUsernameCache() {
  cache.clear();
}

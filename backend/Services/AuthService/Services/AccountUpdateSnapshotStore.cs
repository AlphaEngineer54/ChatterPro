using System.Collections.Concurrent;

namespace AuthService.Services
{
    /// <summary>
    /// Conserve, le temps d'un SAGA, l'état d'origine nécessaire à la compensation
    /// (email avant modification), indexé par SagaId.
    ///
    /// Limite assumée : stockage en mémoire, valable pour un déploiement mono-instance
    /// d'AuthService. Une reprise après crash ou plusieurs répliques nécessiteraient un
    /// magasin partagé (ex. Redis/BDD).
    /// </summary>
    public class AccountUpdateSnapshotStore
    {
        public ConcurrentDictionary<Guid, (int UserId, string OldEmail)> Snapshots { get; } = new();
    }
}

namespace AuthService.Events
{
    /// <summary>
    /// Commande SAGA reçue de UserService (orchestrateur) : demande de validation
    /// et d'application d'une mise à jour de compte. Le mot de passe fourni sert de
    /// preuve d'identité (il est vérifié, jamais modifié).
    /// </summary>
    public class AccountUpdateRequested : Event
    {
        public Guid SagaId { get; set; }
        public int UserId { get; set; }
        public string? NewEmail { get; set; }
        public string? AuthPassword { get; set; }

        public override string ToString()
        {
            return $"[AccountUpdateRequested - Saga: {SagaId}, UserId: {UserId}]";
        }
    }
}

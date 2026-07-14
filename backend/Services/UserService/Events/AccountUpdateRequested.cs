namespace UserService.Events
{
    /// <summary>
    /// Commande SAGA envoyée par UserService (orchestrateur) à AuthService (participant)
    /// pour valider l'identité (mot de passe) et appliquer la mise à jour du compte.
    /// Sérialisée telle quelle sur la file <c>account-update-requested</c>.
    /// </summary>
    public class AccountUpdateRequested
    {
        public Guid SagaId { get; set; }
        public int UserId { get; set; }
        public string? NewEmail { get; set; }
        public string? AuthPassword { get; set; }
    }
}

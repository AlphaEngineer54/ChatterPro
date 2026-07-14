namespace UserService.Events
{
    /// <summary>
    /// Réponse du participant (AuthService) à une commande SAGA, reçue sur la file
    /// <c>account-update-reply</c>. <see cref="Status"/> reprend le nom d'une valeur
    /// d'<see cref="AccountUpdateStatus"/>.
    /// </summary>
    public class AccountUpdateReply
    {
        public Guid SagaId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>Issue possible d'une étape de validation SAGA (statuts transmis en texte).</summary>
    public enum AccountUpdateStatus
    {
        Confirmed,
        InvalidCredentials,
        EmailConflict,
        NotFound,
        Error
    }
}

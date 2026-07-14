namespace UserService.Events
{
    /// <summary>
    /// Commande SAGA de compensation envoyée à AuthService lorsque le commit local
    /// de l'orchestrateur échoue après une confirmation : demande de restaurer l'email.
    /// Sérialisée sur la file <c>account-update-compensate</c>.
    /// </summary>
    public class AccountUpdateCompensate
    {
        public Guid SagaId { get; set; }
    }
}

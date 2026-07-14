namespace AuthService.Events
{
    /// <summary>
    /// Commande SAGA de compensation : UserService demande d'annuler la préparation
    /// (restaurer l'email d'origine) si son commit local a échoué après confirmation.
    /// </summary>
    public class AccountUpdateCompensate : Event
    {
        public Guid SagaId { get; set; }

        public override string ToString()
        {
            return $"[AccountUpdateCompensate - Saga: {SagaId}]";
        }
    }
}

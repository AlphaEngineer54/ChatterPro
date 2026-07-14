namespace AuthService.Models
{
    /// <summary>
    /// Résultat de la validation/préparation d'une mise à jour de compte côté
    /// AuthService (participant du SAGA). Transmis à l'orchestrateur via la réponse.
    /// </summary>
    public enum AccountUpdateOutcome
    {
        Confirmed,
        InvalidCredentials,
        EmailConflict,
        NotFound
    }
}

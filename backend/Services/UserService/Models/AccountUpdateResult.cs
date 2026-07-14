namespace UserService.Models
{
    /// <summary>
    /// Résultat consolidé du SAGA de mise à jour de compte, mappé ensuite vers un
    /// code HTTP par le contrôleur.
    /// </summary>
    public enum AccountUpdateResult
    {
        Ok,
        InvalidCredentials,
        EmailConflict,
        NotFound,
        Error
    }
}

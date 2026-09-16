namespace HelpDesk.Api.DTOs
{
    /// <summary>
    /// Antwort nach einer erfolgreichen Anmeldung.
    /// </summary>
    public class LoginAntwortDto
    {
        /// <summary>
        /// JWT für den Zugriff auf geschützte Endpunkte.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Zeitpunkt, bis zu dem das Token gültig ist.
        /// </summary>
        public DateTime GueltigBis { get; set; }

        /// <summary>
        /// Rolle des angemeldeten Benutzers.
        /// </summary>
        /// <example>Support-Mitarbeiter</example>
        public string Rolle { get; set; } = string.Empty;
    }
}

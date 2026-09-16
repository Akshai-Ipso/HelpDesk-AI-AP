namespace HelpDesk.Api.Models
{
    /// <summary>
    /// Testbenutzer für die JWT-Anmeldung.
    /// </summary>
    public class TestBenutzer
    {
        /// <summary>
        /// Benutzername für die Anmeldung.
        /// </summary>
        public string Benutzername { get; set; } = string.Empty;

        /// <summary>
        /// Passwort für die Anmeldung.
        /// </summary>
        public string Passwort { get; set; } = string.Empty;

        /// <summary>
        /// Rolle des Benutzers.
        /// </summary>
        public string Rolle { get; set; } = string.Empty;
    }
}

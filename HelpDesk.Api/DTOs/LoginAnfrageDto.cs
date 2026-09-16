using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.DTOs
{
    /// <summary>
    /// Zugangsdaten für die Anmeldung.
    /// </summary>
    public class LoginAnfrageDto
    {
        /// <summary>
        /// Benutzername des Testbenutzers.
        /// </summary>
        /// <example>support</example>
        [Required(ErrorMessage = "Der Benutzername ist erforderlich.")]
        public string Benutzername { get; set; } = string.Empty;

        /// <summary>
        /// Passwort des Testbenutzers.
        /// </summary>
        /// <example>Support123!</example>
        [Required(ErrorMessage = "Das Passwort ist erforderlich.")]
        public string Passwort { get; set; } = string.Empty;
    }
}

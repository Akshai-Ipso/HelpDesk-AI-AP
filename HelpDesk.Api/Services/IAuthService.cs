using HelpDesk.Api.DTOs;

namespace HelpDesk.Api.Services
{
    /// <summary>
    /// Stellt die Anmeldung und JWT-Erzeugung bereit.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Prüft die Zugangsdaten und erzeugt bei Erfolg ein JWT.
        /// </summary>
        /// <param name="anfrage">Benutzername und Passwort.</param>
        /// <returns>
        /// Login-Antwort mit JWT oder null bei ungültigen Zugangsdaten.
        /// </returns>
        LoginAntwortDto? Anmelden(LoginAnfrageDto anfrage);
    }
}

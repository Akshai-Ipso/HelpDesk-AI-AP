using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.DTOs
{
    /// <summary>
    /// Eingabedaten zum Erstellen eines neuen Tickets.
    /// </summary>
    public class TicketErstellenDto
    {
        /// <summary>
        /// Kurzer Titel des Problems.
        /// </summary>
        /// <example>Drucker funktioniert nicht</example>
        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        public string Titel { get; set; } = string.Empty;

        /// <summary>
        /// Ausführliche Beschreibung des Problems.
        /// </summary>
        /// <example>
        /// Der Drucker im Büro zeigt seit heute die Fehlermeldung E42.
        /// </example>
        [Required(ErrorMessage = "Die Beschreibung ist erforderlich.")]
        public string Beschreibung { get; set; } = string.Empty;

        /// <summary>
        /// Kategorie des Supportfalls.
        /// Zulässig: Hardware, Software, Netzwerk, Zugriffsrechte oder Sonstiges.
        /// </summary>
        /// <example>Hardware</example>
        [Required(ErrorMessage = "Die Kategorie ist erforderlich.")]
        [RegularExpression(
            "^(Hardware|Software|Netzwerk|Zugriffsrechte|Sonstiges)$",
            ErrorMessage = "Die Kategorie muss Hardware, Software, Netzwerk, Zugriffsrechte oder Sonstiges sein.")]
        public string Kategorie { get; set; } = string.Empty;

        /// <summary>
        /// Priorität des Tickets.
        /// Zulässig: Niedrig, Mittel, Hoch oder Kritisch.
        /// </summary>
        /// <example>Hoch</example>
        [Required(ErrorMessage = "Die Priorität ist erforderlich.")]
        [RegularExpression(
            "^(Niedrig|Mittel|Hoch|Kritisch)$",
            ErrorMessage = "Die Priorität muss Niedrig, Mittel, Hoch oder Kritisch sein.")]
        public string Prioritaet { get; set; } = string.Empty;

        /// <summary>
        /// Name der Person, die das Ticket erstellt.
        /// </summary>
        /// <example>Max Muster</example>
        [Required(ErrorMessage = "Der Ersteller ist erforderlich.")]
        public string ErstelltVon { get; set; } = string.Empty;
    }
}

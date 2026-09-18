using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.DTOs
{
    /// <summary>
    /// Eingabedaten zum Aktualisieren eines Tickets.
    /// </summary>
    public class TicketAktualisierenDto
    {
        /// <summary>
        /// Aktualisierter Titel des Tickets.
        /// </summary>
        /// <example>Drucker funktioniert weiterhin nicht</example>
        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        public string Titel { get; set; } = string.Empty;

        /// <summary>
        /// Aktualisierte Beschreibung des Problems.
        /// </summary>
        /// <example>
        /// Ein Neustart wurde durchgeführt, die Fehlermeldung E42 bleibt bestehen.
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
        /// Aktueller Bearbeitungsstatus.
        /// Zulässig: Offen, InBearbeitung, Gelöst oder Geschlossen.
        /// </summary>
        /// <example>Geschlossen</example>
        [Required(ErrorMessage = "Der Status ist erforderlich.")]
        [RegularExpression(
            "^(Offen|InBearbeitung|Gelöst|Geschlossen)$",
            ErrorMessage = "Der Status muss Offen, InBearbeitung, Gelöst oder Geschlossen sein.")]
        public string Status { get; set; } = string.Empty;
    }
}

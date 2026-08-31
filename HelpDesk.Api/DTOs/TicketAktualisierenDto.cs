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
        /// </summary>
        /// <example>Hardware</example>
        [Required(ErrorMessage = "Die Kategorie ist erforderlich.")]
        public string Kategorie { get; set; } = string.Empty;

        /// <summary>
        /// Priorität des Tickets.
        /// </summary>
        /// <example>Hoch</example>
        [Required(ErrorMessage = "Die Priorität ist erforderlich.")]
        public string Prioritaet { get; set; } = string.Empty;

        /// <summary>
        /// Aktueller Bearbeitungsstatus.
        /// </summary>
        /// <example>Geschlossen</example>
        [Required(ErrorMessage = "Der Status ist erforderlich.")]
        public string Status { get; set; } = string.Empty;
    }
}
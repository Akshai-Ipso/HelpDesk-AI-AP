using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.DTOs
{
    /// <summary>
    /// Eingabedaten zum Erstellen einer manuellen Ticketantwort.
    /// </summary>
    public class AntwortErstellenDto
    {
        /// <summary>
        /// Name der antwortenden Person.
        /// </summary>
        /// <example>Support-Team</example>
        [Required(ErrorMessage = "Der Verfasser ist erforderlich.")]
        public string Verfasser { get; set; } = string.Empty;

        /// <summary>
        /// Inhalt der Antwort.
        /// </summary>
        /// <example>
        /// Bitte trennen Sie den Drucker für 30 Sekunden vom Stromnetz.
        /// </example>
        [Required(ErrorMessage = "Der Antworttext ist erforderlich.")]
        public string Text { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.DTOs
{
    public class TicketErstellenDto
    {
        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        public string Titel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Die Beschreibung ist erforderlich.")]
        public string Beschreibung { get; set; } = string.Empty;

        [Required(ErrorMessage = "Die Kategorie ist erforderlich.")]
        public string Kategorie { get; set; } = string.Empty;

        [Required(ErrorMessage = "Die Priorität ist erforderlich.")]
        public string Prioritaet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Der Ersteller ist erforderlich.")]
        public string ErstelltVon { get; set; } = string.Empty;
    }
}
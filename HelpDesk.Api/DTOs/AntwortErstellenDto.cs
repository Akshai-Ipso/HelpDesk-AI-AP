using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.DTOs
{
    public class AntwortErstellenDto
    {
        [Required(ErrorMessage = "Der Verfasser ist erforderlich.")]
        public string Verfasser { get; set; } = string.Empty;

        [Required(ErrorMessage = "Der Antworttext ist erforderlich.")]
        public string Text { get; set; } = string.Empty;
    }
}
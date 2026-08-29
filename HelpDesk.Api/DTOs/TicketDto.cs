namespace HelpDesk.Api.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }

        public string Titel { get; set; } = string.Empty;

        public string Beschreibung { get; set; } = string.Empty;

        public string Kategorie { get; set; } = string.Empty;

        public string Prioritaet { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ErstelltVon { get; set; } = string.Empty;

        public DateTime ErstelltAm { get; set; }

        public DateTime? GeschlossenAm { get; set; }
    }
}
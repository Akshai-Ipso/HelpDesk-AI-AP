namespace HelpDesk.Api.Models
{
    public class TicketAntwort
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string Verfasser { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public bool IstKiVorschlag { get; set; }

        public DateTime ErstelltAm { get; set; }

        public Ticket Ticket { get; set; } = null!;
    }
}
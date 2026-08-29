namespace HelpDesk.Api.Services
{
    public class TicketGeschlossenException : Exception
    {
        public int TicketId { get; }

        public TicketGeschlossenException(int ticketId)
            : base(
                $"Das Ticket mit der ID {ticketId} ist bereits geschlossen. " +
                "Für geschlossene Tickets können keine Antworten erstellt werden.")
        {
            TicketId = ticketId;
        }
    }
}
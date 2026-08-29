using HelpDesk.Api.DTOs;

namespace HelpDesk.Api.Services
{
    public interface ITicketService
    {
        Task<PagedResultDto<TicketDto>> TicketsAbrufenAsync(
            int seite,
            int seitengroesse,
            string? status,
            string? kategorie,
            string? prioritaet,
            string? sortierenNach,
            bool absteigend);

        Task<TicketDto?> TicketAbrufenAsync(int id);

        Task<TicketDto> TicketErstellenAsync(
            TicketErstellenDto dto);

        Task<TicketDto?> TicketAktualisierenAsync(
            int id,
            TicketAktualisierenDto dto);

        Task<bool> TicketLoeschenAsync(int id);

        Task<IReadOnlyList<AntwortDto>?> AntwortenAbrufenAsync(
            int ticketId);

        Task<AntwortDto?> AntwortErstellenAsync(
            int ticketId,
            AntwortErstellenDto dto);

        Task<AntwortDto?> KiVorschlagErstellenAsync(
            int ticketId);

        Task<bool> AntwortLoeschenAsync(
            int ticketId,
            int antwortId);
    }
}
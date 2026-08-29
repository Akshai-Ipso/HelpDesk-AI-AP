using HelpDesk.Api.DTOs;
using HelpDesk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<TicketDto>>>
            GetTickets(
                [FromQuery] int seite = 1,
                [FromQuery] int seitengroesse = 10,
                [FromQuery] string? status = null,
                [FromQuery] string? kategorie = null,
                [FromQuery] string? prioritaet = null,
                [FromQuery] string? sortierenNach = "erstelltam",
                [FromQuery] bool absteigend = true)
        {
            var ergebnis =
                await _ticketService.TicketsAbrufenAsync(
                    seite,
                    seitengroesse,
                    status,
                    kategorie,
                    prioritaet,
                    sortierenNach,
                    absteigend);

            return Ok(ergebnis);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var ticket =
                await _ticketService.TicketAbrufenAsync(id);

            if (ticket is null)
            {
                return NotFound(new
                {
                    fehler =
                        $"Das Ticket mit der ID {id} wurde nicht gefunden."
                });
            }

            return Ok(ticket);
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> TicketErstellen(
            TicketErstellenDto dto)
        {
            var ticket =
                await _ticketService.TicketErstellenAsync(dto);

            return CreatedAtAction(
                nameof(GetTicket),
                new { id = ticket.Id },
                ticket);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TicketDto>> TicketAktualisieren(
            int id,
            TicketAktualisierenDto dto)
        {
            var ticket =
                await _ticketService.TicketAktualisierenAsync(id, dto);

            if (ticket is null)
            {
                return NotFound(new
                {
                    fehler =
                        $"Das Ticket mit der ID {id} wurde nicht gefunden."
                });
            }

            return Ok(ticket);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> TicketLoeschen(int id)
        {
            var geloescht =
                await _ticketService.TicketLoeschenAsync(id);

            if (!geloescht)
            {
                return NotFound(new
                {
                    fehler =
                        $"Das Ticket mit der ID {id} wurde nicht gefunden."
                });
            }

            return NoContent();
        }

        [HttpGet("{id:int}/antworten")]
        public async Task<
            ActionResult<IReadOnlyList<AntwortDto>>> GetAntworten(int id)
        {
            var antworten =
                await _ticketService.AntwortenAbrufenAsync(id);

            if (antworten is null)
            {
                return NotFound(new
                {
                    fehler =
                        $"Das Ticket mit der ID {id} wurde nicht gefunden."
                });
            }

            return Ok(antworten);
        }

        [HttpPost("{id:int}/antworten")]
        public async Task<ActionResult<AntwortDto>> AntwortErstellen(
            int id,
            AntwortErstellenDto dto)
        {
            try
            {
                var antwort =
                    await _ticketService.AntwortErstellenAsync(id, dto);

                if (antwort is null)
                {
                    return NotFound(new
                    {
                        fehler =
                            $"Das Ticket mit der ID {id} wurde nicht gefunden."
                    });
                }

                return CreatedAtAction(
                    nameof(GetAntworten),
                    new { id },
                    antwort);
            }
            catch (TicketGeschlossenException exception)
            {
                return Conflict(new
                {
                    fehler = exception.Message
                });
            }
        }

        [HttpPost("{id:int}/ki-vorschlag")]
        public async Task<ActionResult<AntwortDto>>
            KiVorschlagErstellen(int id)
        {
            try
            {
                var antwort =
                    await _ticketService.KiVorschlagErstellenAsync(id);

                if (antwort is null)
                {
                    return NotFound(new
                    {
                        fehler =
                            $"Das Ticket mit der ID {id} wurde nicht gefunden."
                    });
                }

                return CreatedAtAction(
                    nameof(GetAntworten),
                    new { id },
                    antwort);
            }
            catch (TicketGeschlossenException exception)
            {
                return Conflict(new
                {
                    fehler = exception.Message
                });
            }
        }

        [HttpDelete("{id:int}/antworten/{antwortId:int}")]
        public async Task<IActionResult> AntwortLoeschen(
            int id,
            int antwortId)
        {
            var geloescht =
                await _ticketService.AntwortLoeschenAsync(
                    id,
                    antwortId);

            if (!geloescht)
            {
                return NotFound(new
                {
                    fehler =
                        $"Die Antwort mit der ID {antwortId} " +
                        $"wurde für Ticket {id} nicht gefunden."
                });
            }

            return NoContent();
        }
    }
}
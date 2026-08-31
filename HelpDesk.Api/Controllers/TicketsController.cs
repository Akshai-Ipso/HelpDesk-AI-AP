using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using HelpDesk.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;

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

        /// <summary>
        /// Ruft eine paginierte Liste aller Tickets ab.
        /// </summary>
        /// <remarks>
        /// Unterstützt Filter nach Status, Kategorie und Priorität sowie
        /// auf- und absteigende Sortierung.
        ///
        /// Beispiel:
        /// GET /api/tickets?page=1&amp;pageSize=10&amp;status=Offen&amp;sortBy=Prioritaet&amp;sortDirection=desc
        /// </remarks>
        /// <response code="200">Die gefilterte Ticketliste wurde geladen.</response>
        [ProducesResponseType(
            typeof(PagedResultDto<TicketDto>),
            StatusCodes.Status200OK)]

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<TicketDto>>>
            GetTickets(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? status = null,
                [FromQuery] string? kategorie = null,
                [FromQuery] string? prioritaet = null,
                [FromQuery] string sortBy = "erstelltam",
                [FromQuery] string sortDirection = "desc")
        {
            var absteigend = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            var ergebnis =
                await _ticketService.TicketsAbrufenAsync(
                    page,
                    pageSize,
                    status,
                    kategorie,
                    prioritaet,
                    sortBy,
                    absteigend);

            return Ok(ergebnis);
        }

        /// <summary>
        /// Ruft ein einzelnes Ticket anhand seiner ID ab.
        /// </summary>
        /// <param name="id">Eindeutige ID des Tickets.</param>
        /// <response code="200">Das Ticket wurde gefunden.</response>
        /// <response code="404">Das Ticket wurde nicht gefunden.</response>
        [ProducesResponseType(
            typeof(TicketDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var ticket =
                await _ticketService.TicketAbrufenAsync(id);

            if (ticket is null)
            {
                return NichtGefunden(
                    $"Das Ticket mit der ID {id} wurde nicht gefunden.");
            }

            return Ok(ticket);
        }

        /// <summary>
        /// Erstellt ein neues Support-Ticket.
        /// </summary>
        /// <remarks>
        /// Das Backend setzt den Status automatisch auf „Offen“ und vergibt
        /// den Erstellungszeitpunkt.
        /// </remarks>
        /// <response code="201">Das Ticket wurde erstellt.</response>
        /// <response code="400">Die Eingabedaten sind ungültig.</response>
        [ProducesResponseType(
            typeof(TicketDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest)]

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

        /// <summary>
        /// Aktualisiert ein vorhandenes Ticket einschließlich seines Status.
        /// </summary>
        /// <remarks>
        /// Beim Statuswechsel auf „Geschlossen“ wird GeschlossenAm automatisch
        /// gesetzt. Bei einer Wiedereröffnung wird GeschlossenAm entfernt.
        /// </remarks>
        /// <param name="id">Eindeutige ID des Tickets.</param>
        /// <response code="200">Das Ticket wurde aktualisiert.</response>
        /// <response code="400">Die Eingabedaten sind ungültig.</response>
        /// <response code="404">Das Ticket wurde nicht gefunden.</response>
        [ProducesResponseType(
            typeof(TicketDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TicketDto>> TicketAktualisieren(
            int id,
            TicketAktualisierenDto dto)
        {
            var ticket =
                await _ticketService.TicketAktualisierenAsync(id, dto);

            if (ticket is null)
{
    return NichtGefunden(
        $"Das Ticket mit der ID {id} wurde nicht gefunden.");
}

            return Ok(ticket);
        }

        /// <summary>
        /// Löscht ein Ticket und seine zugehörigen Antworten.
        /// </summary>
        /// <param name="id">Eindeutige ID des Tickets.</param>
        /// <response code="204">Das Ticket wurde erfolgreich gelöscht.</response>
        /// <response code="404">Das Ticket wurde nicht gefunden.</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> TicketLoeschen(int id)
        {
            var geloescht =
                await _ticketService.TicketLoeschenAsync(id);

            if (!geloescht)
            {
                return NichtGefunden(
                    $"Das Ticket mit der ID {id} wurde nicht gefunden.");
            }

            return NoContent();
        }

        /// <summary>
        /// Ruft alle Antworten eines Tickets ab.
        /// </summary>
        /// <param name="id">Eindeutige ID des Tickets.</param>
        /// <response code="200">
        /// Die Antworten des Tickets wurden geladen.
        /// </response>
        /// <response code="404">
        /// Das zugehörige Ticket wurde nicht gefunden.
        /// </response>
        [ProducesResponseType(
            typeof(IReadOnlyList<AntwortDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]

        [HttpGet("{id:int}/antworten")]
        public async Task<
            ActionResult<IReadOnlyList<AntwortDto>>> GetAntworten(int id)
        {
            var antworten =
                await _ticketService.AntwortenAbrufenAsync(id);

            if (antworten is null)
            {
                return NichtGefunden(
                    $"Das Ticket mit der ID {id} wurde nicht gefunden.");
            }

            return Ok(antworten);
        }

        /// <summary>
        /// Erstellt eine manuelle Antwort für ein Ticket.
        /// </summary>
        /// <remarks>
        /// Für geschlossene Tickets können keine neuen Antworten erstellt werden.
        /// In diesem Fall antwortet die API mit 409 Conflict.
        /// </remarks>
        /// <param name="id">Eindeutige ID des Tickets.</param>
        /// <response code="201">Die Antwort wurde erstellt.</response>
        /// <response code="400">Die Eingabedaten sind ungültig.</response>
        /// <response code="404">Das Ticket wurde nicht gefunden.</response>
        /// <response code="409">Das Ticket ist bereits geschlossen.</response>
        [ProducesResponseType(
            typeof(AntwortDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status409Conflict)]

        [HttpPost("{id:int}/antworten")]
        public async Task<ActionResult<AntwortDto>> AntwortErstellen(
            int id,
            AntwortErstellenDto dto)
        {
            var antwort =
                await _ticketService.AntwortErstellenAsync(id, dto);

            if (antwort is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Ressource nicht gefunden",
                    detail:
                        $"Das Ticket mit der ID {id} wurde nicht gefunden.");
            }

            return CreatedAtAction(
                nameof(GetAntworten),
                new { id },
                antwort);
        }

        /// <summary>
        /// Generiert einen simulierten KI-Antwortvorschlag für ein Ticket.
        /// </summary>
        /// <remarks>
        /// Der Vorschlag wird als TicketAntwort gespeichert und mit
        /// IstKiVorschlag = true gekennzeichnet. Für geschlossene Tickets
        /// wird kein Vorschlag generiert.
        /// </remarks>
        /// <param name="id">Eindeutige ID des Tickets.</param>
        /// <response code="201">
        /// Der KI-Vorschlag wurde generiert und gespeichert.
        /// </response>
        /// <response code="404">Das Ticket wurde nicht gefunden.</response>
        /// <response code="409">Das Ticket ist bereits geschlossen.</response>
        [ProducesResponseType(
            typeof(AntwortDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status409Conflict)]

        [HttpPost("{id:int}/ki-vorschlag")]
        public async Task<ActionResult<AntwortDto>>
    KiVorschlagErstellen(int id)
        {
            var antwort =
                await _ticketService.KiVorschlagErstellenAsync(id);

            if (antwort is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Ressource nicht gefunden",
                    detail:
                        $"Das Ticket mit der ID {id} wurde nicht gefunden.");
            }

            return CreatedAtAction(
                nameof(GetAntworten),
                new { id },
                antwort);
        }

        /// <summary>
        /// Löscht eine bestimmte Antwort eines Tickets.
        /// </summary>
        /// <param name="id">Eindeutige ID des Tickets.</param>
        /// <param name="antwortId">Eindeutige ID der Antwort.</param>
        /// <response code="204">Die Antwort wurde erfolgreich gelöscht.</response>
        /// <response code="404">
        /// Das Ticket oder die Antwort wurde nicht gefunden.
        /// </response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]

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
                return NichtGefunden(
                    $"Die Antwort mit der ID {antwortId} " +
                    $"wurde für Ticket {id} nicht gefunden.");
            }

            return NoContent();
        }

        private ObjectResult NichtGefunden(string detail)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Ressource nicht gefunden",
                detail: detail);

        }
    }
}
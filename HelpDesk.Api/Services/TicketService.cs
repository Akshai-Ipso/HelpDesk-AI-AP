using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services
{
    public class TicketService : ITicketService
    {
        private readonly HelpDeskDbContext _dbContext;
        private readonly IKiAntwortGenerator _kiAntwortGenerator;

        public TicketService(
            HelpDeskDbContext dbContext,
            IKiAntwortGenerator kiAntwortGenerator)
        {
            _dbContext = dbContext;
            _kiAntwortGenerator = kiAntwortGenerator;
        }

        public async Task<PagedResultDto<TicketDto>> TicketsAbrufenAsync(
            int seite,
            int seitengroesse,
            string? status,
            string? kategorie,
            string? prioritaet,
            string? sortierenNach,
            bool absteigend)
        {
            seite = Math.Max(seite, 1);
            seitengroesse = Math.Clamp(seitengroesse, 1, 100);

            IQueryable<Ticket> query =
                _dbContext.Tickets.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(ticket =>
                    ticket.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(kategorie))
            {
                query = query.Where(ticket =>
                    ticket.Kategorie == kategorie);
            }

            if (!string.IsNullOrWhiteSpace(prioritaet))
            {
                query = query.Where(ticket =>
                    ticket.Prioritaet == prioritaet);
            }

            query = (sortierenNach?.ToLowerInvariant(), absteigend)
                switch
            {
                ("titel", false) =>
                    query.OrderBy(ticket => ticket.Titel),

                ("titel", true) =>
                    query.OrderByDescending(ticket => ticket.Titel),

                ("status", false) =>
                    query.OrderBy(ticket => ticket.Status),

                ("status", true) =>
                    query.OrderByDescending(ticket => ticket.Status),

                ("kategorie", false) =>
                    query.OrderBy(ticket => ticket.Kategorie),

                ("kategorie", true) =>
                    query.OrderByDescending(ticket => ticket.Kategorie),

                ("prioritaet", false) =>
                    query.OrderBy(ticket => ticket.Prioritaet),

                ("prioritaet", true) =>
                    query.OrderByDescending(ticket => ticket.Prioritaet),

                ("erstelltam", false) =>
                    query.OrderBy(ticket => ticket.ErstelltAm),

                _ => query.OrderByDescending(
                    ticket => ticket.ErstelltAm)
            };

            var gesamtanzahl = await query.CountAsync();

            var tickets = await query
                .Skip((seite - 1) * seitengroesse)
                .Take(seitengroesse)
                .Select(ticket => new TicketDto
                {
                    Id = ticket.Id,
                    Titel = ticket.Titel,
                    Beschreibung = ticket.Beschreibung,
                    Kategorie = ticket.Kategorie,
                    Prioritaet = ticket.Prioritaet,
                    Status = ticket.Status,
                    ErstelltVon = ticket.ErstelltVon,
                    ErstelltAm = ticket.ErstelltAm,
                    GeschlossenAm = ticket.GeschlossenAm
                })
                .ToListAsync();

            return new PagedResultDto<TicketDto>
            {
                Elemente = tickets,
                Seite = seite,
                Seitengroesse = seitengroesse,
                Gesamtanzahl = gesamtanzahl
            };
        }

        public async Task<TicketDto?> TicketAbrufenAsync(int id)
        {
            return await _dbContext.Tickets
                .AsNoTracking()
                .Where(ticket => ticket.Id == id)
                .Select(ticket => new TicketDto
                {
                    Id = ticket.Id,
                    Titel = ticket.Titel,
                    Beschreibung = ticket.Beschreibung,
                    Kategorie = ticket.Kategorie,
                    Prioritaet = ticket.Prioritaet,
                    Status = ticket.Status,
                    ErstelltVon = ticket.ErstelltVon,
                    ErstelltAm = ticket.ErstelltAm,
                    GeschlossenAm = ticket.GeschlossenAm
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TicketDto> TicketErstellenAsync(
            TicketErstellenDto dto)
        {
            var ticket = new Ticket
            {
                Titel = dto.Titel,
                Beschreibung = dto.Beschreibung,
                Kategorie = dto.Kategorie,
                Prioritaet = dto.Prioritaet,
                Status = "Offen",
                ErstelltVon = dto.ErstelltVon,
                ErstelltAm = DateTime.UtcNow,
                GeschlossenAm = null
            };

            _dbContext.Tickets.Add(ticket);
            await _dbContext.SaveChangesAsync();

            return ZuDto(ticket);
        }

        public async Task<TicketDto?> TicketAktualisierenAsync(
            int id,
            TicketAktualisierenDto dto)
        {
            var ticket = await _dbContext.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return null;
            }

            ticket.Titel = dto.Titel;
            ticket.Beschreibung = dto.Beschreibung;
            ticket.Kategorie = dto.Kategorie;
            ticket.Prioritaet = dto.Prioritaet;
            ticket.Status = dto.Status;

            if (string.Equals(
                dto.Status,
                "Geschlossen",
                StringComparison.OrdinalIgnoreCase))
            {
                ticket.Status = "Geschlossen";
                ticket.GeschlossenAm ??= DateTime.UtcNow;
            }
            else
            {
                ticket.GeschlossenAm = null;
            }

            await _dbContext.SaveChangesAsync();

            return ZuDto(ticket);
        }

        public async Task<bool> TicketLoeschenAsync(int id)
        {
            var ticket = await _dbContext.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return false;
            }

            _dbContext.Tickets.Remove(ticket);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IReadOnlyList<AntwortDto>?>
            AntwortenAbrufenAsync(int ticketId)
        {
            var ticketExistiert = await _dbContext.Tickets
                .AnyAsync(ticket => ticket.Id == ticketId);

            if (!ticketExistiert)
            {
                return null;
            }

            return await _dbContext.TicketAntworten
                .AsNoTracking()
                .Where(antwort => antwort.TicketId == ticketId)
                .OrderBy(antwort => antwort.ErstelltAm)
                .Select(antwort => new AntwortDto
                {
                    Id = antwort.Id,
                    TicketId = antwort.TicketId,
                    Verfasser = antwort.Verfasser,
                    Text = antwort.Text,
                    IstKiVorschlag = antwort.IstKiVorschlag,
                    ErstelltAm = antwort.ErstelltAm
                })
                .ToListAsync();
        }

        public async Task<AntwortDto?> AntwortErstellenAsync(
            int ticketId,
            AntwortErstellenDto dto)
        {
            var ticket = await _dbContext.Tickets.FindAsync(ticketId);

            if (ticket is null)
            {
                return null;
            }

            PruefeObTicketGeschlossenIst(ticket);

            var antwort = new TicketAntwort
            {
                TicketId = ticketId,
                Verfasser = dto.Verfasser,
                Text = dto.Text,
                IstKiVorschlag = false,
                ErstelltAm = DateTime.UtcNow
            };

            _dbContext.TicketAntworten.Add(antwort);
            await _dbContext.SaveChangesAsync();

            return ZuDto(antwort);
        }

        public async Task<AntwortDto?> KiVorschlagErstellenAsync(
            int ticketId)
        {
            var ticket = await _dbContext.Tickets.FindAsync(ticketId);

            if (ticket is null)
            {
                return null;
            }

            PruefeObTicketGeschlossenIst(ticket);

            var text = await _kiAntwortGenerator
                .GeneriereVorschlagAsync(
                    ticket.Titel,
                    ticket.Beschreibung,
                    ticket.Kategorie);

            var antwort = new TicketAntwort
            {
                TicketId = ticketId,
                Verfasser = "KI-Assistent",
                Text = text,
                IstKiVorschlag = true,
                ErstelltAm = DateTime.UtcNow
            };

            _dbContext.TicketAntworten.Add(antwort);
            await _dbContext.SaveChangesAsync();

            return ZuDto(antwort);
        }

        public async Task<bool> AntwortLoeschenAsync(
            int ticketId,
            int antwortId)
        {
            var antwort = await _dbContext.TicketAntworten
                .FirstOrDefaultAsync(antwort =>
                    antwort.Id == antwortId &&
                    antwort.TicketId == ticketId);

            if (antwort is null)
            {
                return false;
            }

            _dbContext.TicketAntworten.Remove(antwort);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        private static void PruefeObTicketGeschlossenIst(
            Ticket ticket)
        {
            if (string.Equals(
                ticket.Status,
                "Geschlossen",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new TicketGeschlossenException(ticket.Id);
            }
        }

        private static TicketDto ZuDto(Ticket ticket)
        {
            return new TicketDto
            {
                Id = ticket.Id,
                Titel = ticket.Titel,
                Beschreibung = ticket.Beschreibung,
                Kategorie = ticket.Kategorie,
                Prioritaet = ticket.Prioritaet,
                Status = ticket.Status,
                ErstelltVon = ticket.ErstelltVon,
                ErstelltAm = ticket.ErstelltAm,
                GeschlossenAm = ticket.GeschlossenAm
            };
        }

        private static AntwortDto ZuDto(TicketAntwort antwort)
        {
            return new AntwortDto
            {
                Id = antwort.Id,
                TicketId = antwort.TicketId,
                Verfasser = antwort.Verfasser,
                Text = antwort.Text,
                IstKiVorschlag = antwort.IstKiVorschlag,
                ErstelltAm = antwort.ErstelltAm
            };
        }
    }
}
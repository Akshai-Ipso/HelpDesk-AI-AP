using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using HelpDesk.Api.Models;
using HelpDesk.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDesk.Api.Tests;

public class TicketServiceTests
{
    [Fact]
    public async Task AntwortZuOffenemTicketWirdGespeichert()
    {
        await using var database = TestDatabase.Create();
        var ticket = await database.AddTicketAsync();
        var ki = new FakeKiAntwortGenerator();
        var service = database.CreateService(ki);

        var antwort = await service.AntwortErstellenAsync(
            ticket.Id,
            new AntwortErstellenDto
            {
                Verfasser = "Support",
                Text = "Bitte starten Sie das Gerät neu."
            });

        Assert.NotNull(antwort);
        Assert.False(antwort.IstKiVorschlag);
        Assert.Equal(1, await database.Context.TicketAntworten.CountAsync());
    }

    [Fact]
    public async Task AntwortZuGeschlossenemTicketWirdAbgelehnt()
    {
        await using var database = TestDatabase.Create();
        var ticket = await database.AddTicketAsync("Geschlossen");
        var service = database.CreateService(new FakeKiAntwortGenerator());

        await Assert.ThrowsAsync<TicketGeschlossenException>(() =>
            service.AntwortErstellenAsync(
                ticket.Id,
                new AntwortErstellenDto
                {
                    Verfasser = "Support",
                    Text = "Darf nicht gespeichert werden"
                }));

        Assert.Empty(await database.Context.TicketAntworten.ToListAsync());
    }

    [Fact]
    public async Task KiVorschlagZuGeschlossenemTicketWirdAbgelehnt()
    {
        await using var database = TestDatabase.Create();
        var ticket = await database.AddTicketAsync("Geschlossen");
        var ki = new FakeKiAntwortGenerator();
        var service = database.CreateService(ki);

        await Assert.ThrowsAsync<TicketGeschlossenException>(() =>
            service.KiVorschlagErstellenAsync(ticket.Id));

        Assert.Equal(string.Empty, ki.Titel);
        Assert.Empty(await database.Context.TicketAntworten.ToListAsync());
    }

    [Fact]
    public async Task StatuswechselAufGeschlossenSetztGeschlossenAm()
    {
        await using var database = TestDatabase.Create();
        var ticket = await database.AddTicketAsync();
        var service = database.CreateService(new FakeKiAntwortGenerator());

        var aktualisiert = await service.TicketAktualisierenAsync(
            ticket.Id,
            new TicketAktualisierenDto
            {
                Titel = ticket.Titel,
                Beschreibung = ticket.Beschreibung,
                Kategorie = ticket.Kategorie,
                Prioritaet = ticket.Prioritaet,
                Status = "Geschlossen"
            });

        Assert.NotNull(aktualisiert);
        Assert.Equal("Geschlossen", aktualisiert.Status);
        Assert.NotNull(aktualisiert.GeschlossenAm);
    }

    [Fact]
    public async Task KiVorschlagVerwendetTestDoppelgaenger()
    {
        await using var database = TestDatabase.Create();
        var ticket = await database.AddTicketAsync();
        var ki = new FakeKiAntwortGenerator();
        var service = database.CreateService(ki);

        var antwort = await service.KiVorschlagErstellenAsync(ticket.Id);

        Assert.NotNull(antwort);
        Assert.True(antwort.IstKiVorschlag);
        Assert.Equal("Test-KI-Antwort", antwort.Text);
        Assert.Equal(ticket.Titel, ki.Titel);
    }

    private sealed class FakeKiAntwortGenerator : IKiAntwortGenerator
    {
        public string Titel { get; private set; } = string.Empty;

        public Task<string> GeneriereVorschlagAsync(
            string titel,
            string beschreibung,
            string kategorie)
        {
            Titel = titel;
            return Task.FromResult("Test-KI-Antwort");
        }
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection, HelpDeskDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public HelpDeskDbContext Context { get; }

        public static TestDatabase Create()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new HelpDeskDbContext(options);
            context.Database.EnsureCreated();
            return new TestDatabase(connection, context);
        }

        public async Task<Ticket> AddTicketAsync(string status = "Offen")
        {
            var ticket = new Ticket
            {
                Titel = "Druckerproblem",
                Beschreibung = "Der Drucker funktioniert nicht.",
                Kategorie = "Hardware",
                Prioritaet = "Hoch",
                Status = status,
                ErstelltVon = "Testperson",
                ErstelltAm = DateTime.UtcNow
            };
            Context.Tickets.Add(ticket);
            await Context.SaveChangesAsync();
            return ticket;
        }

        public TicketService CreateService(IKiAntwortGenerator ki)
        {
            return new TicketService(
                Context,
                ki,
                NullLogger<TicketService>.Instance);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
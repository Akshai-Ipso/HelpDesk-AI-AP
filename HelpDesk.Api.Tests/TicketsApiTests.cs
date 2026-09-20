using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpDesk.Api.Tests;

public class TicketsApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public TicketsApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTicketErstelltTicketEndToEnd()
    {
        await AnmeldenAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();
        Assert.Equal("Offen", ticket!.Status);
    }

    [Fact]
    public async Task GetTicketsLiefertListeEndToEnd()
    {
        await AnmeldenAsync();

        var response = await _client.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PutAufGeschlossenSetztZeitpunktEndToEnd()
    {
        await AnmeldenAsync();

        var create = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();

        var response = await _client.PutAsJsonAsync(
            $"/api/tickets/{ticket!.Id}",
            new TicketAktualisierenDto
            {
                Titel = ticket.Titel,
                Beschreibung = ticket.Beschreibung,
                Kategorie = ticket.Kategorie,
                Prioritaet = ticket.Prioritaet,
                Status = "Geschlossen"
            });

        var aktualisiert = await response.Content.ReadFromJsonAsync<TicketDto>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(aktualisiert!.GeschlossenAm);
    }

    [Fact]
    public async Task VorgegebeneDatenwerteKoennenAktualisiertWerden()
    {
        await AnmeldenAsync();

        var create = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();

        var response = await _client.PutAsJsonAsync(
            $"/api/tickets/{ticket!.Id}",
            new TicketAktualisierenDto
            {
                Titel = ticket.Titel,
                Beschreibung = ticket.Beschreibung,
                Kategorie = "Zugriffsrechte",
                Prioritaet = "Kritisch",
                Status = "InBearbeitung"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var aktualisiert = await response.Content.ReadFromJsonAsync<TicketDto>();
        Assert.Equal("Zugriffsrechte", aktualisiert!.Kategorie);
        Assert.Equal("Kritisch", aktualisiert.Prioritaet);
        Assert.Equal("InBearbeitung", aktualisiert.Status);

        var geloestResponse = await _client.PutAsJsonAsync(
            $"/api/tickets/{ticket.Id}",
            new TicketAktualisierenDto
            {
                Titel = aktualisiert.Titel,
                Beschreibung = aktualisiert.Beschreibung,
                Kategorie = aktualisiert.Kategorie,
                Prioritaet = aktualisiert.Prioritaet,
                Status = "Gelöst"
            });

        Assert.Equal(HttpStatusCode.OK, geloestResponse.StatusCode);
        var geloest = await geloestResponse.Content.ReadFromJsonAsync<TicketDto>();
        Assert.Equal("Gelöst", geloest!.Status);
    }

    [Fact]
    public async Task AntwortAufGeschlossenemTicketLiefertConflict()
    {
        await AnmeldenAsync();

        var create = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();

        await _client.PutAsJsonAsync(
            $"/api/tickets/{ticket!.Id}",
            new TicketAktualisierenDto
            {
                Titel = ticket.Titel,
                Beschreibung = ticket.Beschreibung,
                Kategorie = ticket.Kategorie,
                Prioritaet = ticket.Prioritaet,
                Status = "Geschlossen"
            });

        var response = await _client.PostAsJsonAsync(
            $"/api/tickets/{ticket.Id}/antworten",
            new AntwortErstellenDto
            {
                Verfasser = "Support",
                Text = "Darf nicht erstellt werden"
            });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task LoeschenOhneTokenWirdNichtAutorisiert()
    {
        var response = await _client.DeleteAsync("/api/tickets/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoeschenMitSupportRolleWirdVerboten()
    {
        await AnmeldenAsync();

        var response = await _client.DeleteAsync("/api/tickets/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task LoeschenMitTeamleitungIstErlaubt()
    {
        await AnmeldenAsync("teamleitung", "Team123!");

        var create = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();

        var response = await _client.DeleteAsync(
            $"/api/tickets/{ticket!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task LoeschenMitVorhandenenAntwortenFunktioniert()
    {
        await AnmeldenAsync("teamleitung", "Team123!");

        var create = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();

        var antwort = await _client.PostAsJsonAsync(
            $"/api/tickets/{ticket!.Id}/antworten",
            new AntwortErstellenDto
            {
                Verfasser = "Support",
                Text = "Erste Antwort auf das Ticket"
            });
        Assert.Equal(HttpStatusCode.Created, antwort.StatusCode);

        var response = await _client.DeleteAsync(
            $"/api/tickets/{ticket.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var nachDemLoeschen = await _client.GetAsync(
            $"/api/tickets/{ticket.Id}");
        Assert.Equal(
            HttpStatusCode.NotFound,
            nachDemLoeschen.StatusCode);
    }

    [Fact]
    public async Task LeererTitelWirdAbgelehnt()
    {
        await AnmeldenAsync();

        var ticket = NeuesTicket();
        ticket.Titel = string.Empty;

        var response = await _client.PostAsJsonAsync("/api/tickets", ticket);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnbekannteKategorieWirdAbgelehnt()
    {
        await AnmeldenAsync();

        var ticket = NeuesTicket();
        ticket.Kategorie = "KeineGueltigeKategorie";

        var response = await _client.PostAsJsonAsync("/api/tickets", ticket);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnbekanntePrioritaetWirdAbgelehnt()
    {
        await AnmeldenAsync();

        var ticket = NeuesTicket();
        ticket.Prioritaet = "Extrem";

        var response = await _client.PostAsJsonAsync("/api/tickets", ticket);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnbekannterStatusWirdAbgelehnt()
    {
        await AnmeldenAsync();

        var create = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();

        var response = await _client.PutAsJsonAsync(
            $"/api/tickets/{ticket!.Id}",
            new TicketAktualisierenDto
            {
                Titel = ticket.Titel,
                Beschreibung = ticket.Beschreibung,
                Kategorie = ticket.Kategorie,
                Prioritaet = ticket.Prioritaet,
                Status = "Unbekannt"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LeererAntworttextWirdAbgelehnt()
    {
        await AnmeldenAsync();

        var create = await _client.PostAsJsonAsync(
            "/api/tickets",
            NeuesTicket());
        var ticket = await create.Content.ReadFromJsonAsync<TicketDto>();

        var response = await _client.PostAsJsonAsync(
            $"/api/tickets/{ticket!.Id}/antworten",
            new AntwortErstellenDto
            {
                Verfasser = "Support",
                Text = string.Empty
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NegativeTicketIdWirdNichtGefunden()
    {
        await AnmeldenAsync();

        var response = await _client.GetAsync("/api/tickets/-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task AnmeldenAsync(
        string benutzername = "support",
        string passwort = "Support123!")
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginAnfrageDto
            {
                Benutzername = benutzername,
                Passwort = passwort
            });

        response.EnsureSuccessStatusCode();

        var login = await response.Content
            .ReadFromJsonAsync<LoginAntwortDto>();

        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    private static TicketErstellenDto NeuesTicket() => new()
    {
        Titel = "Neues Ticket",
        Beschreibung = "Eine ausreichende Beschreibung",
        Kategorie = "Hardware",
        Prioritaet = "Mittel",
        ErstelltVon = "Integrationstest"
    };
}

public sealed class ApiFactory : WebApplicationFactory<HelpDesk.Api.Program>
{
    private const string TestJwtKey =
        "HelpDesk-Integrationstest-Schluessel-2026-123456";

    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    private readonly string? _urspruenglicherJwtKey =
        Environment.GetEnvironmentVariable("Jwt__Key");

    public ApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                service => service.ServiceType ==
                    typeof(DbContextOptions<HelpDeskDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<HelpDeskDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<HelpDeskDbContext>()
            .Database.EnsureCreated();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            _urspruenglicherJwtKey);
    }
}

using System.Net;
using System.Net.Http.Json;
using HelpDesk.Api.Data;
using HelpDesk.Api.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        var response = await _client.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PutAufGeschlossenSetztZeitpunktEndToEnd()
    {
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
    public async Task AntwortAufGeschlossenemTicketLiefertConflict()
    {
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
    public async Task LoeschenOhneApiKeyWirdNichtAutorisiert()
    {
        var response = await _client.DeleteAsync("/api/tickets/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LeererTitelWirdAbgelehnt()
    {
        var ticket = NeuesTicket();
        ticket.Titel = string.Empty;

        var response = await _client.PostAsJsonAsync("/api/tickets", ticket);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LeererAntworttextWirdAbgelehnt()
    {
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
        var response = await _client.GetAsync("/api/tickets/-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static TicketErstellenDto NeuesTicket() => new()
    {
        Titel = "Neues Ticket",
        Beschreibung = "Eine ausreichende Beschreibung",
        Kategorie = "Hardware",
        Prioritaet = "Normal",
        ErstelltVon = "Integrationstest"
    };
}

public sealed class ApiFactory : WebApplicationFactory<HelpDesk.Api.Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Development");
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
    }
}
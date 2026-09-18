using HelpDesk.Api.Services;

namespace HelpDesk.Api.Tests;

public class SimulierterKiAntwortGeneratorTests
{
    [Theory]
    [InlineData(
        "Gerät ohne Funktion",
        "Beim Drucken erscheint ein Papierfehler.",
        "Hardware",
        "Drucker")]
    [InlineData(
        "Keine Verbindung",
        "Das WLAN verbindet sich nicht.",
        "Netzwerk",
        "Netzwerkverbindung")]
    [InlineData(
        "Anmeldung nicht möglich",
        "Mein Passwort wird nicht akzeptiert.",
        "Zugriffsrechte",
        "Passwort-zurücksetzen")]
    public async Task BeschreibungBestimmtDenLoesungsvorschlag(
        string titel,
        string beschreibung,
        string kategorie,
        string erwarteterText)
    {
        var generator = new SimulierterKiAntwortGenerator();

        var vorschlag = await generator.GeneriereVorschlagAsync(
            titel,
            beschreibung,
            kategorie);

        Assert.Contains(erwarteterText, vorschlag);
    }
}

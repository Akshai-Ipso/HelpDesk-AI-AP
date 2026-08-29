namespace HelpDesk.Api.Services
{
    public class SimulierterKiAntwortGenerator
        : IKiAntwortGenerator
    {
        public Task<string> GeneriereVorschlagAsync(
            string titel,
            string beschreibung,
            string kategorie)
        {
            var vorschlag =
                $"Vielen Dank für Ihre Anfrage zum Thema „{titel}“. " +
                $"Wir haben das Anliegen der Kategorie „{kategorie}“ aufgenommen. " +
                "Bitte prüfen Sie zunächst, ob das Problem nach einem Neustart " +
                "weiterhin besteht. Falls ja, senden Sie uns bitte weitere " +
                "Informationen oder eine genaue Fehlermeldung.";

            return Task.FromResult(vorschlag);
        }
    }
}
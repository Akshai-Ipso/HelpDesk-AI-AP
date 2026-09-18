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
            var suchtext = $"{titel} {beschreibung}".ToLowerInvariant();

            string loesung;
            if (EnthaeltEines(suchtext, "drucker", "drucken", "papier", "toner"))
            {
                loesung =
                    "Prüfen Sie, ob der Drucker eingeschaltet und verbunden ist, " +
                    "ob Papier vorhanden ist und ob eine Fehlermeldung angezeigt wird. " +
                    "Starten Sie danach den Drucker und den Druckauftrag neu.";
            }
            else if (kategorie.Equals("Netzwerk", StringComparison.OrdinalIgnoreCase) ||
                     EnthaeltEines(suchtext, "netzwerk", "internet", "wlan", "vpn", "verbindung"))
            {
                loesung =
                    "Prüfen Sie die Netzwerkverbindung und testen Sie, ob andere " +
                    "Webseiten oder Geräte erreichbar sind. Verbinden Sie WLAN oder " +
                    "VPN neu und notieren Sie eine angezeigte Fehlermeldung.";
            }
            else if (kategorie.Equals("Zugriffsrechte", StringComparison.OrdinalIgnoreCase) ||
                     EnthaeltEines(suchtext, "passwort", "login", "anmeldung", "konto", "zugriff"))
            {
                loesung =
                    "Prüfen Sie Benutzername und Feststelltaste. Nutzen Sie danach " +
                    "die Passwort-zurücksetzen-Funktion. Teilen Sie niemals Ihr " +
                    "Passwort in einer Supportanfrage mit.";
            }
            else if (kategorie.Equals("Software", StringComparison.OrdinalIgnoreCase) ||
                     EnthaeltEines(suchtext, "software", "programm", "anwendung", "absturz"))
            {
                loesung =
                    "Beenden und starten Sie die betroffene Anwendung neu. Prüfen " +
                    "Sie verfügbare Updates und senden Sie uns bei erneutem Auftreten " +
                    "die genaue Fehlermeldung.";
            }
            else
            {
                loesung =
                    "Starten Sie das betroffene Gerät oder Programm neu. Falls das " +
                    "Problem bestehen bleibt, senden Sie uns die genaue Fehlermeldung " +
                    "und die zuletzt ausgeführten Schritte.";
            }

            var vorschlag =
                $"Vorschlag für „{titel}“ (Kategorie: {kategorie}): {loesung}";

            return Task.FromResult(vorschlag);
        }

        private static bool EnthaeltEines(string text, params string[] suchwoerter)
        {
            return suchwoerter.Any(text.Contains);
        }
    }
}

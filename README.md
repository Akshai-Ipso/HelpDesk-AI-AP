# HelpDesk API

## Installierte Komponenten

Für das Projekt werden folgende Komponenten verwendet:

- Visual Studio 2022
- ASP.NET-und-Webentwicklung-Workload
- .NET 8 SDK
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.AspNetCore.Authentication.JwtBearer
- SQLite-Datenbank
- xUnit für automatisierte Tests
- Swagger/OpenAPI

## Aufbau der Solution

Die Solution `HelpDesk` besteht aus zwei Projekten:

- `HelpDesk.Api`: ASP.NET-Core-Web-API
- `HelpDesk.Api.Tests`: xUnit-Testprojekt

Das API-Projekt enthält folgende Ordner:

- `Controllers`: API-Endpunkte
- `Data`: Datenbankzugriff und DbContext
- `DTOs`: Objekte für Ein- und Ausgaben der API
- `Middleware`: zentrale Fehlerbehandlung
- `Models`: Datenmodelle
- `Services`: Geschäftslogik und KI-Antwortgenerator

Die SQLite-Datenbank befindet sich in `HelpDesk.Api/helpdesk.db`.

## Projekt lokal starten

1. Ein Terminal im Repository-Stamm öffnen.
2. Vor dem ersten Start einen lokalen JWT-Signaturschlüssel hinterlegen:

```powershell
dotnet user-secrets set "Jwt:Key" "Einen-langen-lokalen-Schluessel-eintragen" --project HelpDesk.Api
```

3. Die Solution `HelpDesk.sln` in Visual Studio 2022 öffnen.
4. `HelpDesk.Api` als Startprojekt festlegen.
5. Das Projekt mit `F5` oder `Strg + F5` starten. Swagger wird im Browser geöffnet.

Alternativ kann das API-Projekt nach Schritt 2 im Terminal gestartet werden:

```powershell
dotnet run --project HelpDesk.Api
```

Der JWT-Signaturschlüssel wird lokal über .NET User Secrets gespeichert
und nicht in das Git-Repository aufgenommen.

## Authentifizierung und Rollen

Die API verwendet JWT-Bearer-Authentifizierung. Ein Token wird über
folgenden Endpunkt angefordert:

```text
POST /api/auth/login
```

Für die lokale Demonstration sind zwei Testbenutzer in der
`appsettings.json` hinterlegt:

| Benutzername | Passwort | Rolle |
|---|---|---|
| `support` | `Support123!` | Support-Mitarbeiter |
| `teamleitung` | `Team123!` | Teamleitung |

Beispiel für die Anmeldung:

```json
{
  "benutzername": "support",
  "passwort": "Support123!"
}
```

Bei erfolgreicher Anmeldung liefert die API ein JWT, den
Gültigkeitszeitpunkt und die Rolle des Benutzers zurück. Das Token ist
standardmäßig 60 Minuten gültig.

Alle Ticket-Endpunkte erfordern ein gültiges JWT. Beide Rollen dürfen
Tickets lesen, erstellen und aktualisieren sowie Antworten und
KI-Vorschläge erstellen.

Die folgenden Endpunkte dürfen ausschließlich mit der Rolle
`Teamleitung` ausgeführt werden:

- `DELETE /api/tickets/{id}`
- `DELETE /api/tickets/{id}/antworten/{antwortId}`

Die Autorisierung wurde über Swagger nachgewiesen:

- Erfolgreicher Login als Teamleitung: `200 OK`
- Zugriff ohne Token: `401 Unauthorized`
- Löschzugriff als Support-Mitarbeiter: `403 Forbidden`
- Löschzugriff als Teamleitung: `204 No Content`

Die zugehörigen Screenshots befinden sich unter
`Screenshots Testing/6.6 Authentifizierung`.

In Swagger wird das vom Login-Endpunkt erhaltene JWT über die
Schaltfläche **Authorize** eingetragen.

## Automatisierte Tests

Die Solution enthält Unit- und Integrationstests. Die Integrationstests
verwenden eine SQLite-In-Memory-Datenbank und prüfen auch JWT und Rollen.

```powershell
dotnet test .\HelpDesk.Api.Tests\HelpDesk.Api.Tests.csproj
```

## KI-Antwortvorschläge

Für die KI-Vorschlagsfunktion wurde Variante B, eine simulierte
KI-Implementierung, gewählt.

Die Klasse `SimulierterKiAntwortGenerator` wertet Titel, Beschreibung und
Kategorie aus. Schlüsselwörter führen zu unterschiedlichen Vorschlägen für
Drucker-, Netzwerk-, Passwort-/Zugriffs- und Softwareprobleme. Für andere
Anliegen wird ein allgemeiner Vorschlag erzeugt. Diese Variante wurde gewählt,
weil sie keine externe Abhängigkeit und keinen API-Key benötigt und dadurch
lokal deterministisch getestet werden kann.

Die Abstraktion erfolgt über das Interface `IKiAntwortGenerator`.
Die konkrete Implementierung wird über Dependency Injection
eingebunden und kann später durch eine echte LLM-Anbindung ersetzt
werden, ohne den Ticket-Service oder den Controller anzupassen.

Generierte Vorschläge werden als `TicketAntwort` gespeichert und mit
`IstKiVorschlag = true` gekennzeichnet.

## Architekturüberblick

Die Anwendung ist in mehrere klar getrennte Schichten aufgebaut:

- Controller: Verarbeitet HTTP-Anfragen und gibt HTTP-Statuscodes zurück.
- Service-Layer: Enthält Geschäftslogik, Statuswechsel und KI-Aufrufe.
- Datenzugriff: Erfolgt über Entity Framework Core und den `HelpDeskDbContext`.
- Datenmodelle: Bilden die Tabellen der SQLite-Datenbank ab.
- DTOs: Definieren die Ein- und Ausgaben der REST-API.
- Middleware: Behandelt Fehler zentral und erzeugt einheitliche `ProblemDetails`.
- KI-Komponente: Erzeugt simulierte Antwortvorschläge über ein Interface.

Der Ablauf einer Anfrage ist:

```text
Client
  -> TicketsController
  -> ITicketService / TicketService
  -> HelpDeskDbContext
  -> SQLite-Datenbank
```

## Datenmodell

Die Anwendung verwendet die zwei zusammengehörenden Datenmodelle `Ticket`
und `TicketAntwort`.

### Ticket

Ein Ticket enthält:

- Titel
- Beschreibung
- Kategorie
- Priorität
- Status
- Ersteller
- Erstellungszeitpunkt
- optionalen Abschlusszeitpunkt

### TicketAntwort

Eine Ticketantwort enthält:

- die zugehörige Ticket-ID
- Verfasser
- Antworttext
- Kennzeichnung als KI-Vorschlag
- Erstellungszeitpunkt

Zwischen den Tabellen besteht eine 1:n-Beziehung:

```text
Ticket 1 -------- n TicketAntwort
```

Ein Ticket kann mehrere Antworten besitzen. Jede Antwort gehört über
`TicketId` zu genau einem Ticket. Beim Löschen eines Tickets werden die
zugehörigen Antworten ebenfalls gelöscht.

## API-Endpunkte

### Tickets

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/tickets` | Tickets mit Pagination, Filterung und Sortierung abrufen |
| GET | `/api/tickets/{id}` | Einzelnes Ticket abrufen |
| POST | `/api/tickets` | Neues Ticket erstellen |
| PUT | `/api/tickets/{id}` | Ticket und Status aktualisieren |
| DELETE | `/api/tickets/{id}` | Ticket löschen |

### Antworten und KI-Vorschläge

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/tickets/{id}/antworten` | Antworten eines Tickets abrufen |
| POST | `/api/tickets/{id}/antworten` | Manuelle Antwort erstellen |
| POST | `/api/tickets/{id}/ki-vorschlag` | Simulierten KI-Vorschlag erzeugen und speichern |
| DELETE | `/api/tickets/{id}/antworten/{antwortId}` | Antwort löschen |

## Pagination, Filterung und Sortierung

`GET /api/tickets` unterstützt folgende Query-Parameter:

- `page`: Seitennummer
- `pageSize`: Anzahl Tickets pro Seite
- `status`: Filter nach Status
- `kategorie`: Filter nach Kategorie
- `prioritaet`: Filter nach Priorität
- `sortBy`: Sortierfeld
- `sortDirection`: `asc` oder `desc`

Beispiel:

```text
GET /api/tickets?page=1&pageSize=10&status=Offen&sortBy=Prioritaet&sortDirection=desc
```

## Geschäftsregel

Auf geschlossene Tickets dürfen weder manuelle Antworten noch
KI-Vorschläge erstellt werden.

Die betroffenen Endpunkte liefern in diesem Fall:

```text
409 Conflict
```

Die Fehlerantwort wird als `ProblemDetails` ausgegeben.

Wird der Status eines Tickets über `PUT /api/tickets/{id}` auf
`Geschlossen` gesetzt, vergibt das Backend automatisch `GeschlossenAm`.
Bei einer Wiedereröffnung wird `GeschlossenAm` wieder entfernt.

## Fehlerbehandlung und Logging

Eine zentrale Exception-Handling-Middleware erzeugt konsistente
`ProblemDetails`-Antworten.

Verwendete HTTP-Statuscodes sind unter anderem:

- `200 OK`
- `201 Created`
- `204 No Content`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

Über `ILogger` werden zentrale Vorgänge strukturiert protokolliert:

- Ticketerstellung
- Statuswechsel
- angeforderte und generierte KI-Vorschläge
- abgelehnte Antworten
- unerwartete Fehler

## Testabdeckung

Die automatisierten Tests verwenden eine SQLite-In-Memory-Datenbank.
Der KI-Aufruf wird über `FakeKiAntwortGenerator` ohne externe Abhängigkeit
geprüft.

| Bereich | Nachweis |
|---|---|
| Antwort zu offenem Ticket | `TicketServiceTests.AntwortZuOffenemTicketWirdGespeichert` |
| Antwort zu geschlossenem Ticket | `TicketServiceTests.AntwortZuGeschlossenemTicketWirdAbgelehnt` |
| `GeschlossenAm` beim Statuswechsel | `TicketServiceTests.StatuswechselAufGeschlossenSetztGeschlossenAm` |
| KI-Aufruf mit Test-Doppelgänger | `TicketServiceTests.KiVorschlagVerwendetTestDoppelgaenger` |
| End-to-end Ticket erstellen, Liste und Statuswechsel | `TicketsApiTests` |
| Zugriff ohne JWT | `TicketsApiTests.LoeschenOhneTokenWirdNichtAutorisiert` |
| Zugriff mit falscher Rolle | `TicketsApiTests.LoeschenMitSupportRolleWirdVerboten` |
| Zugriff mit Teamleitung | `TicketsApiTests.LoeschenMitTeamleitungIstErlaubt` |
| Leerer Titel, leerer Antworttext und negative ID | `TicketsApiTests` |
| Ungültige Kategorie, Priorität und Status | `TicketsApiTests` |
| Regelbasierte Drucker-, Netzwerk- und Passwortvorschläge | `SimulierterKiAntwortGeneratorTests` |

## Manuelle Endpunktprüfung

Die Endpunkte wurden über Swagger mit positiven, negativen und
Konfliktfällen geprüft.

| Methode und Route | Erwartung | Screenshot |
|---|---:|---|
| `POST /api/auth/login` als Teamleitung | 200 | [00-login-teamleitung-200.png](Screenshots%20Testing/6.6%20Authentifizierung/00-login-teamleitung-200.png) |
| `GET /api/tickets` | 200 | [01-GET-alle-tickets.png](Screenshots%20Testing/01-GET-alle-tickets.png) |
| `GET /api/tickets/{id}` | 200 oder 404 | [02-GET-einzelnes-ticket.png](Screenshots%20Testing/02-GET-einzelnes-ticket.png) |
| `POST /api/tickets` | 201 | [03-POST-ticket-erstellen.png](Screenshots%20Testing/03-POST-ticket-erstellen.png) |
| `PUT /api/tickets/{id}` | 200, `GeschlossenAm` gesetzt | [04-PUT-ticket-schliessen.png](Screenshots%20Testing/04-PUT-ticket-schliessen.png) |
| `GET /api/tickets/{id}/antworten` | 200 | [05-GET-ticket-antworten.png](Screenshots%20Testing/05-GET-ticket-antworten.png) |
| `POST /api/tickets/{id}/antworten` | 201 | [06-POST-manuelle-antwort.png](Screenshots%20Testing/06-POST-manuelle-antwort.png) |
| `POST /api/tickets/{id}/ki-vorschlag` | 201 | [07-POST-ki-vorschlag.png](Screenshots%20Testing/07-POST-ki-vorschlag.png) |
| `DELETE /api/tickets/{id}/antworten/{antwortId}` | 204 | [10-DELETE-antwort-204.png](Screenshots%20Testing/10-DELETE-antwort-204.png) |
| Antwort bei geschlossenem Ticket | 409 | [08-POST-antwort-geschlossen-409.png](Screenshots%20Testing/08-POST-antwort-geschlossen-409.png) |
| `DELETE /api/tickets/{id}` als Teamleitung | 204 | [03-teamleitung-loeschen-204.png](Screenshots%20Testing/6.6%20Authentifizierung/03-teamleitung-loeschen-204.png) |
| `DELETE /api/tickets/{id}` als Support-Mitarbeiter | 403 | [02-support-loeschen-403.png](Screenshots%20Testing/6.6%20Authentifizierung/02-support-loeschen-403.png) |
| `GET /api/tickets` ohne JWT | 401 | [01-ohne-token-401.png](Screenshots%20Testing/6.6%20Authentifizierung/01-ohne-token-401.png) |
| KI-Vorschlag bei geschlossenem Ticket | 409 | [12-POST-ki-geschlossen-409.png](Screenshots%20Testing/12-POST-ki-geschlossen-409.png) |

## Coderichtlinien und Prüfung

- Die zentrale `.editorconfig` gilt für API- und Testprojekt.
- `EnableNETAnalyzers` und `AnalysisMode=Recommended` aktivieren die
  integrierten .NET-Analyzer.
- Typen und öffentliche Mitglieder verwenden PascalCase.
- Parameter verwenden camelCase.
- Die Einrückung erfolgt mit vier Leerzeichen.
- JSON-Dateien verwenden zwei Leerzeichen Einrückung.
- JWT-Signaturschlüssel werden nicht versioniert, sondern über User Secrets
  oder `Jwt__Key` bereitgestellt.

Konkretes Vorher-/Nachher-Beispiel einer behobenen Regelverletzung:

```csharp
// Vorher: falsche Einrückung innerhalb eines Blocks
if (ticket is null)
{
return NotFound();
}

// Nachher: vier Leerzeichen pro Blockebene gemäss .editorconfig
if (ticket is null)
{
    return NotFound();
}
```

Die betroffenen Controller- und Service-Dateien wurden formatiert. Die
Formatprüfung stellt sicher, dass die Korrektur nicht wieder verloren geht.

Die Qualitätsprüfung erfolgt mit:

```powershell
dotnet build .\HelpDesk.sln --no-restore --no-incremental
dotnet test .\HelpDesk.Api.Tests\HelpDesk.Api.Tests.csproj --no-restore
dotnet format .\HelpDesk.sln whitespace --no-restore --verify-no-changes
```

Der Build läuft ohne Warnungen durch. Alle 22 Tests und die
Whitespace-Formatprüfung sind erfolgreich. Der gespeicherte Konsolenlauf ist
unter [Testnachweise/Testlauf-2026-09-17.txt](Testnachweise/Testlauf-2026-09-17.txt)
nachvollziehbar.

## Anforderungsabgleich

| Bereich | Anforderung | Umsetzung und Nachweis | Status |
|---|---|---|---|
| Funktional | Tickets und Antworten vollständig per CRUD verwalten | Controller, Service-Layer, Swagger-Screenshots und Integrationstests | Erfüllt |
| Funktional | Keine Antwort und kein KI-Vorschlag bei geschlossenem Ticket | Zentrale Prüfung im `TicketService`, Unit-Test und 409-Screenshots | Erfüllt |
| Funktional | Pagination, Filterung und Sortierung | `GET /api/tickets` mit Query-Parametern und manueller Nachweis | Erfüllt |
| Funktional | Simulierter KI-Vorschlag nach Kategorie und Beschreibung | `IKiAntwortGenerator`, regelbasierter Generator und drei Testfälle | Erfüllt |
| Datenqualität | Nur definierte Kategorien, Prioritäten und Statuswerte | Kategorien inkl. `Zugriffsrechte`, Prioritäten `Niedrig`/`Mittel`/`Hoch`/`Kritisch`, Status `Offen`/`InBearbeitung`/`Gelöst`/`Geschlossen`; Positiv- und Negativtests | Erfüllt |
| Nichtfunktional | Saubere Schichten, DI und nachvollziehbares Logging | Controller, Service, DbContext, KI-Abstraktion und strukturiertes `ILogger` | Erfüllt |
| Nichtfunktional | Dokumentierte, lokal lauffähige API | Startanleitung, OpenAPI/Swagger, SQLite und Testlaufnachweis | Erfüllt |
| Qualität | Unit- und Integrationstests sowie Coderichtlinien | 22 Tests, `.editorconfig`, Analyzer und Formatprüfung | Erfüllt |
| Sicherheit | JWT-Login mit Rollen-Claim | Login-Endpunkt und JWT-Konfiguration | Erfüllt |
| Sicherheit | 401 ohne Token, 403 mit falscher Rolle, Löschen nur als Teamleitung | Integrationstests und Screenshots unter `Screenshots Testing/6.6 Authentifizierung` | Erfüllt |
| Sicherheit | Kein aktueller Signaturschlüssel in Konfigurationsdateien | .NET User Secrets bzw. Umgebungsvariable `Jwt__Key` | Erfüllt im aktuellen Stand; Bereinigung des früheren Git-Commits separat nötig |

## Kurzreflexion

Besonders herausfordernd war, Geschäftsregeln nicht mehrfach in den
Controllern zu verteilen, sondern zentral im Service-Layer umzusetzen. Auch die
Kombination aus JWT-Authentifizierung, Rollenprüfung und realistischen
Integrationstests erforderte eine saubere Testkonfiguration mit eigener
In-Memory-Datenbank und lokalem Testschlüssel.

Beim nächsten Mal würden wir erlaubte Ticketwerte von Anfang an zentral als
fachliche Konstanten oder Enums modellieren und Sicherheitskonfigurationen vor
dem ersten Commit über User Secrets einrichten. Zusätzlich würden wir bereits
während der Entwicklung für jede Anforderung direkt einen automatisierten Test
und einen nachvollziehbaren Nachweis ergänzen.

## Änderungsprotokoll

| Datum | Befund | Korrektur |
|---|---|---|
| 11.09.2026 | Zentrale Geschäftslogik war nicht automatisiert geprüft. | Unit-Tests mit SQLite-In-Memory und Fake-KI ergänzt. |
| 11.09.2026 | Kein End-to-end-Test und kein Authentifizierungsnachweis vorhanden. | `WebApplicationFactory`-Tests und ein erster Zugriffsschutz ergänzt. |
| 11.09.2026 | Leere Pflichtfelder und negative IDs waren nicht als Testfälle dokumentiert. | Negativtests und `[Range]`-Validierung ergänzt. |
| 11.09.2026 | Manuelle Prüfung und Konfliktfall waren nicht dokumentiert. | Testmatrix mit Request, Antwort und Screenshots ergänzt. |
| 16.09.2026 | Integrationstests verwendeten noch die frühere API-Key-Authentifizierung. | Tests auf JWT-Login und beide Rollen umgestellt. |
| 16.09.2026 | JWT-Schlüssel befand sich in der Konfigurationsdatei. | Schlüssel entfernt, ersetzt und lokal über User Secrets gespeichert. |
| 17.09.2026 | KI-Vorschläge berücksichtigten die Beschreibung nicht. | Regeln für Drucker-, Netzwerk-, Passwort- und Softwareprobleme samt Tests ergänzt. |
| 17.09.2026 | Beliebige bzw. von der Vorlage abweichende Kategorie-, Prioritäts- und Statuswerte waren möglich. | Werte an die Datenquelle angepasst sowie ein Positiv- und drei Negativtests ergänzt. |
| 17.09.2026 | Reflexion, Anforderungsabgleich und Testlaufnachweis fehlten. | Dokumentation und gespeicherten Testlauf ergänzt. |

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

1. Die Solution `HelpDesk.sln` in Visual Studio 2022 öffnen.
2. `HelpDesk.Api` als Startprojekt festlegen.
3. Das Projekt mit `F5` oder `Strg + F5` starten.
4. Swagger wird im Browser geöffnet.

Alternativ kann das API-Projekt im Terminal gestartet werden:

```bash
dotnet user-secrets set "Jwt:Key" "Einen-langen-lokalen-Schluessel-eintragen" --project HelpDesk.Api
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

Die Klasse `SimulierterKiAntwortGenerator` erzeugt anhand der
Ticketdaten einen vorlagenbasierten Antwortvorschlag. Diese Variante
wurde gewählt, weil sie keine externe Abhängigkeit und keinen API-Key
benötigt und dadurch lokal zuverlässig getestet werden kann.

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

Die Qualitätsprüfung erfolgt mit:

```powershell
dotnet build .\HelpDesk.sln --no-restore --no-incremental
dotnet test .\HelpDesk.Api.Tests\HelpDesk.Api.Tests.csproj --no-restore
dotnet format .\HelpDesk.sln whitespace --no-restore --verify-no-changes
```

Der Build läuft ohne Warnungen durch. Alle 15 Tests und die
Whitespace-Formatprüfung sind erfolgreich.

## Änderungsprotokoll

| Datum | Befund | Korrektur |
|---|---|---|
| 11.09.2026 | Zentrale Geschäftslogik war nicht automatisiert geprüft. | Unit-Tests mit SQLite-In-Memory und Fake-KI ergänzt. |
| 11.09.2026 | Kein End-to-end-Test und kein Authentifizierungsnachweis vorhanden. | `WebApplicationFactory`-Tests und ein erster Zugriffsschutz ergänzt. |
| 11.09.2026 | Leere Pflichtfelder und negative IDs waren nicht als Testfälle dokumentiert. | Negativtests und `[Range]`-Validierung ergänzt. |
| 11.09.2026 | Manuelle Prüfung und Konfliktfall waren nicht dokumentiert. | Testmatrix mit Request, Antwort und Screenshots ergänzt. |
| 16.09.2026 | Integrationstests verwendeten noch die frühere API-Key-Authentifizierung. | Tests auf JWT-Login und beide Rollen umgestellt. |
| 16.09.2026 | JWT-Schlüssel befand sich in der Konfigurationsdatei. | Schlüssel entfernt, ersetzt und lokal über User Secrets gespeichert. |

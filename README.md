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
dotnet run --project HelpDesk.Api

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
- `404 Not Found`
- `409 Conflict`
- `500 Internal Server Error`

Über `ILogger` werden zentrale Vorgänge strukturiert protokolliert:

- Ticketerstellung
- Statuswechsel
- angeforderte und generierte KI-Vorschläge
- abgelehnte Antworten
- unerwartete Fehler
# Handlungsziel 3: Anforderungen überprüfen und Korrekturen

Stand: 11.09.2026

## Automatisierte Tests

Der Testlauf erfolgt mit:

```powershell
dotnet test .\HelpDesk.Api.Tests\HelpDesk.Api.Tests.csproj
```

Die Tests verwenden eine SQLite-In-Memory-Datenbank. Der KI-Aufruf wird
über `FakeKiAntwortGenerator` getestet und benötigt keine externe
Abhängigkeit.

| Bereich | Nachweis |
|---|---|
| Antwort zu offenem Ticket | `TicketServiceTests.AntwortZuOffenemTicketWirdGespeichert` |
| Antwort zu geschlossenem Ticket | `TicketServiceTests.AntwortZuGeschlossenemTicketWirdAbgelehnt` |
| `GeschlossenAm` beim Statuswechsel | `TicketServiceTests.StatuswechselAufGeschlossenSetztGeschlossenAm` |
| KI-Aufruf mit Test-Doppelgänger | `TicketServiceTests.KiVorschlagVerwendetTestDoppelgaenger` |
| End-to-end Ticket erstellen, Liste, Statuswechsel | `TicketsApiTests` |
| Nicht autorisierter Zugriff | `TicketsApiTests.LoeschenOhneApiKeyWirdNichtAutorisiert` |
| Leerer Titel, leerer Antworttext, negative ID | `TicketsApiTests` |

## Manuelle Endpunktprüfung

Die Endpunkte werden bei laufender API über Swagger unter `/swagger` oder
über Postman geprüft. Die Antwort beim Konfliktfall muss `409 Conflict`
sein.

| Methode und Route | Erwartung | Screenshot |
|---|---:|---|
| `GET /api/tickets` | 200 | [01-GET-alle-tickets.png](../Screenshots%20Testing/01-GET-alle-tickets.png) |
| `GET /api/tickets/{id}` | 200 oder 404 | [02-GET-einzelnes-ticket.png](../Screenshots%20Testing/02-GET-einzelnes-ticket.png) |
| `POST /api/tickets` | 201 | [03-POST-ticket-erstellen.png](../Screenshots%20Testing/03-POST-ticket-erstellen.png) |
| `PUT /api/tickets/{id}` | 200, `GeschlossenAm` gesetzt | [04-PUT-ticket-schliessen.png](../Screenshots%20Testing/04-PUT-ticket-schliessen.png) |
| `GET /api/tickets/{id}/antworten` | 200 | [05-GET-ticket-antworten.png](../Screenshots%20Testing/05-GET-ticket-antworten.png) |
| `POST /api/tickets/{id}/antworten` | 201 | [06-POST-manuelle-antwort.png](../Screenshots%20Testing/06-POST-manuelle-antwort.png) |
| `POST /api/tickets/{id}/ki-vorschlag` | 201 | [07-POST-ki-vorschlag.png](../Screenshots%20Testing/07-POST-ki-vorschlag.png) |
| Antwort bei geschlossenem Ticket | 409 | [08-POST-antwort-geschlossen-409.png](../Screenshots%20Testing/08-POST-antwort-geschlossen-409.png) |
| `DELETE /api/tickets/{id}` mit `X-API-Key` | 204 | [09-DELETE-ticket-204.png](../Screenshots%20Testing/09-DELETE-ticket-204.png) |
| `DELETE /api/tickets/{id}/antworten/{antwortId}` mit `X-API-Key` | 204 | [10-DELETE-antwort-204.png](../Screenshots%20Testing/10-DELETE-antwort-204.png) |
| `DELETE /api/tickets/{id}` ohne `X-API-Key` | 401 | [11-DELETE-ohne-api-key-401.png](../Screenshots%20Testing/11-DELETE-ohne-api-key-401.png) |
| KI-Vorschlag bei geschlossenem Ticket | 409 | [12-POST-ki-geschlossen-409.png](../Screenshots%20Testing/12-POST-ki-geschlossen-409.png) |

## Änderungsprotokoll

| Datum | Befund | Korrektur |
|---|---|---|
| 11.09.2026 | Zentrale Geschäftslogik war nicht automatisiert geprüft. | Unit-Tests mit SQLite-In-Memory und Fake-KI ergänzt. |
| 11.09.2026 | Kein End-to-end-Test und kein Authentifizierungsnachweis vorhanden. | `WebApplicationFactory`-Tests und API-Key-Schutz für Löschroute ergänzt. |
| 11.09.2026 | Leere Pflichtfelder und negative IDs waren nicht als Testfälle dokumentiert. | Drei Negativtests sowie `[Range]`-Validierung für Ticket-IDs ergänzt. |
| 11.09.2026 | Manuelle Prüfung und Konfliktfall waren nicht festgehalten. | Testmatrix mit Request, erwarteter Antwort und Screenshot-Spalte ergänzt. |
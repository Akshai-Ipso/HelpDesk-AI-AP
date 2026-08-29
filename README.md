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
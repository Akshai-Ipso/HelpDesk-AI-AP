# Handlungsziel 4: Coderichtlinien

Stand: 12.09.2026

## Aktivierte Regeln

- Die zentrale `.editorconfig` gilt für API- und Testprojekt.
- `EnableNETAnalyzers` und `AnalysisMode=Recommended` aktivieren die
  integrierten .NET-Analyzer ohne zusätzliche StyleCop-Abhängigkeit.
- Typen und Mitglieder verwenden PascalCase.
- Parameter verwenden camelCase.
- Einrückung erfolgt mit vier Leerzeichen; öffnende Klammern stehen nach
  Microsoft-Konventionen in einer neuen Zeile.
- Bei `var` wird zwischen eingebautem Typ, offensichtlichem Typ und sonstigen
  Fällen unterschieden.
- JSON-Dateien verwenden zwei Leerzeichen Einrückung.
- API-Schlüssel werden nicht versioniert. Für lokale Tests wird der Wert über
  `Security__ApiKey` als Umgebungsvariable gesetzt.

## Korrekturen

| Vorher | Nachher |
|---|---|
| Veralteter `ISystemClock`-Konstruktor im Auth-Handler | Aktueller `TimeProvider`-Konstruktor |
| Fehlende XML-Parameterdokumentation für zwei Controller-Methoden | `dto`-Parameter mit `<param>` dokumentiert |
| Keine zentrale Formatierungs- und Namenskonfiguration | `.editorconfig` im Solution-Root aktiviert |
| API-Key in Entwicklungs-Konfiguration | Schlüssel aus JSON entfernt und auf Umgebungsvariable umgestellt |

## Prüfung

```powershell
dotnet build .\HelpDesk.sln --no-restore --no-incremental
dotnet test .\HelpDesk.Api.Tests\HelpDesk.Api.Tests.csproj --no-restore
```

Der Build ist ohne Warnungen durchgelaufen; die Tests bleiben als separate
Verhaltensprüfung Bestandteil der Solution.
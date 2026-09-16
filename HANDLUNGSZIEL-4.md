# Handlungsziel 4: Coderichtlinien

Stand: 16.09.2026

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
- JWT-Signaturschlüssel werden nicht versioniert. Für die lokale Ausführung
  wird der Wert über User Secrets oder `Jwt__Key` bereitgestellt.

## Korrekturen

| Vorher | Nachher |
|---|---|
| Frühere API-Key-Authentifizierung | Durch JWT-Bearer-Authentifizierung mit Rollen-Claims ersetzt |
| Fehlende XML-Parameterdokumentation für zwei Controller-Methoden | `dto`-Parameter mit `<param>` dokumentiert |
| Keine zentrale Formatierungs- und Namenskonfiguration | `.editorconfig` im Solution-Root aktiviert |
| JWT-Schlüssel in Konfigurationsdatei | Schlüssel wird über User Secrets oder `Jwt__Key` bereitgestellt |

## Prüfung

```powershell
dotnet build .\HelpDesk.sln --no-restore --no-incremental
dotnet test .\HelpDesk.Api.Tests\HelpDesk.Api.Tests.csproj --no-restore
dotnet format .\HelpDesk.sln whitespace --no-restore --verify-no-changes
```

Der Build läuft ohne Warnungen durch. Alle 15 Tests und die
Whitespace-Formatprüfung sind erfolgreich.

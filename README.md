# Lift2

Eine Windows Forms Anwendung mit .NET 9.

## Projektstruktur

- **Lift2App**: Die Haupt-Windows-Forms-Anwendung

## Anforderungen

- .NET 9 SDK oder höher

## Erstellen und Ausführen

```bash
cd Lift2App
dotnet build
dotnet run
```

## Technologie

- .NET 9.0
- Windows Forms
- C#

## Branches

Das Projekt verwendet folgende Branch-Struktur:

### main
Der Hauptbranch, der den stabilen, produktionsreifen Code enthält. Alle Features werden durch Pull Requests in diesen Branch gemerged.

### Feature-Branches
Feature-Branches folgen dem Namensschema `copilot/*` oder `feature/*` und werden für neue Funktionen oder Verbesserungen erstellt:

- **copilot/create-windows-forms-app-dotnet9**: Initiale Erstellung der Windows Forms Anwendung mit .NET 9 (gemerged)
- **copilot/overview-of-branches**: Dokumentation der Branch-Struktur (aktuell)

### Workflow
1. Erstelle einen Feature-Branch vom `main` Branch
2. Implementiere die Änderungen im Feature-Branch
3. Erstelle einen Pull Request zum `main` Branch
4. Nach Review und Tests wird der Pull Request gemerged

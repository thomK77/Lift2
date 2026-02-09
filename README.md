# Lift2

Eine Windows Forms Anwendung mit .NET 9.

## Funktionen

### Drag & Drop Launcher
Die Anwendung unterstützt Drag & Drop, um Dateien mit erhöhten Rechten (Administrator) zu starten:
- Ziehen Sie eine Datei auf das MainForm-Fenster
- Die Datei wird automatisch mit den gleichen Rechten gestartet, mit denen die Lift2-Anwendung läuft
- Unterstützt verschiedene Dateitypen (EXE, TXT, etc.)
- Fehlermeldungen werden angezeigt, falls Probleme auftreten
- **Drag & Drop funktioniert auch von nicht-erhöhten Anwendungen** (z.B. normaler Windows Explorer), selbst wenn Lift2 mit Administrator-Rechten läuft

**Hinweis**: Um Dateien mit Administrator-Rechten zu starten, muss die Lift2-Anwendung selbst mit Administrator-Rechten ausgeführt werden.

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

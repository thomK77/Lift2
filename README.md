# Lift2

Eine Windows Forms Anwendung mit .NET 9.

## Funktionen

### Single-Instance Architektur
Lift2 verwendet eine Single-Instance-Architektur mit IPC (Inter-Process Communication):
- Nur eine Instanz der Anwendung kann gleichzeitig laufen
- Wenn bereits eine Instanz läuft und eine neue mit einem Dateipfad gestartet wird, wird der Dateipfad an die laufende Instanz übergeben
- Die neue Instanz beendet sich automatisch nach erfolgreicher Übergabe
- Kommunikation erfolgt über Named Pipes (`\\.\pipe\Lift2Pipe`)
- Mutex-basierte Erkennung (`Global\Lift2AppMutex`)

### Drag & Drop Launcher
Die Anwendung unterstützt Drag & Drop, um Dateien mit erhöhten Rechten (Administrator) zu starten:
- Ziehen Sie eine Datei auf das MainForm-Fenster
- Die Datei wird automatisch mit den gleichen Rechten gestartet, mit denen die Lift2-Anwendung läuft
- Unterstützt verschiedene Dateitypen (EXE, TXT, etc.)
- Fehlermeldungen werden angezeigt, falls Probleme auftreten
- **Drag & Drop funktioniert auch von nicht-erhöhten Anwendungen** (z.B. normaler Windows Explorer), selbst wenn Lift2 mit Administrator-Rechten läuft

**Hinweis**: Um Dateien mit Administrator-Rechten zu starten, muss die Lift2-Anwendung selbst mit Administrator-Rechten ausgeführt werden.

### Kommandozeilenparameter
Die Anwendung akzeptiert einen Dateipfad als Kommandozeilenargument:
```bash
Lift2App.exe "C:\Pfad\zur\Datei.txt"
```
- Wenn bereits eine Instanz läuft, wird die Datei an die laufende Instanz übergeben
- Wenn keine Instanz läuft, wird die Anwendung gestartet und die Datei geöffnet

## Windows Context-Menü Integration

Um Lift2 in das Windows Context-Menü zu integrieren, sodass Sie Dateien per Rechtsklick mit Lift2 öffnen können:

### Automatische Installation (Registry-Datei)

Erstellen Sie eine `.reg`-Datei mit folgendem Inhalt:

```reg
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\*\shell\Lift2]
@="Mit Lift2 öffnen"
"Icon"="C:\\Pfad\\zu\\Lift2App.exe,0"

[HKEY_CLASSES_ROOT\*\shell\Lift2\command]
@="\"C:\\Pfad\\zu\\Lift2App.exe\" \"%1\""
```

**Wichtig**: Ersetzen Sie `C:\\Pfad\\zu\\Lift2App.exe` mit dem tatsächlichen Pfad zur Lift2App.exe-Datei (beachten Sie die doppelten Backslashes).

Führen Sie die `.reg`-Datei mit Administrator-Rechten aus, um die Änderungen in die Registry zu übernehmen.

### Manuelle Installation

1. Öffnen Sie den Registry-Editor (`regedit.exe`) mit Administrator-Rechten
2. Navigieren Sie zu `HKEY_CLASSES_ROOT\*\shell`
3. Erstellen Sie einen neuen Schlüssel mit dem Namen `Lift2`
4. Setzen Sie den Standardwert (Default) auf `Mit Lift2 öffnen`
5. (Optional) Erstellen Sie einen String-Wert `Icon` und setzen Sie ihn auf `C:\Pfad\zu\Lift2App.exe,0`
6. Erstellen Sie einen Unterschlüssel `command` unter `Lift2`
7. Setzen Sie den Standardwert auf `"C:\Pfad\zu\Lift2App.exe" "%1"` (mit Anführungszeichen)

### Deinstallation

Um die Context-Menü-Integration zu entfernen:

**Mit Registry-Datei:**
```reg
Windows Registry Editor Version 5.00

[-HKEY_CLASSES_ROOT\*\shell\Lift2]
```

**Manuell:**
1. Öffnen Sie `regedit.exe` mit Administrator-Rechten
2. Navigieren Sie zu `HKEY_CLASSES_ROOT\*\shell`
3. Löschen Sie den Schlüssel `Lift2`

### Verwendung

Nach der Installation:
1. Starten Sie Lift2 mit Administrator-Rechten und lassen Sie es im Hintergrund laufen
2. Klicken Sie mit der rechten Maustaste auf eine beliebige Datei im Windows Explorer
3. Wählen Sie "Mit Lift2 öffnen" aus dem Kontextmenü
4. Eine neue Lift2-Instanz wird kurz gestartet (UAC-Abfrage), erkennt die laufende Instanz und übergibt den Dateipfad
5. Die laufende Instanz öffnet die Datei mit Administrator-Rechten
6. Die neue Instanz beendet sich automatisch

## Projektstruktur

- **Lift2App**: Die Haupt-Windows-Forms-Anwendung

## Anforderungen

- .NET 9 SDK oder höher
- Windows-Betriebssystem (für Named Pipes und Context-Menü)

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
- Named Pipes für IPC
- Mutex für Single-Instance-Erkennung

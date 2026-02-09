using System.IO.Pipes;
using System.Text;
using System.Diagnostics;

namespace Lift2App;

static class Program
{
    private const string PipeName = "Lift2Pipe";

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Datei aus Kommandozeile
        string? filePath = args.Length > 0 ? args[0] : null;
        
        if (!string.IsNullOrEmpty(filePath))
        {
            // Versuche an laufende Admin-Instanz zu senden
            if (TryConnectToPipe(filePath))
            {
                // Erfolgreich gesendet, beenden
                return;
            }
            
            // Keine Admin-Instanz läuft → Neue starten mit Admin-Rechten
            StartElevatedInstance(filePath);
            return;
        }
        
        // Keine Datei → Normale Instanz starten
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(filePath));
    }

    private static bool TryConnectToPipe(string filePath)
    {
        try
        {
            using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
            {
                client.Connect(1000); // 1 Sekunde Timeout
                byte[] buffer = Encoding.UTF8.GetBytes(filePath);
                client.Write(buffer, 0, buffer.Length);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private static void StartElevatedInstance(string filePath)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Application.ExecutablePath,
                Arguments = $"\"{filePath}\"",
                Verb = "runas", // Admin-Rechte anfordern
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fehler beim Starten der Admin-Instanz:\n{ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
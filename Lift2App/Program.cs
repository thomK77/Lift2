using System.IO.Pipes;
using System.Text;

namespace Lift2App;

static class Program
{
    private const string MutexName = "Global\\Lift2AppMutex";
    private const string PipeName = "Lift2Pipe";
    private const int IpcTimeoutMs = 5000; // 5 seconds timeout
    private static Mutex? appMutex; // Hold mutex for application lifetime

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Check if a file path was passed as command-line argument
        string? filePath = args.Length > 0 ? args[0] : null;

        // Try to create or open the mutex
        bool createdNew;
        appMutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // Another instance is already running
            if (!string.IsNullOrEmpty(filePath))
            {
                // Try to send the file path to the running instance via IPC
                if (SendFilePathToRunningInstance(filePath))
                {
                    // Successfully sent, exit this instance
                    appMutex.Dispose();
                    return;
                }
                else
                {
                    // IPC failed, start a new instance anyway
                    MessageBox.Show(
                        "Konnte nicht mit der laufenden Instanz kommunizieren.\nStarte neue Instanz.",
                        "Warnung",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                // No file path to send, just inform the user
                MessageBox.Show(
                    "Eine Instanz von Lift2 läuft bereits.",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                appMutex.Dispose();
                return;
            }
        }

        try
        {
            // This is the first instance or IPC failed
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Create and run the main form, passing the file path if provided
            var mainForm = new MainForm(filePath);
            Application.Run(mainForm);
        }
        finally
        {
            // Release the mutex when application exits
            appMutex?.Dispose();
        }
    }

    /// <summary>
    /// Sends a file path to the running instance via Named Pipe.
    /// </summary>
    /// <param name="filePath">The file path to send.</param>
    /// <returns>True if successful, false otherwise.</returns>
    private static bool SendFilePathToRunningInstance(string filePath)
    {
        try
        {
            using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
            {
                // Try to connect with timeout
                client.Connect(IpcTimeoutMs);

                // Send the file path
                byte[] data = Encoding.UTF8.GetBytes(filePath);
                client.Write(data, 0, data.Length);
                client.Flush();

                return true;
            }
        }
        catch (TimeoutException)
        {
            // Connection timeout
            return false;
        }
        catch (IOException)
        {
            // Pipe not available or other IO error
            return false;
        }
        catch (Exception)
        {
            // Other errors
            return false;
        }
    }
}
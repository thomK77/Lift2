namespace Lift2App;

static class Program
{
    private const string MutexName = "Global\\Lift2AppMutex";
    private static Mutex? appMutex; // Hold mutex for application lifetime

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Try to create or open the mutex
        bool createdNew;
        appMutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // Another instance is already running, inform the user
            MessageBox.Show(
                "Eine Instanz von Lift2 läuft bereits.",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            appMutex.Dispose();
            return;
        }

        try
        {
            // This is the first instance
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Create and run the main form
            var mainForm = new MainForm();
            Application.Run(mainForm);
        }
        finally
        {
            // Release the mutex when application exits
            appMutex?.Dispose();
        }
    }
}
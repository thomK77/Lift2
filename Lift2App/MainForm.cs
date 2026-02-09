using System.Diagnostics;

namespace Lift2App;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_DragEnter(object? sender, DragEventArgs e)
    {
        // Check if the data being dragged is a file
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Allow the drop operation and provide visual feedback
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void MainForm_DragDrop(object? sender, DragEventArgs e)
    {
        try
        {
            // Get the files that were dropped
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];

                if (files != null && files.Length > 0)
                {
                    foreach (string filePath in files)
                    {
                        // Display the dropped file path
                        labelDroppedFile.Text = $"Gestartete Datei: {filePath}";

                        // Check if file exists
                        if (!File.Exists(filePath))
                        {
                            MessageBox.Show(
                                $"Die Datei existiert nicht:\n{filePath}",
                                "Fehler",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            continue;
                        }

                        // Start the file with the same privileges as the current process
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true,
                            // If Lift2 is running with admin rights, the started process will inherit them
                            Verb = "" // Empty verb means use default action
                        };

                        Process.Start(startInfo);

                        MessageBox.Show(
                            $"Datei wurde gestartet:\n{filePath}",
                            "Erfolg",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            // Handle cases where the file type is not associated or cannot be executed
            MessageBox.Show(
                $"Fehler beim Starten der Datei:\n{ex.Message}\n\nMögliche Ursachen:\n- Keine Zuordnung für diesen Dateityp\n- Fehlende Berechtigung",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            // Handle any other errors
            MessageBox.Show(
                $"Unerwarteter Fehler:\n{ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}

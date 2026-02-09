using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Lift2App;

public partial class MainForm : Form
{
    // P/Invoke declarations for Windows UIPI (User Interface Privilege Isolation) bypass
    // These are needed to allow drag & drop from non-elevated applications when running with admin rights
    [DllImport("user32.dll")]
    private static extern bool ChangeWindowMessageFilterEx(IntPtr hwnd, uint message, uint action, IntPtr pChangeFilterStruct);

    private const uint WM_DROPFILES = 0x0233;
    private const uint WM_COPYDATA = 0x004A;
    private const uint WM_COPYGLOBALDATA = 0x0049;
    private const uint MSGFLT_ADD = 1;

    public MainForm()
    {
        InitializeComponent();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // UIPI-Bypass: Allow drag & drop from non-elevated applications
        // When running with admin rights, Windows blocks drag & drop from normal applications
        // for security reasons. These calls explicitly allow the necessary messages.
        // Note: WM_COPYDATA and WM_COPYGLOBALDATA are included for complete drag & drop support,
        // as some operations may use these messages for inter-process data transfer.
        ChangeWindowMessageFilterEx(this.Handle, WM_DROPFILES, MSGFLT_ADD, IntPtr.Zero);
        ChangeWindowMessageFilterEx(this.Handle, WM_COPYDATA, MSGFLT_ADD, IntPtr.Zero);
        ChangeWindowMessageFilterEx(this.Handle, WM_COPYGLOBALDATA, MSGFLT_ADD, IntPtr.Zero);
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
                    List<string> successfulFiles = new List<string>();
                    List<string> failedFiles = new List<string>();

                    foreach (string filePath in files)
                    {
                        try
                        {
                            // Check if file exists
                            if (!File.Exists(filePath))
                            {
                                failedFiles.Add($"{filePath} (Datei existiert nicht)");
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

                            Process? startedProcess = Process.Start(startInfo);
                            if (startedProcess != null)
                            {
                                successfulFiles.Add(filePath);
                            }
                            else
                            {
                                failedFiles.Add($"{filePath} (Prozess konnte nicht gestartet werden)");
                            }
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            failedFiles.Add($"{filePath} ({ex.Message})");
                        }
                        catch (Exception ex)
                        {
                            failedFiles.Add($"{filePath} ({ex.Message})");
                        }
                    }

                    // Update the label with summary
                    if (successfulFiles.Count > 0)
                    {
                        labelDroppedFile.Text = $"Gestartet: {successfulFiles.Count} Datei(en)";
                    }

                    // Show summary message
                    string message = "";
                    if (successfulFiles.Count > 0)
                    {
                        message += $"Erfolgreich gestartet: {successfulFiles.Count} Datei(en)\n";
                        if (successfulFiles.Count <= 5)
                        {
                            message += string.Join("\n", successfulFiles.Select(f => $"  • {Path.GetFileName(f)}"));
                        }
                    }

                    if (failedFiles.Count > 0)
                    {
                        if (message.Length > 0) message += "\n\n";
                        message += $"Fehler bei {failedFiles.Count} Datei(en):\n";
                        message += string.Join("\n", failedFiles.Select(f => $"  • {f}"));
                    }

                    if (message.Length > 0)
                    {
                        MessageBox.Show(
                            message,
                            failedFiles.Count > 0 ? "Ergebnis" : "Erfolg",
                            MessageBoxButtons.OK,
                            failedFiles.Count > 0 && successfulFiles.Count == 0 ? MessageBoxIcon.Error : MessageBoxIcon.Information);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any other unexpected errors
            MessageBox.Show(
                $"Unerwarteter Fehler:\n{ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}

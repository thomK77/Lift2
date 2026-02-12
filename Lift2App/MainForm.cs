using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Text;

namespace Lift2App;

public partial class MainForm : Form
{
    // P/Invoke declarations for Windows UIPI (User Interface Privilege Isolation) bypass
    // These are needed to allow drag & drop from non-elevated applications when running with admin rights
    [DllImport("user32.dll")]
    private static extern bool ChangeWindowMessageFilterEx(IntPtr hwnd, uint message, uint action, IntPtr pChangeFilterStruct);

    private const uint WM_DRAGENTER = 0x0231;
    private const uint WM_DRAGOVER = 0x0232;
    private const uint WM_DROPFILES = 0x0233;
    private const uint WM_COPYDATA = 0x004A;
    private const uint WM_COPYGLOBALDATA = 0x0049;
    private const uint MSGFLT_ADD = 1;

    private const string PipeName = "Lift2Pipe";
    private Thread? pipeServerThread;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public MainForm(string? initialFilePath = null)
    {
        InitializeComponent();
        
        // Start Named Pipe Server
        pipeServerThread = new Thread(RunPipeServer)
        {
            IsBackground = true
        };
        pipeServerThread.Start();
        
        // Initial file öffnen falls vorhanden
        if (!string.IsNullOrEmpty(initialFilePath))
        {
            OpenFile(initialFilePath);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // UIPI-Bypass: Allow drag & drop from non-elevated applications
        // When running with admin rights, Windows blocks drag & drop from normal applications
        // for security reasons. These calls explicitly allow the necessary messages.
        ChangeWindowMessageFilterEx(this.Handle, WM_DRAGENTER, MSGFLT_ADD, IntPtr.Zero);
        ChangeWindowMessageFilterEx(this.Handle, WM_DRAGOVER, MSGFLT_ADD, IntPtr.Zero);
        ChangeWindowMessageFilterEx(this.Handle, WM_DROPFILES, MSGFLT_ADD, IntPtr.Zero);
        ChangeWindowMessageFilterEx(this.Handle, WM_COPYDATA, MSGFLT_ADD, IntPtr.Zero);
        ChangeWindowMessageFilterEx(this.Handle, WM_COPYGLOBALDATA, MSGFLT_ADD, IntPtr.Zero);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        
        // Signal cancellation to the pipe server thread
        cancellationTokenSource.Cancel();
        
        // Wait for the pipe server thread to complete (with timeout)
        if (pipeServerThread != null && pipeServerThread.IsAlive)
        {
            pipeServerThread.Join(1000); // Wait up to 1 second
        }
        
        // Now safe to dispose the cancellation token source
        cancellationTokenSource.Dispose();
    }

    private void RunPipeServer()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                using (var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous))
                {
                    // Use async wait with cancellation token
                    var connectTask = server.WaitForConnectionAsync(cancellationTokenSource.Token);
                    
                    try
                    {
                        connectTask.Wait(cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    
                    if (cancellationTokenSource.Token.IsCancellationRequested) break;
                    
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    
                    try
                    {
                        // Read with timeout to prevent indefinite blocking
                        var readTask = server.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token);
                        readTask.Wait(5000); // 5 second timeout
                        bytesRead = readTask.Result;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
                    {
                        break;
                    }
                    
                    if (bytesRead > 0)
                    {
                        string filePath = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            // Check if form is still valid before invoking
                            if (!cancellationTokenSource.Token.IsCancellationRequested && !this.IsDisposed)
                            {
                                this.Invoke(new Action(() => OpenFile(filePath)));
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                // I/O errors are expected when shutting down
                if (cancellationTokenSource.Token.IsCancellationRequested) break;
            }
            catch (ObjectDisposedException)
            {
                // Expected during shutdown
                break;
            }
        }
    }

    /// <summary>
    /// Opens a file with the same privileges as the current process.
    /// </summary>
    /// <param name="filePath">The file path to open.</param>
    private void OpenFile(string filePath)
    {
        try
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                MessageBox.Show(
                    $"Die Datei existiert nicht:\n{filePath}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
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
                labelDroppedFile.Text = $"Gestartet: {Path.GetFileName(filePath)}";
                MessageBox.Show(
                    $"Datei erfolgreich gestartet:\n{Path.GetFileName(filePath)}",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"Prozess konnte nicht gestartet werden:\n{filePath}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            MessageBox.Show(
                $"Fehler beim Starten der Datei:\n{filePath}\n\n{ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unerwarteter Fehler:\n{ex.Message}",
                "Fehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
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

    private void ButtonOpenFile_Click(object? sender, EventArgs e)
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Title = "Datei zum Ausführen auswählen";
            openFileDialog.Filter = "Alle Dateien (*.*)|*.*";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                OpenFile(selectedFilePath);
            }
        }
    }
}

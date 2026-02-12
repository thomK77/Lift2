namespace Lift2App;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        labelInstructions = new Label();
        labelDroppedFile = new Label();
        buttonOpenFile = new Button();
        SuspendLayout();
        // 
        // labelInstructions
        // 
        labelInstructions.AutoSize = true;
        labelInstructions.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        labelInstructions.Location = new Point(200, 150);
        labelInstructions.Name = "labelInstructions";
        labelInstructions.TabIndex = 0;
        labelInstructions.Text = "Dateien hier ablegen (Drag & Drop)\r\num sie mit Administrator-Rechten zu starten";
        labelInstructions.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // labelDroppedFile
        // 
        labelDroppedFile.AutoSize = true;
        labelDroppedFile.Location = new Point(50, 250);
        labelDroppedFile.Name = "labelDroppedFile";
        labelDroppedFile.Size = new Size(0, 15);
        labelDroppedFile.TabIndex = 1;
        // 
        // buttonOpenFile
        // 
        buttonOpenFile.Font = new Font("Segoe UI", 12F);
        buttonOpenFile.Location = new Point(300, 300);
        buttonOpenFile.Name = "buttonOpenFile";
        buttonOpenFile.Size = new Size(200, 50);
        buttonOpenFile.TabIndex = 2;
        buttonOpenFile.Text = "Datei öffnen...";
        buttonOpenFile.UseVisualStyleBackColor = true;
        buttonOpenFile.Click += ButtonOpenFile_Click;
        // 
        // MainForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(buttonOpenFile);
        Controls.Add(labelDroppedFile);
        Controls.Add(labelInstructions);
        Name = "MainForm";
        Text = "Lift2 - Drag & Drop Launcher";
        DragDrop += MainForm_DragDrop;
        DragEnter += MainForm_DragEnter;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label labelInstructions;
    private Label labelDroppedFile;
    private Button buttonOpenFile;
}

namespace Personal_Notes_Manager
{
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
            SaveNoteButton = new Button();
            splitContainer1 = new SplitContainer();
            lstNotes = new ListBox();
            rtbNoteBody = new RichTextBox();
            NewNoteButton = new Button();
            DelNoteButton = new Button();
            BottomUI = new Panel();
            txtNoteTitle = new TextBox();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            BottomUI.SuspendLayout();
            SuspendLayout();
            // 
            // SaveNoteButton
            // 
            SaveNoteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            SaveNoteButton.Location = new Point(181, 5);
            SaveNoteButton.Name = "SaveNoteButton";
            SaveNoteButton.Size = new Size(75, 23);
            SaveNoteButton.TabIndex = 0;
            SaveNoteButton.Text = "Save";
            SaveNoteButton.UseVisualStyleBackColor = true;
            SaveNoteButton.Click += SaveNote;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lstNotes);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(rtbNoteBody);
            splitContainer1.Size = new Size(1059, 752);
            splitContainer1.SplitterDistance = 249;
            splitContainer1.TabIndex = 1;
            // 
            // lstNotes
            // 
            lstNotes.Dock = DockStyle.Fill;
            lstNotes.FormattingEnabled = true;
            lstNotes.Items.AddRange(new object[] { "1", "2" });
            lstNotes.Location = new Point(0, 0);
            lstNotes.Name = "lstNotes";
            lstNotes.Size = new Size(249, 752);
            lstNotes.TabIndex = 0;
            lstNotes.SelectedIndexChanged += GetNote;
            // 
            // rtbNoteBody
            // 
            rtbNoteBody.Dock = DockStyle.Fill;
            rtbNoteBody.Location = new Point(0, 0);
            rtbNoteBody.Name = "rtbNoteBody";
            rtbNoteBody.Size = new Size(806, 752);
            rtbNoteBody.TabIndex = 0;
            rtbNoteBody.Text = "";
            // 
            // NewNoteButton
            // 
            NewNoteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            NewNoteButton.Location = new Point(12, 5);
            NewNoteButton.Name = "NewNoteButton";
            NewNoteButton.Size = new Size(75, 23);
            NewNoteButton.TabIndex = 2;
            NewNoteButton.Text = "New Note";
            NewNoteButton.UseVisualStyleBackColor = true;
            NewNoteButton.Click += CreateNote;
            // 
            // DelNoteButton
            // 
            DelNoteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            DelNoteButton.Location = new Point(93, 5);
            DelNoteButton.Name = "DelNoteButton";
            DelNoteButton.Size = new Size(82, 23);
            DelNoteButton.TabIndex = 4;
            DelNoteButton.Text = "Delete Note";
            DelNoteButton.UseVisualStyleBackColor = true;
            DelNoteButton.Click += DeleteNote;
            // 
            // BottomUI
            // 
            BottomUI.Controls.Add(txtNoteTitle);
            BottomUI.Controls.Add(label1);
            BottomUI.Controls.Add(NewNoteButton);
            BottomUI.Controls.Add(SaveNoteButton);
            BottomUI.Controls.Add(DelNoteButton);
            BottomUI.Dock = DockStyle.Bottom;
            BottomUI.Location = new Point(0, 712);
            BottomUI.Name = "BottomUI";
            BottomUI.Size = new Size(1059, 40);
            BottomUI.TabIndex = 5;
            // 
            // txtNoteTitle
            // 
            txtNoteTitle.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtNoteTitle.Location = new Point(343, 6);
            txtNoteTitle.Name = "txtNoteTitle";
            txtNoteTitle.Size = new Size(100, 23);
            txtNoteTitle.TabIndex = 6;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(262, 9);
            label1.Name = "label1";
            label1.Size = new Size(75, 15);
            label1.TabIndex = 5;
            label1.Text = "Note Title ->";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1059, 752);
            Controls.Add(BottomUI);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Personal Notes Manager";
            FormClosing += OnFormClosing;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            BottomUI.ResumeLayout(false);
            BottomUI.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button SaveNoteButton;
        private SplitContainer splitContainer1;
        private Button NewNoteButton;
        private ListBox lstNotes;
        private RichTextBox rtbNoteBody;
        private Button DelNoteButton;
        private Panel BottomUI;
        private Label label1;
        private TextBox txtNoteTitle;
    }
}

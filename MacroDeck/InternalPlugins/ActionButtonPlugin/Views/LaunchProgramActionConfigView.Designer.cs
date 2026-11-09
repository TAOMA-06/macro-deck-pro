
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SuchByte.MacroDeck.GUI.CustomControls;

namespace SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.Views
{
    partial class LaunchProgramActionConfigView
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.lblFilePath = new Label();
            this.txtFilePath = new RoundedTextBox();
            this.btnBrowse = new ButtonPrimary();
            this.picIconPreview = new PictureBox();
            this.lblExtracted = new Label();
            this.lblArguments = new Label();
            this.txtArguments = new RoundedTextBox();
            this.lblWorkingDirectory = new Label();
            this.txtWorkingDirectory = new RoundedTextBox();
            this.btnWorkingDirBrowse = new ButtonPrimary();
            ((System.ComponentModel.ISupportInitialize)(this.picIconPreview)).BeginInit();
            this.SuspendLayout();
            //
            // lblFilePath
            //
            this.lblFilePath.Anchor = AnchorStyles.None;
            this.lblFilePath.Font = new Font("Tahoma", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblFilePath.Location = new Point(70, 80);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new Size(140, 25);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "File path";
            this.lblFilePath.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtFilePath
            //
            this.txtFilePath.Anchor = AnchorStyles.None;
            this.txtFilePath.BackColor = Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
            this.txtFilePath.Cursor = Cursors.Hand;
            this.txtFilePath.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.txtFilePath.Icon = null;
            this.txtFilePath.Location = new Point(216, 80);
            this.txtFilePath.MaxCharacters = 32767;
            this.txtFilePath.Multiline = false;
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Padding = new Padding(8, 5, 8, 5);
            this.txtFilePath.PasswordChar = false;
            this.txtFilePath.PlaceHolderColor = Color.Gray;
            this.txtFilePath.PlaceHolderText = "Select executable (.exe)";
            this.txtFilePath.ReadOnly = false;
            this.txtFilePath.ScrollBars = ScrollBars.None;
            this.txtFilePath.SelectionStart = 0;
            this.txtFilePath.Size = new Size(360, 25);
            this.txtFilePath.TabIndex = 1;
            this.txtFilePath.TextAlignment = HorizontalAlignment.Left;
            //
            // btnBrowse
            //
            this.btnBrowse.Anchor = AnchorStyles.None;
            this.btnBrowse.BorderRadius = 8;
            this.btnBrowse.Cursor = Cursors.Hand;
            this.btnBrowse.FlatAppearance.BorderSize = 0;
            this.btnBrowse.FlatStyle = FlatStyle.Flat;
            this.btnBrowse.Font = new Font("Tahoma", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnBrowse.ForeColor = Color.White;
            this.btnBrowse.HoverColor = Color.Empty;
            this.btnBrowse.Icon = null;
            this.btnBrowse.Location = new Point(582, 80);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Progress = 0;
            this.btnBrowse.ProgressColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(103)))), ((int)(((byte)(205)))));
            this.btnBrowse.Size = new Size(80, 25);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.UseWindowsAccentColor = true;
            this.btnBrowse.Click += new EventHandler(this.BtnBrowse_Click);
            //
            // picIconPreview
            //
            this.picIconPreview.Anchor = AnchorStyles.None;
            this.picIconPreview.BackColor = Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.picIconPreview.BorderStyle = BorderStyle.FixedSingle;
            this.picIconPreview.Location = new Point(680, 74);
            this.picIconPreview.Name = "picIconPreview";
            this.picIconPreview.Size = new Size(40, 40);
            this.picIconPreview.SizeMode = PictureBoxSizeMode.Zoom;
            this.picIconPreview.TabIndex = 3;
            this.picIconPreview.TabStop = false;
            //
            // lblExtracted
            //
            this.lblExtracted.Anchor = AnchorStyles.None;
            this.lblExtracted.Font = new Font("Tahoma", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblExtracted.Location = new Point(726, 74);
            this.lblExtracted.Name = "lblExtracted";
            this.lblExtracted.Size = new Size(25, 40);
            this.lblExtracted.TabIndex = 4;
            this.lblExtracted.Text = "";
            this.lblExtracted.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblArguments
            //
            this.lblArguments.Anchor = AnchorStyles.None;
            this.lblArguments.Font = new Font("Tahoma", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblArguments.Location = new Point(70, 130);
            this.lblArguments.Name = "lblArguments";
            this.lblArguments.Size = new Size(140, 25);
            this.lblArguments.TabIndex = 5;
            this.lblArguments.Text = "Arguments";
            this.lblArguments.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtArguments
            //
            this.txtArguments.Anchor = AnchorStyles.None;
            this.txtArguments.BackColor = Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
            this.txtArguments.Cursor = Cursors.Hand;
            this.txtArguments.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.txtArguments.Icon = null;
            this.txtArguments.Location = new Point(216, 130);
            this.txtArguments.MaxCharacters = 32767;
            this.txtArguments.Multiline = false;
            this.txtArguments.Name = "txtArguments";
            this.txtArguments.Padding = new Padding(8, 5, 8, 5);
            this.txtArguments.PasswordChar = false;
            this.txtArguments.PlaceHolderColor = Color.Gray;
            this.txtArguments.PlaceHolderText = "Optional command-line arguments";
            this.txtArguments.ReadOnly = false;
            this.txtArguments.ScrollBars = ScrollBars.None;
            this.txtArguments.SelectionStart = 0;
            this.txtArguments.Size = new Size(530, 25);
            this.txtArguments.TabIndex = 6;
            this.txtArguments.TextAlignment = HorizontalAlignment.Left;
            //
            // lblWorkingDirectory
            //
            this.lblWorkingDirectory.Anchor = AnchorStyles.None;
            this.lblWorkingDirectory.Font = new Font("Tahoma", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblWorkingDirectory.Location = new Point(70, 180);
            this.lblWorkingDirectory.Name = "lblWorkingDirectory";
            this.lblWorkingDirectory.Size = new Size(140, 25);
            this.lblWorkingDirectory.TabIndex = 7;
            this.lblWorkingDirectory.Text = "Working directory";
            this.lblWorkingDirectory.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtWorkingDirectory
            //
            this.txtWorkingDirectory.Anchor = AnchorStyles.None;
            this.txtWorkingDirectory.BackColor = Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
            this.txtWorkingDirectory.Cursor = Cursors.Hand;
            this.txtWorkingDirectory.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.txtWorkingDirectory.Icon = null;
            this.txtWorkingDirectory.Location = new Point(216, 180);
            this.txtWorkingDirectory.MaxCharacters = 32767;
            this.txtWorkingDirectory.Multiline = false;
            this.txtWorkingDirectory.Name = "txtWorkingDirectory";
            this.txtWorkingDirectory.Padding = new Padding(8, 5, 8, 5);
            this.txtWorkingDirectory.PasswordChar = false;
            this.txtWorkingDirectory.PlaceHolderColor = Color.Gray;
            this.txtWorkingDirectory.PlaceHolderText = "Optional working directory";
            this.txtWorkingDirectory.ReadOnly = false;
            this.txtWorkingDirectory.ScrollBars = ScrollBars.None;
            this.txtWorkingDirectory.SelectionStart = 0;
            this.txtWorkingDirectory.Size = new Size(360, 25);
            this.txtWorkingDirectory.TabIndex = 8;
            this.txtWorkingDirectory.TextAlignment = HorizontalAlignment.Left;
            //
            // btnWorkingDirBrowse
            //
            this.btnWorkingDirBrowse.Anchor = AnchorStyles.None;
            this.btnWorkingDirBrowse.BorderRadius = 8;
            this.btnWorkingDirBrowse.Cursor = Cursors.Hand;
            this.btnWorkingDirBrowse.FlatAppearance.BorderSize = 0;
            this.btnWorkingDirBrowse.FlatStyle = FlatStyle.Flat;
            this.btnWorkingDirBrowse.Font = new Font("Tahoma", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnWorkingDirBrowse.ForeColor = Color.White;
            this.btnWorkingDirBrowse.HoverColor = Color.Empty;
            this.btnWorkingDirBrowse.Icon = null;
            this.btnWorkingDirBrowse.Location = new Point(582, 180);
            this.btnWorkingDirBrowse.Name = "btnWorkingDirBrowse";
            this.btnWorkingDirBrowse.Progress = 0;
            this.btnWorkingDirBrowse.ProgressColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(103)))), ((int)(((byte)(205)))));
            this.btnWorkingDirBrowse.Size = new Size(80, 25);
            this.btnWorkingDirBrowse.TabIndex = 9;
            this.btnWorkingDirBrowse.Text = "Browse";
            this.btnWorkingDirBrowse.UseVisualStyleBackColor = true;
            this.btnWorkingDirBrowse.UseWindowsAccentColor = true;
            this.btnWorkingDirBrowse.Click += new EventHandler(this.BtnWorkingDirBrowse_Click);
            //
            // LaunchProgramActionConfigView
            //
            this.AutoScaleMode = AutoScaleMode.Dpi;
            ((System.ComponentModel.ISupportInitialize)(this.picIconPreview)).EndInit();
            this.Controls.Add(this.lblFilePath);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.picIconPreview);
            this.Controls.Add(this.lblExtracted);
            this.Controls.Add(this.lblArguments);
            this.Controls.Add(this.txtArguments);
            this.Controls.Add(this.lblWorkingDirectory);
            this.Controls.Add(this.txtWorkingDirectory);
            this.Controls.Add(this.btnWorkingDirBrowse);
            this.Name = "LaunchProgramActionConfigView";
            this.Load += new EventHandler(this.LaunchProgramActionConfigView_Load);
            this.ResumeLayout(false);
        }

        #endregion

        private Label lblFilePath;
        private RoundedTextBox txtFilePath;
        private ButtonPrimary btnBrowse;
        private PictureBox picIconPreview;
        private Label lblExtracted;
        private Label lblArguments;
        private RoundedTextBox txtArguments;
        private Label lblWorkingDirectory;
        private RoundedTextBox txtWorkingDirectory;
        private ButtonPrimary btnWorkingDirBrowse;
    }
}

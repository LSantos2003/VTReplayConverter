﻿namespace VTReplayConverter
{
    partial class VTRConverterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VTRConverterForm));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ReConvertAll = new System.Windows.Forms.Button();
            this.ProgressText = new System.Windows.Forms.Label();
            this.OpenFolder = new System.Windows.Forms.Button();
            this.excludeEW = new System.Windows.Forms.CheckBox();
            this.ReplayButtonPanel = new System.Windows.Forms.Panel();
            this.TemplateButton = new System.Windows.Forms.Button();
            this.ConvertRemaining = new System.Windows.Forms.Button();
            this.OpenTacviewFolder = new System.Windows.Forms.Button();
            this.OpenReplayFolder = new System.Windows.Forms.Button();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.updateButton = new System.Windows.Forms.Button();
            this.refreshReplaysButton = new System.Windows.Forms.Button();
            this.excludeBullets = new System.Windows.Forms.CheckBox();
            this.versionLabel = new System.Windows.Forms.Label();
            this.ReplayButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.ForeColor = System.Drawing.Color.Gold;
            this.progressBar1.Location = new System.Drawing.Point(375, 304);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(247, 38);
            this.progressBar1.Step = 1;
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 1;
            this.progressBar1.Value = 25;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ReConvertAll
            // 
            this.ReConvertAll.BackColor = System.Drawing.Color.Silver;
            this.ReConvertAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ReConvertAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReConvertAll.Font = new System.Drawing.Font("Unispace", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReConvertAll.Location = new System.Drawing.Point(375, 78);
            this.ReConvertAll.Name = "ReConvertAll";
            this.ReConvertAll.Size = new System.Drawing.Size(178, 60);
            this.ReConvertAll.TabIndex = 2;
            this.ReConvertAll.Tag = "";
            this.ReConvertAll.Text = "RE-CONVERT ALL";
            this.ReConvertAll.UseVisualStyleBackColor = false;
            this.ReConvertAll.Click += new System.EventHandler(this.ReConvertAll_Click);
            // 
            // ProgressText
            // 
            this.ProgressText.AutoSize = true;
            this.ProgressText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProgressText.ForeColor = System.Drawing.Color.Gold;
            this.ProgressText.Location = new System.Drawing.Point(372, 256);
            this.ProgressText.Name = "ProgressText";
            this.ProgressText.Size = new System.Drawing.Size(141, 45);
            this.ProgressText.TabIndex = 4;
            this.ProgressText.Tag = "ProgressText";
            this.ProgressText.Text = "Converting All\r\nProgress: 69%\r\nKeyframes 1123/123010\r\n";
            // 
            // OpenFolder
            // 
            this.OpenFolder.BackColor = System.Drawing.Color.Silver;
            this.OpenFolder.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.OpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenFolder.Font = new System.Drawing.Font("Unispace", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenFolder.Location = new System.Drawing.Point(375, 144);
            this.OpenFolder.Name = "OpenFolder";
            this.OpenFolder.Size = new System.Drawing.Size(178, 60);
            this.OpenFolder.TabIndex = 5;
            this.OpenFolder.Text = "OPEN VTR FILE";
            this.OpenFolder.UseVisualStyleBackColor = false;
            this.OpenFolder.Click += new System.EventHandler(this.OpenFolder_Click);
            // 
            // excludeEW
            // 
            this.excludeEW.AutoSize = true;
            this.excludeEW.Checked = true;
            this.excludeEW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.excludeEW.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.excludeEW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.excludeEW.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.excludeEW.ForeColor = System.Drawing.Color.Gold;
            this.excludeEW.Location = new System.Drawing.Point(375, 210);
            this.excludeEW.Name = "excludeEW";
            this.excludeEW.Size = new System.Drawing.Size(408, 21);
            this.excludeEW.TabIndex = 7;
            this.excludeEW.Text = "Exclude Electronic Warfare (Requires Reconverting Replays)";
            this.excludeEW.UseVisualStyleBackColor = true;
            this.excludeEW.CheckedChanged += new System.EventHandler(this.includeEW_CheckedChanged);
            // 
            // ReplayButtonPanel
            // 
            this.ReplayButtonPanel.AutoScroll = true;
            this.ReplayButtonPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.ReplayButtonPanel.Controls.Add(this.TemplateButton);
            this.ReplayButtonPanel.Location = new System.Drawing.Point(12, 1);
            this.ReplayButtonPanel.Name = "ReplayButtonPanel";
            this.ReplayButtonPanel.Size = new System.Drawing.Size(247, 437);
            this.ReplayButtonPanel.TabIndex = 8;
            // 
            // TemplateButton
            // 
            this.TemplateButton.BackColor = System.Drawing.Color.Crimson;
            this.TemplateButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Teal;
            this.TemplateButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.CadetBlue;
            this.TemplateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TemplateButton.Font = new System.Drawing.Font("Unispace", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TemplateButton.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.TemplateButton.Location = new System.Drawing.Point(3, 3);
            this.TemplateButton.Name = "TemplateButton";
            this.TemplateButton.Size = new System.Drawing.Size(222, 30);
            this.TemplateButton.TabIndex = 0;
            this.TemplateButton.Text = "Template Button";
            this.TemplateButton.UseVisualStyleBackColor = false;
            // 
            // ConvertRemaining
            // 
            this.ConvertRemaining.BackColor = System.Drawing.Color.Silver;
            this.ConvertRemaining.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ConvertRemaining.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConvertRemaining.Font = new System.Drawing.Font("Unispace", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConvertRemaining.Location = new System.Drawing.Point(375, 12);
            this.ConvertRemaining.Name = "ConvertRemaining";
            this.ConvertRemaining.Size = new System.Drawing.Size(178, 60);
            this.ConvertRemaining.TabIndex = 9;
            this.ConvertRemaining.Tag = "";
            this.ConvertRemaining.Text = "CONVERT REPLAYS";
            this.ConvertRemaining.UseVisualStyleBackColor = false;
            this.ConvertRemaining.Click += new System.EventHandler(this.ConvertRemaining_Click);
            // 
            // OpenTacviewFolder
            // 
            this.OpenTacviewFolder.BackColor = System.Drawing.Color.Silver;
            this.OpenTacviewFolder.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.OpenTacviewFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenTacviewFolder.Font = new System.Drawing.Font("Unispace", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenTacviewFolder.Location = new System.Drawing.Point(580, 12);
            this.OpenTacviewFolder.Name = "OpenTacviewFolder";
            this.OpenTacviewFolder.Size = new System.Drawing.Size(178, 44);
            this.OpenTacviewFolder.TabIndex = 10;
            this.OpenTacviewFolder.Text = "OPEN TACVIEW FOLDER\r\n";
            this.OpenTacviewFolder.UseVisualStyleBackColor = false;
            this.OpenTacviewFolder.Click += new System.EventHandler(this.OpenTacviewFolder_Click);
            // 
            // OpenReplayFolder
            // 
            this.OpenReplayFolder.BackColor = System.Drawing.Color.Silver;
            this.OpenReplayFolder.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.OpenReplayFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenReplayFolder.Font = new System.Drawing.Font("Unispace", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenReplayFolder.Location = new System.Drawing.Point(580, 62);
            this.OpenReplayFolder.Name = "OpenReplayFolder";
            this.OpenReplayFolder.Size = new System.Drawing.Size(178, 44);
            this.OpenReplayFolder.TabIndex = 11;
            this.OpenReplayFolder.Text = "OPEN REPLAYS FOLDER";
            this.OpenReplayFolder.UseVisualStyleBackColor = false;
            this.OpenReplayFolder.Click += new System.EventHandler(this.OpenReplayFolder_Click);
            // 
            // WarningLabel
            // 
            this.WarningLabel.AutoSize = true;
            this.WarningLabel.Font = new System.Drawing.Font("Unispace", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningLabel.ForeColor = System.Drawing.Color.DarkRed;
            this.WarningLabel.Location = new System.Drawing.Point(371, 354);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(389, 38);
            this.WarningLabel.TabIndex = 12;
            this.WarningLabel.Text = "CANNOT OPEN REPLAYS OR UPDATE PROGRAM \r\nWHILE CONVERTING";
            this.WarningLabel.Click += new System.EventHandler(this.WarningLabel_Click);
            // 
            // updateButton
            // 
            this.updateButton.BackColor = System.Drawing.Color.Silver;
            this.updateButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.updateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updateButton.Font = new System.Drawing.Font("Unispace", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateButton.Location = new System.Drawing.Point(615, 384);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(183, 32);
            this.updateButton.TabIndex = 14;
            this.updateButton.Text = "UPDATE AVAILABLE!";
            this.updateButton.UseVisualStyleBackColor = false;
            this.updateButton.Visible = false;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // refreshReplaysButton
            // 
            this.refreshReplaysButton.BackColor = System.Drawing.Color.Silver;
            this.refreshReplaysButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.refreshReplaysButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshReplaysButton.Font = new System.Drawing.Font("Unispace", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refreshReplaysButton.Location = new System.Drawing.Point(580, 112);
            this.refreshReplaysButton.Name = "refreshReplaysButton";
            this.refreshReplaysButton.Size = new System.Drawing.Size(178, 44);
            this.refreshReplaysButton.TabIndex = 15;
            this.refreshReplaysButton.Text = "REFRESH REPLAYS";
            this.refreshReplaysButton.UseVisualStyleBackColor = false;
            this.refreshReplaysButton.Visible = false;
            this.refreshReplaysButton.Click += new System.EventHandler(this.refreshReplaysButton_Click);
            // 
            // excludeBullets
            // 
            this.excludeBullets.AutoSize = true;
            this.excludeBullets.Checked = true;
            this.excludeBullets.CheckState = System.Windows.Forms.CheckState.Checked;
            this.excludeBullets.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.excludeBullets.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.excludeBullets.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.excludeBullets.ForeColor = System.Drawing.Color.Gold;
            this.excludeBullets.Location = new System.Drawing.Point(375, 232);
            this.excludeBullets.Name = "excludeBullets";
            this.excludeBullets.Size = new System.Drawing.Size(375, 21);
            this.excludeBullets.TabIndex = 16;
            this.excludeBullets.Text = "Exclude Bullets (Signficantly Reduces Conversion Time)";
            this.excludeBullets.UseVisualStyleBackColor = true;
            this.excludeBullets.CheckedChanged += new System.EventHandler(this.excludeBullets_CheckedChanged);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Font = new System.Drawing.Font("Unispace", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionLabel.ForeColor = System.Drawing.Color.Gainsboro;
            this.versionLabel.Location = new System.Drawing.Point(649, 419);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(149, 19);
            this.versionLabel.TabIndex = 13;
            this.versionLabel.Text = "Version 1.0.00";
            this.versionLabel.Click += new System.EventHandler(this.versionLabel_Click);
            // 
            // VTRConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(33)))), ((int)(((byte)(46)))));
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.excludeBullets);
            this.Controls.Add(this.refreshReplaysButton);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.WarningLabel);
            this.Controls.Add(this.OpenReplayFolder);
            this.Controls.Add(this.OpenTacviewFolder);
            this.Controls.Add(this.ConvertRemaining);
            this.Controls.Add(this.ReplayButtonPanel);
            this.Controls.Add(this.excludeEW);
            this.Controls.Add(this.OpenFolder);
            this.Controls.Add(this.ProgressText);
            this.Controls.Add(this.ReConvertAll);
            this.Controls.Add(this.progressBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VTRConverterForm";
            this.Text = "VTReplayConverter";
            this.Load += new System.EventHandler(this.VTRConverterForm_Load);
            this.ReplayButtonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button ReConvertAll;
        private System.Windows.Forms.Label ProgressText;
        private System.Windows.Forms.Button OpenFolder;
        private System.Windows.Forms.CheckBox excludeEW;
        private System.Windows.Forms.Panel ReplayButtonPanel;
        private System.Windows.Forms.Button TemplateButton;
        private System.Windows.Forms.Button ConvertRemaining;
        private System.Windows.Forms.Button OpenTacviewFolder;
        private System.Windows.Forms.Button OpenReplayFolder;
        private System.Windows.Forms.Label WarningLabel;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button refreshReplaysButton;
        private System.Windows.Forms.CheckBox excludeBullets;
        private System.Windows.Forms.Label versionLabel;
    }
}
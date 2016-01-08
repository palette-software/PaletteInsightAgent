namespace PaletteConfigurator
{
    partial class InsightAgentConfigurator
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            PaletteConfigurator.DbDetails dbDetails3 = new PaletteConfigurator.DbDetails();
            PaletteConfigurator.DbDetails dbDetails4 = new PaletteConfigurator.DbDetails();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.configBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.agentFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.saveConfigButton = new System.Windows.Forms.Button();
            this.logFileMaskText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.threadPollNumeric = new System.Windows.Forms.NumericUpDown();
            this.agentLocationSelector = new PaletteConfigurator.FolderSelector();
            this.logfilesFolderSelector = new PaletteConfigurator.FolderSelector();
            this.tableauRepo = new PaletteConfigurator.DbConfigurator();
            this.resultDbConfiguration = new PaletteConfigurator.DbConfigurator();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.pollIntervalNumeric = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.logPollNumeric = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.configBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.threadPollNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pollIntervalNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logPollNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(6, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Results Database";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(6, 243);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Tableau Repository";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(6, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(179, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Palette Insight Agent Location";
            // 
            // saveConfigButton
            // 
            this.saveConfigButton.Location = new System.Drawing.Point(335, 568);
            this.saveConfigButton.Name = "saveConfigButton";
            this.saveConfigButton.Size = new System.Drawing.Size(163, 23);
            this.saveConfigButton.TabIndex = 7;
            this.saveConfigButton.Text = "Save Configuration";
            this.saveConfigButton.UseVisualStyleBackColor = true;
            this.saveConfigButton.Click += new System.EventHandler(this.saveConfigButton_Click);
            // 
            // logFileMaskText
            // 
            this.logFileMaskText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.logFileMaskText.Location = new System.Drawing.Point(413, 434);
            this.logFileMaskText.Name = "logFileMaskText";
            this.logFileMaskText.Size = new System.Drawing.Size(100, 20);
            this.logFileMaskText.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(6, 406);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Log Directory";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.Location = new System.Drawing.Point(429, 406);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Log File Mask";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(6, 474);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Poll Intervals";
            // 
            // threadPollNumeric
            // 
            this.threadPollNumeric.Location = new System.Drawing.Point(9, 521);
            this.threadPollNumeric.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.threadPollNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.threadPollNumeric.Name = "threadPollNumeric";
            this.threadPollNumeric.Size = new System.Drawing.Size(80, 20);
            this.threadPollNumeric.TabIndex = 15;
            this.threadPollNumeric.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.threadPollNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // agentLocationSelector
            // 
            this.agentLocationSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.agentLocationSelector.Location = new System.Drawing.Point(9, 36);
            this.agentLocationSelector.MaximumSize = new System.Drawing.Size(1800, 30);
            this.agentLocationSelector.MinimumSize = new System.Drawing.Size(200, 30);
            this.agentLocationSelector.Name = "agentLocationSelector";
            this.agentLocationSelector.SelectedFolder = null;
            this.agentLocationSelector.Size = new System.Drawing.Size(495, 30);
            this.agentLocationSelector.TabIndex = 9;
            // 
            // logfilesFolderSelector
            // 
            this.logfilesFolderSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logfilesFolderSelector.Location = new System.Drawing.Point(5, 430);
            this.logfilesFolderSelector.MaximumSize = new System.Drawing.Size(1800, 30);
            this.logfilesFolderSelector.MinimumSize = new System.Drawing.Size(200, 30);
            this.logfilesFolderSelector.Name = "logfilesFolderSelector";
            this.logfilesFolderSelector.SelectedFolder = null;
            this.logfilesFolderSelector.Size = new System.Drawing.Size(392, 30);
            this.logfilesFolderSelector.TabIndex = 8;
            // 
            // tableauRepo
            // 
            this.tableauRepo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dbDetails3.Database = "TabMon";
            dbDetails3.DbType = "Postgres";
            dbDetails3.Host = "localhost";
            dbDetails3.Password = "abc12def";
            dbDetails3.Port = 5432;
            dbDetails3.Username = "postgres";
            this.tableauRepo.Database = dbDetails3;
            this.tableauRepo.Location = new System.Drawing.Point(1, 261);
            this.tableauRepo.MaximumSize = new System.Drawing.Size(1800, 130);
            this.tableauRepo.MinimumSize = new System.Drawing.Size(530, 130);
            this.tableauRepo.Name = "tableauRepo";
            this.tableauRepo.Size = new System.Drawing.Size(530, 130);
            this.tableauRepo.TabIndex = 2;
            // 
            // resultDbConfiguration
            // 
            this.resultDbConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dbDetails4.Database = "TabMon";
            dbDetails4.DbType = "Postgres";
            dbDetails4.Host = "localhost";
            dbDetails4.Password = "abc12def";
            dbDetails4.Port = 5432;
            dbDetails4.Username = "postgres";
            this.resultDbConfiguration.Database = dbDetails4;
            this.resultDbConfiguration.Location = new System.Drawing.Point(1, 98);
            this.resultDbConfiguration.MaximumSize = new System.Drawing.Size(1800, 130);
            this.resultDbConfiguration.MinimumSize = new System.Drawing.Size(530, 130);
            this.resultDbConfiguration.Name = "resultDbConfiguration";
            this.resultDbConfiguration.Size = new System.Drawing.Size(530, 130);
            this.resultDbConfiguration.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 500);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(119, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Thread Info Poll interval";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(91, 523);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "sec";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(240, 523);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 23;
            this.label10.Text = "sec";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(157, 500);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "JMX Poll interval";
            // 
            // pollIntervalNumeric
            // 
            this.pollIntervalNumeric.Location = new System.Drawing.Point(158, 521);
            this.pollIntervalNumeric.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.pollIntervalNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.pollIntervalNumeric.Name = "pollIntervalNumeric";
            this.pollIntervalNumeric.Size = new System.Drawing.Size(80, 20);
            this.pollIntervalNumeric.TabIndex = 21;
            this.pollIntervalNumeric.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pollIntervalNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(378, 523);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 13);
            this.label12.TabIndex = 26;
            this.label12.Text = "sec";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(295, 500);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(82, 13);
            this.label13.TabIndex = 25;
            this.label13.Text = "Log Poll interval";
            // 
            // logPollNumeric
            // 
            this.logPollNumeric.Location = new System.Drawing.Point(296, 521);
            this.logPollNumeric.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.logPollNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.logPollNumeric.Name = "logPollNumeric";
            this.logPollNumeric.Size = new System.Drawing.Size(80, 20);
            this.logPollNumeric.TabIndex = 24;
            this.logPollNumeric.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.logPollNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // InsightAgentConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.logPollNumeric);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.pollIntervalNumeric);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.threadPollNumeric);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.logFileMaskText);
            this.Controls.Add(this.agentLocationSelector);
            this.Controls.Add(this.logfilesFolderSelector);
            this.Controls.Add(this.saveConfigButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tableauRepo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.resultDbConfiguration);
            this.MaximumSize = new System.Drawing.Size(1800, 620);
            this.MinimumSize = new System.Drawing.Size(534, 620);
            this.Name = "InsightAgentConfigurator";
            this.Size = new System.Drawing.Size(534, 620);
            ((System.ComponentModel.ISupportInitialize)(this.configBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.threadPollNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pollIntervalNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logPollNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DbConfigurator resultDbConfiguration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private DbConfigurator tableauRepo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.BindingSource configBindingSource;
        private System.Windows.Forms.FolderBrowserDialog agentFolderBrowserDialog;
        private System.Windows.Forms.Button saveConfigButton;
        private FolderSelector logfilesFolderSelector;
        private FolderSelector agentLocationSelector;
        private System.Windows.Forms.TextBox logFileMaskText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown threadPollNumeric;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown pollIntervalNumeric;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown logPollNumeric;
    }
}

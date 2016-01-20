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
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.pollIntervalNumeric = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.logPollNumeric = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.addProcessButton = new System.Windows.Forms.Button();
            this.deleteProcessBtn = new System.Windows.Forms.Button();
            this.processListView = new System.Windows.Forms.ListView();
            this.clusterTreeView = new System.Windows.Forms.TreeView();
            this.deleteClusterButton = new System.Windows.Forms.Button();
            this.addClusterButton = new System.Windows.Forms.Button();
            this.addClusterNodeButton = new System.Windows.Forms.Button();
            this.loadConfigurationButton = new System.Windows.Forms.Button();
            this.tabPages = new System.Windows.Forms.TabControl();
            this.generalPage = new System.Windows.Forms.TabPage();
            this.label14 = new System.Windows.Forms.Label();
            this.databasesPage = new System.Windows.Forms.TabPage();
            this.processesPage = new System.Windows.Forms.TabPage();
            this.label15 = new System.Windows.Forms.Label();
            this.logfilesFolderSelector = new PaletteConfigurator.FolderSelector();
            this.resultDbConfiguration = new PaletteConfigurator.DbConfigurator();
            this.tableauRepo = new PaletteConfigurator.DbConfigurator();
            this.agentLocationSelector = new PaletteConfigurator.FolderSelector();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.configBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.threadPollNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pollIntervalNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logPollNumeric)).BeginInit();
            this.tabPages.SuspendLayout();
            this.generalPage.SuspendLayout();
            this.databasesPage.SuspendLayout();
            this.processesPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(6, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Results Database";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(6, 188);
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
            this.saveConfigButton.Location = new System.Drawing.Point(391, 45);
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
            this.logFileMaskText.Location = new System.Drawing.Point(176, 213);
            this.logFileMaskText.Name = "logFileMaskText";
            this.logFileMaskText.Size = new System.Drawing.Size(100, 20);
            this.logFileMaskText.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(28, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Log Polling";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.Location = new System.Drawing.Point(92, 216);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(73, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Log File Mask";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(28, 14);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Poll Intervals";
            // 
            // threadPollNumeric
            // 
            this.threadPollNumeric.Location = new System.Drawing.Point(155, 41);
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
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 43);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(119, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Thread Info Poll interval";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(237, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(24, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "sec";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(237, 71);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(24, 13);
            this.label10.TabIndex = 23;
            this.label10.Text = "sec";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(64, 71);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "JMX Poll interval";
            // 
            // pollIntervalNumeric
            // 
            this.pollIntervalNumeric.Location = new System.Drawing.Point(155, 69);
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
            this.label12.Location = new System.Drawing.Point(237, 98);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 13);
            this.label12.TabIndex = 26;
            this.label12.Text = "sec";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(67, 98);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(82, 13);
            this.label13.TabIndex = 25;
            this.label13.Text = "Log Poll interval";
            // 
            // logPollNumeric
            // 
            this.logPollNumeric.Location = new System.Drawing.Point(155, 96);
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
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label8.Location = new System.Drawing.Point(8, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Processes";
            // 
            // addProcessButton
            // 
            this.addProcessButton.Location = new System.Drawing.Point(184, 50);
            this.addProcessButton.Name = "addProcessButton";
            this.addProcessButton.Size = new System.Drawing.Size(54, 23);
            this.addProcessButton.TabIndex = 29;
            this.addProcessButton.Text = "Add";
            this.addProcessButton.UseVisualStyleBackColor = true;
            this.addProcessButton.Click += new System.EventHandler(this.addProcessButton_Click);
            // 
            // deleteProcessBtn
            // 
            this.deleteProcessBtn.Location = new System.Drawing.Point(186, 80);
            this.deleteProcessBtn.Name = "deleteProcessBtn";
            this.deleteProcessBtn.Size = new System.Drawing.Size(52, 23);
            this.deleteProcessBtn.TabIndex = 30;
            this.deleteProcessBtn.Text = "Del";
            this.deleteProcessBtn.UseVisualStyleBackColor = true;
            this.deleteProcessBtn.Click += new System.EventHandler(this.deleteProcessBtn_Click);
            // 
            // processListView
            // 
            this.processListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.processListView.LabelEdit = true;
            this.processListView.LabelWrap = false;
            this.processListView.Location = new System.Drawing.Point(11, 50);
            this.processListView.Name = "processListView";
            this.processListView.Size = new System.Drawing.Size(167, 325);
            this.processListView.TabIndex = 31;
            this.processListView.UseCompatibleStateImageBehavior = false;
            this.processListView.View = System.Windows.Forms.View.List;
            // 
            // clusterTreeView
            // 
            this.clusterTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.clusterTreeView.LabelEdit = true;
            this.clusterTreeView.Location = new System.Drawing.Point(270, 50);
            this.clusterTreeView.Name = "clusterTreeView";
            this.clusterTreeView.Size = new System.Drawing.Size(183, 325);
            this.clusterTreeView.TabIndex = 32;
            // 
            // deleteClusterButton
            // 
            this.deleteClusterButton.Location = new System.Drawing.Point(461, 122);
            this.deleteClusterButton.Name = "deleteClusterButton";
            this.deleteClusterButton.Size = new System.Drawing.Size(52, 23);
            this.deleteClusterButton.TabIndex = 34;
            this.deleteClusterButton.Text = "Del";
            this.deleteClusterButton.UseVisualStyleBackColor = true;
            this.deleteClusterButton.Click += new System.EventHandler(this.deleteClusterButton_Click);
            // 
            // addClusterButton
            // 
            this.addClusterButton.Location = new System.Drawing.Point(459, 50);
            this.addClusterButton.Name = "addClusterButton";
            this.addClusterButton.Size = new System.Drawing.Size(54, 23);
            this.addClusterButton.TabIndex = 33;
            this.addClusterButton.Text = "+Cluster";
            this.addClusterButton.UseVisualStyleBackColor = true;
            this.addClusterButton.Click += new System.EventHandler(this.addClusterButton_Click);
            // 
            // addClusterNodeButton
            // 
            this.addClusterNodeButton.Location = new System.Drawing.Point(459, 79);
            this.addClusterNodeButton.Name = "addClusterNodeButton";
            this.addClusterNodeButton.Size = new System.Drawing.Size(54, 23);
            this.addClusterNodeButton.TabIndex = 35;
            this.addClusterNodeButton.Text = "+Node";
            this.addClusterNodeButton.UseVisualStyleBackColor = true;
            this.addClusterNodeButton.Click += new System.EventHandler(this.addClusterNodeButton_Click);
            // 
            // loadConfigurationButton
            // 
            this.loadConfigurationButton.Location = new System.Drawing.Point(3, 45);
            this.loadConfigurationButton.Name = "loadConfigurationButton";
            this.loadConfigurationButton.Size = new System.Drawing.Size(163, 23);
            this.loadConfigurationButton.TabIndex = 36;
            this.loadConfigurationButton.Text = "Load Configuration";
            this.loadConfigurationButton.UseVisualStyleBackColor = true;
            this.loadConfigurationButton.Click += new System.EventHandler(this.loadConfigurationButton_Click);
            // 
            // tabPages
            // 
            this.tabPages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabPages.Controls.Add(this.generalPage);
            this.tabPages.Controls.Add(this.databasesPage);
            this.tabPages.Controls.Add(this.processesPage);
            this.tabPages.Location = new System.Drawing.Point(3, 94);
            this.tabPages.Name = "tabPages";
            this.tabPages.SelectedIndex = 0;
            this.tabPages.Size = new System.Drawing.Size(555, 414);
            this.tabPages.TabIndex = 37;
            // 
            // generalPage
            // 
            this.generalPage.Controls.Add(this.label14);
            this.generalPage.Controls.Add(this.label4);
            this.generalPage.Controls.Add(this.logfilesFolderSelector);
            this.generalPage.Controls.Add(this.label5);
            this.generalPage.Controls.Add(this.logFileMaskText);
            this.generalPage.Controls.Add(this.label6);
            this.generalPage.Controls.Add(this.threadPollNumeric);
            this.generalPage.Controls.Add(this.label7);
            this.generalPage.Controls.Add(this.label9);
            this.generalPage.Controls.Add(this.pollIntervalNumeric);
            this.generalPage.Controls.Add(this.label11);
            this.generalPage.Controls.Add(this.label12);
            this.generalPage.Controls.Add(this.label10);
            this.generalPage.Controls.Add(this.label13);
            this.generalPage.Controls.Add(this.logPollNumeric);
            this.generalPage.Location = new System.Drawing.Point(4, 22);
            this.generalPage.Name = "generalPage";
            this.generalPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalPage.Size = new System.Drawing.Size(547, 388);
            this.generalPage.TabIndex = 0;
            this.generalPage.Text = "General";
            this.generalPage.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label14.Location = new System.Drawing.Point(97, 185);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(70, 13);
            this.label14.TabIndex = 27;
            this.label14.Text = "Log Directory";
            // 
            // databasesPage
            // 
            this.databasesPage.Controls.Add(this.label17);
            this.databasesPage.Controls.Add(this.label16);
            this.databasesPage.Controls.Add(this.resultDbConfiguration);
            this.databasesPage.Controls.Add(this.label1);
            this.databasesPage.Controls.Add(this.label2);
            this.databasesPage.Controls.Add(this.tableauRepo);
            this.databasesPage.Location = new System.Drawing.Point(4, 22);
            this.databasesPage.Name = "databasesPage";
            this.databasesPage.Padding = new System.Windows.Forms.Padding(3);
            this.databasesPage.Size = new System.Drawing.Size(547, 388);
            this.databasesPage.TabIndex = 1;
            this.databasesPage.Text = "Databases";
            this.databasesPage.UseVisualStyleBackColor = true;
            // 
            // processesPage
            // 
            this.processesPage.Controls.Add(this.label15);
            this.processesPage.Controls.Add(this.processListView);
            this.processesPage.Controls.Add(this.addProcessButton);
            this.processesPage.Controls.Add(this.label8);
            this.processesPage.Controls.Add(this.addClusterNodeButton);
            this.processesPage.Controls.Add(this.deleteProcessBtn);
            this.processesPage.Controls.Add(this.deleteClusterButton);
            this.processesPage.Controls.Add(this.clusterTreeView);
            this.processesPage.Controls.Add(this.addClusterButton);
            this.processesPage.Location = new System.Drawing.Point(4, 22);
            this.processesPage.Name = "processesPage";
            this.processesPage.Size = new System.Drawing.Size(547, 388);
            this.processesPage.TabIndex = 2;
            this.processesPage.Text = "Processes & Clusters";
            this.processesPage.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label15.Location = new System.Drawing.Point(267, 11);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(52, 13);
            this.label15.TabIndex = 36;
            this.label15.Text = "Clusters";
            // 
            // logfilesFolderSelector
            // 
            this.logfilesFolderSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logfilesFolderSelector.Location = new System.Drawing.Point(151, 177);
            this.logfilesFolderSelector.MaximumSize = new System.Drawing.Size(1800, 30);
            this.logfilesFolderSelector.MinimumSize = new System.Drawing.Size(200, 30);
            this.logfilesFolderSelector.Name = "logfilesFolderSelector";
            this.logfilesFolderSelector.SelectedFolder = null;
            this.logfilesFolderSelector.Size = new System.Drawing.Size(309, 30);
            this.logfilesFolderSelector.TabIndex = 8;
            // 
            // resultDbConfiguration
            // 
            this.resultDbConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultDbConfiguration.Location = new System.Drawing.Point(6, 48);
            this.resultDbConfiguration.MaximumSize = new System.Drawing.Size(1800, 130);
            this.resultDbConfiguration.MinimumSize = new System.Drawing.Size(530, 110);
            this.resultDbConfiguration.Name = "resultDbConfiguration";
            this.resultDbConfiguration.Size = new System.Drawing.Size(535, 110);
            this.resultDbConfiguration.TabIndex = 0;
            // 
            // tableauRepo
            // 
            this.tableauRepo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableauRepo.Location = new System.Drawing.Point(6, 233);
            this.tableauRepo.MaximumSize = new System.Drawing.Size(1800, 130);
            this.tableauRepo.MinimumSize = new System.Drawing.Size(530, 110);
            this.tableauRepo.Name = "tableauRepo";
            this.tableauRepo.Size = new System.Drawing.Size(535, 110);
            this.tableauRepo.TabIndex = 2;
            // 
            // agentLocationSelector
            // 
            this.agentLocationSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.agentLocationSelector.Location = new System.Drawing.Point(209, 3);
            this.agentLocationSelector.MaximumSize = new System.Drawing.Size(1800, 30);
            this.agentLocationSelector.MinimumSize = new System.Drawing.Size(200, 30);
            this.agentLocationSelector.Name = "agentLocationSelector";
            this.agentLocationSelector.SelectedFolder = null;
            this.agentLocationSelector.Size = new System.Drawing.Size(349, 30);
            this.agentLocationSelector.TabIndex = 9;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label16.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label16.Location = new System.Drawing.Point(5, 28);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(227, 13);
            this.label16.TabIndex = 4;
            this.label16.Text = "The database where the Agent stores the data";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label17.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label17.Location = new System.Drawing.Point(7, 206);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(494, 13);
            this.label17.TabIndex = 5;
            this.label17.Text = "The \'readonly\' account to the postgres server housing the repository of the Table" +
    "u Servers in the cluster";
            // 
            // InsightAgentConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabPages);
            this.Controls.Add(this.loadConfigurationButton);
            this.Controls.Add(this.agentLocationSelector);
            this.Controls.Add(this.saveConfigButton);
            this.Controls.Add(this.label3);
            this.MaximumSize = new System.Drawing.Size(1800, 620);
            this.MinimumSize = new System.Drawing.Size(534, 520);
            this.Name = "InsightAgentConfigurator";
            this.Size = new System.Drawing.Size(561, 520);
            ((System.ComponentModel.ISupportInitialize)(this.configBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.threadPollNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pollIntervalNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logPollNumeric)).EndInit();
            this.tabPages.ResumeLayout(false);
            this.generalPage.ResumeLayout(false);
            this.generalPage.PerformLayout();
            this.databasesPage.ResumeLayout(false);
            this.databasesPage.PerformLayout();
            this.processesPage.ResumeLayout(false);
            this.processesPage.PerformLayout();
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
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button addProcessButton;
        private System.Windows.Forms.Button deleteProcessBtn;
        private System.Windows.Forms.ListView processListView;
        private System.Windows.Forms.TreeView clusterTreeView;
        private System.Windows.Forms.Button deleteClusterButton;
        private System.Windows.Forms.Button addClusterButton;
        private System.Windows.Forms.Button addClusterNodeButton;
        private System.Windows.Forms.Button loadConfigurationButton;
        private System.Windows.Forms.TabControl tabPages;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TabPage processesPage;
        private System.Windows.Forms.TabPage databasesPage;
        private System.Windows.Forms.TabPage generalPage;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
    }
}

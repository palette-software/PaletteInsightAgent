namespace PaletteConfigurator
{
    partial class DbConfigurator
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
            this.databaseText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.passwordText = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.usernameText = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.portSpinner = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.databaseTypeCombobox = new System.Windows.Forms.ComboBox();
            this.hostNameText = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.testConnectionButton = new System.Windows.Forms.Button();
            this.dbDetailsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.portSpinner)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDetailsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // databaseText
            // 
            this.databaseText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.databaseText.Location = new System.Drawing.Point(342, 5);
            this.databaseText.Name = "databaseText";
            this.databaseText.Size = new System.Drawing.Size(175, 20);
            this.databaseText.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(238, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Database / service";
            // 
            // passwordText
            // 
            this.passwordText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.passwordText.Location = new System.Drawing.Point(342, 61);
            this.passwordText.Name = "passwordText";
            this.passwordText.Size = new System.Drawing.Size(175, 20);
            this.passwordText.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(285, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Password";
            // 
            // usernameText
            // 
            this.usernameText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.usernameText.Location = new System.Drawing.Point(71, 57);
            this.usernameText.Name = "usernameText";
            this.usernameText.Size = new System.Drawing.Size(198, 20);
            this.usernameText.TabIndex = 19;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 60);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Username";
            // 
            // portSpinner
            // 
            this.portSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.portSpinner.Location = new System.Drawing.Point(421, 33);
            this.portSpinner.Maximum = new decimal(new int[] {
            65555,
            0,
            0,
            0});
            this.portSpinner.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.portSpinner.Name = "portSpinner";
            this.portSpinner.Size = new System.Drawing.Size(96, 20);
            this.portSpinner.TabIndex = 17;
            this.portSpinner.Value = new decimal(new int[] {
            5432,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(389, 35);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(26, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Port";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 7);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Db Type";
            // 
            // databaseTypeCombobox
            // 
            this.databaseTypeCombobox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.databaseTypeCombobox.FormattingEnabled = true;
            this.databaseTypeCombobox.Items.AddRange(new object[] {
            "Postgres",
            "Oracle",
            "MSSQL"});
            this.databaseTypeCombobox.Location = new System.Drawing.Point(71, 4);
            this.databaseTypeCombobox.Name = "databaseTypeCombobox";
            this.databaseTypeCombobox.Size = new System.Drawing.Size(127, 21);
            this.databaseTypeCombobox.TabIndex = 14;
            // 
            // hostNameText
            // 
            this.hostNameText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hostNameText.Location = new System.Drawing.Point(71, 31);
            this.hostNameText.Name = "hostNameText";
            this.hostNameText.Size = new System.Drawing.Size(312, 20);
            this.hostNameText.TabIndex = 13;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 34);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(29, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "Host";
            // 
            // testConnectionButton
            // 
            this.testConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.testConnectionButton.Location = new System.Drawing.Point(380, 87);
            this.testConnectionButton.Name = "testConnectionButton";
            this.testConnectionButton.Size = new System.Drawing.Size(137, 23);
            this.testConnectionButton.TabIndex = 24;
            this.testConnectionButton.Text = "Test Connection";
            this.testConnectionButton.UseVisualStyleBackColor = true;
            this.testConnectionButton.Click += new System.EventHandler(this.testConnectionButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(7, 86);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(506, 10);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 25;
            this.progressBar1.Visible = false;
            // 
            // DbConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.testConnectionButton);
            this.Controls.Add(this.databaseText);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.passwordText);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.usernameText);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.portSpinner);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.databaseTypeCombobox);
            this.Controls.Add(this.hostNameText);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.progressBar1);
            this.MaximumSize = new System.Drawing.Size(1800, 110);
            this.MinimumSize = new System.Drawing.Size(500, 110);
            this.Name = "DbConfigurator";
            this.Size = new System.Drawing.Size(530, 110);
            ((System.ComponentModel.ISupportInitialize)(this.portSpinner)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDetailsBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox databaseText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox passwordText;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox usernameText;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown portSpinner;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox databaseTypeCombobox;
        private System.Windows.Forms.TextBox hostNameText;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button testConnectionButton;
        private System.Windows.Forms.BindingSource dbDetailsBindingSource;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

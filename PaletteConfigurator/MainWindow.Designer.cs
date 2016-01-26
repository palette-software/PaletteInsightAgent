namespace PaletteConfigurator
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.insightAgentConfigurator1 = new PaletteConfigurator.InsightAgentConfigurator();
            this.SuspendLayout();
            // 
            // insightAgentConfigurator1
            // 
            this.insightAgentConfigurator1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.insightAgentConfigurator1.Location = new System.Drawing.Point(15, 22);
            this.insightAgentConfigurator1.Margin = new System.Windows.Forms.Padding(11);
            this.insightAgentConfigurator1.MaximumSize = new System.Drawing.Size(3300, 1145);
            this.insightAgentConfigurator1.MinimumSize = new System.Drawing.Size(979, 960);
            this.insightAgentConfigurator1.Name = "insightAgentConfigurator1";
            this.insightAgentConfigurator1.Size = new System.Drawing.Size(1043, 960);
            this.insightAgentConfigurator1.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1080, 967);
            this.Controls.Add(this.insightAgentConfigurator1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MinimumSize = new System.Drawing.Size(1089, 983);
            this.Name = "MainWindow";
            this.Text = "Palette Configurator";
            this.ResumeLayout(false);

        }

        #endregion
        private InsightAgentConfigurator insightAgentConfigurator1;
    }
}


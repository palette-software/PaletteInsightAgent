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
            this.insightAgentConfigurator1 = new PaletteConfigurator.InsightAgentConfigurator();
            this.SuspendLayout();
            // 
            // insightAgentConfigurator1
            // 
            this.insightAgentConfigurator1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.insightAgentConfigurator1.Location = new System.Drawing.Point(8, 12);
            this.insightAgentConfigurator1.MaximumSize = new System.Drawing.Size(1800, 620);
            this.insightAgentConfigurator1.MinimumSize = new System.Drawing.Size(534, 520);
            this.insightAgentConfigurator1.Name = "insightAgentConfigurator1";
            this.insightAgentConfigurator1.Size = new System.Drawing.Size(569, 520);
            this.insightAgentConfigurator1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 524);
            this.Controls.Add(this.insightAgentConfigurator1);
            this.MinimumSize = new System.Drawing.Size(605, 562);
            this.Name = "Form1";
            this.Text = "Palette Configurator";
            this.ResumeLayout(false);

        }

        #endregion
        private InsightAgentConfigurator insightAgentConfigurator1;
    }
}


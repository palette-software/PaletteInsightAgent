namespace LicenseGenerator
{
    partial class LicenseGeneratorTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenseGeneratorTool));
            this.paletteHeaderBar1 = new PaletteGUI.PaletteHeaderBar();
            this.licenseGeneratorControl1 = new LicenseGenerator.LicenseGeneratorControl();
            this.SuspendLayout();
            // 
            // paletteHeaderBar1
            // 
            this.paletteHeaderBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.paletteHeaderBar1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("paletteHeaderBar1.BackgroundImage")));
            this.paletteHeaderBar1.HeaderLabel = "Palette Insight License Generator";
            this.paletteHeaderBar1.Location = new System.Drawing.Point(0, 0);
            this.paletteHeaderBar1.MaximumSize = new System.Drawing.Size(1800, 100);
            this.paletteHeaderBar1.MinimumSize = new System.Drawing.Size(500, 100);
            this.paletteHeaderBar1.Name = "paletteHeaderBar1";
            this.paletteHeaderBar1.Size = new System.Drawing.Size(687, 100);
            this.paletteHeaderBar1.TabIndex = 1;
            // 
            // licenseGeneratorControl1
            // 
            this.licenseGeneratorControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.licenseGeneratorControl1.Location = new System.Drawing.Point(0, 100);
            this.licenseGeneratorControl1.Name = "licenseGeneratorControl1";
            this.licenseGeneratorControl1.Size = new System.Drawing.Size(688, 559);
            this.licenseGeneratorControl1.TabIndex = 0;
            // 
            // LicenseGeneratorTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 653);
            this.Controls.Add(this.paletteHeaderBar1);
            this.Controls.Add(this.licenseGeneratorControl1);
            this.Name = "LicenseGeneratorTool";
            this.Text = "LicenseGeneratorTool";
            this.ResumeLayout(false);

        }

        #endregion

        private LicenseGeneratorControl licenseGeneratorControl1;
        private PaletteGUI.PaletteHeaderBar paletteHeaderBar1;
    }
}
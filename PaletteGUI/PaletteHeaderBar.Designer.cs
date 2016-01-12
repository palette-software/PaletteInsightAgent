namespace PaletteGUI
{
    partial class PaletteHeaderBar
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
            this.paletteSiteLink = new System.Windows.Forms.Label();
            this.supportCenterLink = new System.Windows.Forms.Label();
            this.headerSmoothLabel = new PaletteGUI.SmoothLabel();
            this.SuspendLayout();
            // 
            // paletteSiteLink
            // 
            this.paletteSiteLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.paletteSiteLink.AutoSize = true;
            this.paletteSiteLink.BackColor = System.Drawing.Color.Transparent;
            this.paletteSiteLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.paletteSiteLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.paletteSiteLink.ForeColor = System.Drawing.SystemColors.GrayText;
            this.paletteSiteLink.Location = new System.Drawing.Point(231, 40);
            this.paletteSiteLink.Name = "paletteSiteLink";
            this.paletteSiteLink.Size = new System.Drawing.Size(127, 13);
            this.paletteSiteLink.TabIndex = 0;
            this.paletteSiteLink.Text = "Palette Software Website";
            this.paletteSiteLink.Click += new System.EventHandler(this.paletteSiteLink_Click);
            // 
            // supportCenterLink
            // 
            this.supportCenterLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.supportCenterLink.AutoSize = true;
            this.supportCenterLink.BackColor = System.Drawing.Color.Transparent;
            this.supportCenterLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.supportCenterLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.supportCenterLink.ForeColor = System.Drawing.SystemColors.GrayText;
            this.supportCenterLink.Location = new System.Drawing.Point(364, 40);
            this.supportCenterLink.Name = "supportCenterLink";
            this.supportCenterLink.Size = new System.Drawing.Size(114, 13);
            this.supportCenterLink.TabIndex = 1;
            this.supportCenterLink.Text = "Palette Support Center";
            this.supportCenterLink.Click += new System.EventHandler(this.supportCenterLink_Click);
            // 
            // headerSmoothLabel
            // 
            this.headerSmoothLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.headerSmoothLabel.AutoSize = true;
            this.headerSmoothLabel.BackColor = System.Drawing.Color.Transparent;
            this.headerSmoothLabel.Font = new System.Drawing.Font("Calibri Light", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.headerSmoothLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.headerSmoothLabel.Kerning = 0.55F;
            this.headerSmoothLabel.Location = new System.Drawing.Point(125, 11);
            this.headerSmoothLabel.Name = "headerSmoothLabel";
            this.headerSmoothLabel.Size = new System.Drawing.Size(358, 29);
            this.headerSmoothLabel.TabIndex = 2;
            this.headerSmoothLabel.Text = "Palette Insight License Generator";
            // 
            // PaletteHeaderBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::PaletteGUI.Properties.Resources.palette_header_bar_bg_1800;
            this.Controls.Add(this.headerSmoothLabel);
            this.Controls.Add(this.supportCenterLink);
            this.Controls.Add(this.paletteSiteLink);
            this.MaximumSize = new System.Drawing.Size(1800, 100);
            this.MinimumSize = new System.Drawing.Size(500, 100);
            this.Name = "PaletteHeaderBar";
            this.Size = new System.Drawing.Size(500, 100);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label paletteSiteLink;
        private System.Windows.Forms.Label supportCenterLink;
        private SmoothLabel headerSmoothLabel;
    }
}

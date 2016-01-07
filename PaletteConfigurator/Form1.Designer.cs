namespace PaletteConfigurator
{
    partial class Form1
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
            this.dbConfigurator1 = new PaletteConfigurator.DbConfigurator();
            this.SuspendLayout();
            // 
            // dbConfigurator1
            // 
            this.dbConfigurator1.Location = new System.Drawing.Point(39, 30);
            this.dbConfigurator1.Name = "dbConfigurator1";
            this.dbConfigurator1.Size = new System.Drawing.Size(740, 768);
            this.dbConfigurator1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 714);
            this.Controls.Add(this.dbConfigurator1);
            this.Name = "Form1";
            this.Text = "Palette Configurator";
            this.ResumeLayout(false);

        }

        #endregion

        private DbConfigurator dbConfigurator1;
    }
}


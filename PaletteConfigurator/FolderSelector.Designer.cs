namespace PaletteConfigurator
{
    partial class FolderSelector
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
            this.insightFolderButton = new System.Windows.Forms.Button();
            this.insightFolderText = new System.Windows.Forms.TextBox();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // insightFolderButton
            // 
            this.insightFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.insightFolderButton.Location = new System.Drawing.Point(409, 3);
            this.insightFolderButton.Name = "insightFolderButton";
            this.insightFolderButton.Size = new System.Drawing.Size(31, 23);
            this.insightFolderButton.TabIndex = 7;
            this.insightFolderButton.Text = "...";
            this.insightFolderButton.UseVisualStyleBackColor = true;
            this.insightFolderButton.Click += new System.EventHandler(this.insightFolderButton_Click);
            // 
            // insightFolderText
            // 
            this.insightFolderText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.insightFolderText.Location = new System.Drawing.Point(3, 5);
            this.insightFolderText.Name = "insightFolderText";
            this.insightFolderText.Size = new System.Drawing.Size(400, 20);
            this.insightFolderText.TabIndex = 6;
            // 
            // FolderSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.insightFolderButton);
            this.Controls.Add(this.insightFolderText);
            this.MaximumSize = new System.Drawing.Size(1800, 30);
            this.MinimumSize = new System.Drawing.Size(200, 30);
            this.Name = "FolderSelector";
            this.Size = new System.Drawing.Size(441, 30);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button insightFolderButton;
        private System.Windows.Forms.TextBox insightFolderText;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

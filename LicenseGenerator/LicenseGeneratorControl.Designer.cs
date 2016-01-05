namespace LicenseGenerator
{
    partial class LicenseGeneratorControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenseGeneratorControl));
            this.licenseTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.validUntilPicker = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.licenseId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.licenseName = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.keyGeneratorControl = new LicenseGenerator.KeyGeneratorControl();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.saveLicenseButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // licenseTextBox
            // 
            this.licenseTextBox.Location = new System.Drawing.Point(37, 219);
            this.licenseTextBox.Multiline = true;
            this.licenseTextBox.Name = "licenseTextBox";
            this.licenseTextBox.ReadOnly = true;
            this.licenseTextBox.Size = new System.Drawing.Size(607, 170);
            this.licenseTextBox.TabIndex = 15;
            this.licenseTextBox.WordWrap = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Valid Until (UTC)";
            // 
            // validUntilPicker
            // 
            this.validUntilPicker.Location = new System.Drawing.Point(146, 119);
            this.validUntilPicker.Name = "validUntilPicker";
            this.validUntilPicker.Size = new System.Drawing.Size(200, 20);
            this.validUntilPicker.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "License ID";
            // 
            // licenseId
            // 
            this.licenseId.Location = new System.Drawing.Point(146, 76);
            this.licenseId.Name = "licenseId";
            this.licenseId.Size = new System.Drawing.Size(292, 20);
            this.licenseId.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Name for license";
            // 
            // licenseName
            // 
            this.licenseName.Location = new System.Drawing.Point(146, 36);
            this.licenseName.Name = "licenseName";
            this.licenseName.Size = new System.Drawing.Size(292, 20);
            this.licenseName.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(37, 161);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(197, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Generate License";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // keyGeneratorControl
            // 
            this.keyGeneratorControl.KeyPair = ((Licensing.LicenseKeyPair)(resources.GetObject("keyGeneratorControl.KeyPair")));
            this.keyGeneratorControl.Location = new System.Drawing.Point(37, 418);
            this.keyGeneratorControl.Name = "keyGeneratorControl";
            this.keyGeneratorControl.Size = new System.Drawing.Size(571, 187);
            this.keyGeneratorControl.TabIndex = 16;
            // 
            // saveLicenseButton
            // 
            this.saveLicenseButton.Location = new System.Drawing.Point(447, 161);
            this.saveLicenseButton.Name = "saveLicenseButton";
            this.saveLicenseButton.Size = new System.Drawing.Size(197, 23);
            this.saveLicenseButton.TabIndex = 17;
            this.saveLicenseButton.Text = "Save License";
            this.saveLicenseButton.UseVisualStyleBackColor = true;
            this.saveLicenseButton.Click += new System.EventHandler(this.saveLicenseButton_Click);
            // 
            // LicenseGeneratorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.saveLicenseButton);
            this.Controls.Add(this.keyGeneratorControl);
            this.Controls.Add(this.licenseTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.validUntilPicker);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.licenseId);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.licenseName);
            this.Controls.Add(this.button1);
            this.Name = "LicenseGeneratorControl";
            this.Size = new System.Drawing.Size(797, 608);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox licenseTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker validUntilPicker;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox licenseId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox licenseName;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.BindingSource bindingSource1;
        private KeyGeneratorControl keyGeneratorControl;
        private System.Windows.Forms.Button saveLicenseButton;
    }
}

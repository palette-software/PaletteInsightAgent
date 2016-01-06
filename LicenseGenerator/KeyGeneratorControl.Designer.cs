namespace LicenseGenerator
{
    partial class KeyGeneratorControl
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
            this.publicKey = new System.Windows.Forms.TextBox();
            this.privateKey = new System.Windows.Forms.TextBox();
            this.generateKeyButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.saveKeypairButton = new System.Windows.Forms.Button();
            this.loadKeyPairButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.keypairNameText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // publicKey
            // 
            this.publicKey.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.publicKey.Location = new System.Drawing.Point(115, 62);
            this.publicKey.Multiline = true;
            this.publicKey.Name = "publicKey";
            this.publicKey.ReadOnly = true;
            this.publicKey.Size = new System.Drawing.Size(437, 24);
            this.publicKey.TabIndex = 1;
            this.publicKey.TextChanged += new System.EventHandler(this.publicKey_TextChanged);
            // 
            // privateKey
            // 
            this.privateKey.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.privateKey.Location = new System.Drawing.Point(115, 92);
            this.privateKey.Multiline = true;
            this.privateKey.Name = "privateKey";
            this.privateKey.ReadOnly = true;
            this.privateKey.Size = new System.Drawing.Size(437, 64);
            this.privateKey.TabIndex = 2;
            // 
            // generateKeyButton
            // 
            this.generateKeyButton.Location = new System.Drawing.Point(3, 3);
            this.generateKeyButton.Name = "generateKeyButton";
            this.generateKeyButton.Size = new System.Drawing.Size(187, 23);
            this.generateKeyButton.TabIndex = 3;
            this.generateKeyButton.Text = "Generate Keypair";
            this.generateKeyButton.UseVisualStyleBackColor = true;
            this.generateKeyButton.Click += new System.EventHandler(this.generateKeyButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Public Key";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Private Key";
            // 
            // saveKeypairButton
            // 
            this.saveKeypairButton.Location = new System.Drawing.Point(441, 3);
            this.saveKeypairButton.Name = "saveKeypairButton";
            this.saveKeypairButton.Size = new System.Drawing.Size(111, 23);
            this.saveKeypairButton.TabIndex = 6;
            this.saveKeypairButton.Text = "Save Keypair";
            this.saveKeypairButton.UseVisualStyleBackColor = true;
            this.saveKeypairButton.Click += new System.EventHandler(this.saveKeypairButton_Click);
            // 
            // loadKeyPairButton
            // 
            this.loadKeyPairButton.Location = new System.Drawing.Point(268, 3);
            this.loadKeyPairButton.Name = "loadKeyPairButton";
            this.loadKeyPairButton.Size = new System.Drawing.Size(111, 23);
            this.loadKeyPairButton.TabIndex = 7;
            this.loadKeyPairButton.Text = "Load Keypair";
            this.loadKeyPairButton.UseVisualStyleBackColor = true;
            this.loadKeyPairButton.Click += new System.EventHandler(this.loadKeyPairButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Name";
            // 
            // keypairNameText
            // 
            this.keypairNameText.Location = new System.Drawing.Point(115, 32);
            this.keypairNameText.Multiline = true;
            this.keypairNameText.Name = "keypairNameText";
            this.keypairNameText.Size = new System.Drawing.Size(437, 24);
            this.keypairNameText.TabIndex = 8;
            // 
            // KeyGeneratorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.keypairNameText);
            this.Controls.Add(this.loadKeyPairButton);
            this.Controls.Add(this.saveKeypairButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.generateKeyButton);
            this.Controls.Add(this.privateKey);
            this.Controls.Add(this.publicKey);
            this.Name = "KeyGeneratorControl";
            this.Size = new System.Drawing.Size(571, 187);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox publicKey;
        private System.Windows.Forms.TextBox privateKey;
        private System.Windows.Forms.Button generateKeyButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button saveKeypairButton;
        private System.Windows.Forms.Button loadKeyPairButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox keypairNameText;
    }
}

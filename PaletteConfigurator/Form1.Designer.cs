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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.insightAgentConfigurator1 = new PaletteConfigurator.InsightAgentConfigurator();
            this.chargebackConfigurationPanel1 = new PaletteConfigurator.ChargebackConfigurator.ChargebackConfigurationPanel();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(589, 683);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.insightAgentConfigurator1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(581, 657);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Agent Options";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Insight Agent Configuration";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chargebackConfigurationPanel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(581, 657);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Chargeback Model";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // insightAgentConfigurator1
            // 
            this.insightAgentConfigurator1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.insightAgentConfigurator1.Location = new System.Drawing.Point(6, 49);
            this.insightAgentConfigurator1.MaximumSize = new System.Drawing.Size(1800, 620);
            this.insightAgentConfigurator1.MinimumSize = new System.Drawing.Size(534, 620);
            this.insightAgentConfigurator1.Name = "insightAgentConfigurator1";
            this.insightAgentConfigurator1.Size = new System.Drawing.Size(569, 620);
            this.insightAgentConfigurator1.TabIndex = 0;
            // 
            // chargebackConfigurationPanel1
            // 
            this.chargebackConfigurationPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chargebackConfigurationPanel1.Location = new System.Drawing.Point(0, 0);
            this.chargebackConfigurationPanel1.Name = "chargebackConfigurationPanel1";
            this.chargebackConfigurationPanel1.Size = new System.Drawing.Size(578, 657);
            this.chargebackConfigurationPanel1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 684);
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(605, 722);
            this.Name = "Form1";
            this.Text = "Palette Configurator";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label1;
        private InsightAgentConfigurator insightAgentConfigurator1;
        private System.Windows.Forms.TabPage tabPage2;
        private ChargebackConfigurator.ChargebackConfigurationPanel chargebackConfigurationPanel1;
    }
}


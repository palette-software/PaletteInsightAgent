namespace PaletteConfigurator.ChargebackConfigurator
{
    partial class ChargebackConfigurationPanel
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
            this.timezoneSelector = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.currencySelector = new System.Windows.Forms.ComboBox();
            this.effectiveFromPicker = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.effectiveToPicker = new System.Windows.Forms.DateTimePicker();
            this.modelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.kindDataSource = new System.Windows.Forms.BindingSource(this.components);
            this.categoryIndexSource = new System.Windows.Forms.BindingSource(this.components);
            this.chargebackKindSelector = new PaletteConfigurator.ChargebackConfigurator.ChargebackKindSelector();
            this.weeklyCategoriesControl = new PaletteConfigurator.ChargebackConfigurator.WeeklyCategoriesControl();
            ((System.ComponentModel.ISupportInitialize)(this.modelBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kindDataSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.categoryIndexSource)).BeginInit();
            this.SuspendLayout();
            // 
            // timezoneSelector
            // 
            this.timezoneSelector.FormattingEnabled = true;
            this.timezoneSelector.Location = new System.Drawing.Point(142, 8);
            this.timezoneSelector.Name = "timezoneSelector";
            this.timezoneSelector.Size = new System.Drawing.Size(265, 21);
            this.timezoneSelector.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Timezone for chargeback";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Currency for chargeback";
            // 
            // currencySelector
            // 
            this.currencySelector.FormattingEnabled = true;
            this.currencySelector.Location = new System.Drawing.Point(142, 35);
            this.currencySelector.Name = "currencySelector";
            this.currencySelector.Size = new System.Drawing.Size(265, 21);
            this.currencySelector.TabIndex = 2;
            // 
            // effectiveFromPicker
            // 
            this.effectiveFromPicker.Location = new System.Drawing.Point(142, 62);
            this.effectiveFromPicker.Name = "effectiveFromPicker";
            this.effectiveFromPicker.Size = new System.Drawing.Size(201, 20);
            this.effectiveFromPicker.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(61, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Effective From";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(71, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Effective To";
            // 
            // effectiveToPicker
            // 
            this.effectiveToPicker.Location = new System.Drawing.Point(142, 88);
            this.effectiveToPicker.Name = "effectiveToPicker";
            this.effectiveToPicker.Size = new System.Drawing.Size(201, 20);
            this.effectiveToPicker.TabIndex = 6;
            // 
            // chargebackKindSelector
            // 
            this.chargebackKindSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.chargebackKindSelector.Currency = null;
            this.chargebackKindSelector.Location = new System.Drawing.Point(0, 114);
            this.chargebackKindSelector.Name = "chargebackKindSelector";
            this.chargebackKindSelector.SelectedIndex = 0;
            this.chargebackKindSelector.Size = new System.Drawing.Size(220, 502);
            this.chargebackKindSelector.TabIndex = 11;
            // 
            // weeklyCategoriesControl
            // 
            this.weeklyCategoriesControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.weeklyCategoriesControl.BoxPadding = 2;
            this.weeklyCategoriesControl.Location = new System.Drawing.Point(226, 114);
            this.weeklyCategoriesControl.Name = "weeklyCategoriesControl";
            this.weeklyCategoriesControl.Size = new System.Drawing.Size(465, 491);
            this.weeklyCategoriesControl.TabIndex = 10;
            // 
            // ChargebackConfigurationPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chargebackKindSelector);
            this.Controls.Add(this.weeklyCategoriesControl);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.effectiveToPicker);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.effectiveFromPicker);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.currencySelector);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.timezoneSelector);
            this.Name = "ChargebackConfigurationPanel";
            this.Size = new System.Drawing.Size(694, 619);
            ((System.ComponentModel.ISupportInitialize)(this.modelBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kindDataSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.categoryIndexSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox timezoneSelector;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox currencySelector;
        private System.Windows.Forms.DateTimePicker effectiveFromPicker;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker effectiveToPicker;
        private System.Windows.Forms.BindingSource modelBindingSource;
        private System.Windows.Forms.BindingSource kindDataSource;
        private WeeklyCategoriesControl weeklyCategoriesControl;
        private ChargebackKindSelector chargebackKindSelector;
        private System.Windows.Forms.BindingSource categoryIndexSource;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaletteConfigurator.ChargebackConfigurator
{
    /// <summary>
    /// A category editor dialog for Chargeback categories.
    /// </summary>
    public partial class ChargebackCategoryEditor : Form
    {

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChargebackCategory Category
        {
            get { return category; }
            set {
                category = value;
                categoryBindingSource.DataSource = value;
            }
        }

        private ChargebackCategory category;

        public ChargebackCategoryEditor()
        {
            InitializeComponent();
            Category = new ChargebackCategory { };
            nameText.DataBindings.Add(new Binding("Text", categoryBindingSource, "Name"));
            priceNumeric.DataBindings.Add(new Binding("Value", categoryBindingSource, "Price"));

            currencyTextBox.DataBindings.Add(new Binding("Text", categoryBindingSource, "Currency"));

        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

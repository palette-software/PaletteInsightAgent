using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaletteConfigurator
{
    public partial class DbConfigurator : UserControl
    {
        public DbConfigurator()
        {
            InitializeComponent();

            databaseTypeCombobox.SelectedIndex = 0;
        }
    }
}

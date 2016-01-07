using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaletteGUI
{
    public partial class PaletteHeaderBar : UserControl
    {
        public string HeaderLabel {
            get { return headerSmoothLabel.Text; }
            set { headerSmoothLabel.Text = value; }
        }

        public PaletteHeaderBar()
        {
            InitializeComponent();
           
        }

        private void paletteSiteLink_Click(object sender, EventArgs e)
        {
            HttpHelpers.openWebPage("http://palette-software.com");
        }

        private void supportCenterLink_Click(object sender, EventArgs e)
        {
            HttpHelpers.openWebPage("http://kb.palette-software.com/home");
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PaletteConfigurator
{
    /// <summary>
    /// A generic folder selector with a textbox
    /// </summary>
    public partial class FolderSelector : UserControl
    {
        public string SelectedFolder { get; set; }

        public FolderSelector()
        {
            InitializeComponent();

            insightFolderText.DataBindings.Add(new Binding("Text", bindingSource1, "SelectedFolder", true, DataSourceUpdateMode.OnPropertyChanged));

            bindingSource1.DataSource = this;
        }

        private void insightFolderButton_Click(object sender, EventArgs e)
        {

            // browse from the set folder
            folderBrowserDialog1.SelectedPath = SelectedFolder;
            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                SelectedFolder = folderBrowserDialog1.SelectedPath;
                bindingSource1.ResetBindings(false);
            }
        }
    }
}

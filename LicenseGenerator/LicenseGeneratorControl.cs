using Licensing;
using System;
using System.IO;
using System.Windows.Forms;

namespace LicenseGenerator
{
    public partial class LicenseGeneratorControl : UserControl
    {
        ILicenseManager licenseManager = new Ed25519LicenseManager();
        private const string FileDialogFilters = "license files (*.license)|*.license|All files (*.*)|*.*";

        public LicenseGeneratorControl()
        {
            InitializeComponent();
            // because of this line in the generated designer:
            // this.keyGeneratorControl.KeyPair = ((Licensing.LicenseKeyPair)(resources.GetObject("keyGeneratorControl.KeyPair")));
            // we need to generate a new key here.
            keyGeneratorControl.GenerateNewKey();
        }

        private void button1_Click(object sender, EventArgs e)
        {


            licenseTextBox.Lines = licenseManager.serializeLicense(
                licenseManager.generateLicense(licenseName.Text, licenseId.Text, validUntilPicker.Value),
                keyGeneratorControl.KeyPair.privateKey
                ).Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Do a selection on the contents of the license
            licenseTextBox.Focus();
            licenseTextBox.SelectAll();

            var result = licenseManager.isValidLicense(licenseTextBox.Text, keyGeneratorControl.KeyPair.publicKey);

            if (!result.isValid)
            {
                MessageBox.Show("Invalid license generated... Please notify the authors of the License Generator Utility." );
            }
        }

        private void saveLicenseButton_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = FileDialogFilters;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            using (var stream = saveFileDialog1.OpenFile())
            {
                if (stream == null) return;
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(licenseTextBox.Text);
                }
            }
        }
    }
}

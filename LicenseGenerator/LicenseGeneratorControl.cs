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
            try
            {
                keyGeneratorControl.GenerateNewKey();
            }
            catch (Exception e)
            {

            }
        }


        /// <summary>
        /// Returns a License object corresponding to the currently set data in the form.
        /// </summary>
        /// <returns></returns>
        private License getCurrentLicense()
        {
            return licenseManager.generateLicense(
                licenseName.Text,
                licenseId.Text,
                Decimal.ToInt32(coreCount.Value),
                validUntilPicker.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var licenseText = licenseManager.serializeLicense(
                        getCurrentLicense(),
                        keyGeneratorControl.KeyPair.privateKey);


            licenseTextBox.Lines = licenseText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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

        private void keyGeneratorControl_Load(object sender, EventArgs e)
        {

        }
    }
}

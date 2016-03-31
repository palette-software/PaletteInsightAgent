using Licensing;
using System;
using System.Linq;
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
            catch (Exception)
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
                // force the license datetime to be UTC
                DateTime.SpecifyKind(validUntilPicker.Value, DateTimeKind.Utc)
                );
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // get the current license
            var license = getCurrentLicense();
            // serialize it
            var licenseText = licenseManager.serializeLicense(license, keyGeneratorControl.KeyPair.privateKey);
            // set the output text
            licenseTextBox.Lines = LinesOf(licenseText);

            // Do a selection on the contents of the license
            licenseTextBox.Focus();
            licenseTextBox.SelectAll();

            var result = licenseManager.isValidLicense(licenseTextBox.Text, license.coreCount, keyGeneratorControl.KeyPair.publicKey);

            if (!result.isValid)
            {
                MessageBox.Show("Invalid license generated... Please notify the authors of the License Generator Utility.");
                return;
            }

            var licenseMetadata = String.Format("User: {0} // LicenseId: {1} // Cores: {2} // Valid until: {3}", license.owner, license.licenseId, license.coreCount, license.validUntilUTC );

            // set the metadata
            metaTextBox.Text = licenseMetadata;


        }

        private static string[] LinesOf(string licenseText)
        {
            return licenseText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
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

        private void copyLicenseToClipboardBtn_Click(object sender, EventArgs e)
        {
        }
    }
}

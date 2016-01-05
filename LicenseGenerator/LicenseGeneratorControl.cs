using Licensing;
using System;
using System.Windows.Forms;

namespace LicenseGenerator
{
    public partial class LicenseGeneratorControl : UserControl
    {
        ILicenseManager licenseManager = new Ed25519LicenseManager();

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

            if (result.isValid)
            {
                licenseCheck.Text = result.license.ToString();
            }
            else
            {
                licenseCheck.Text = "INVALID";
            }
        }
    }
}

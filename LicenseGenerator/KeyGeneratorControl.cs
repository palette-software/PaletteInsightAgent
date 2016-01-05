using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Licensing;
using System.IO;

namespace LicenseGenerator
{
    public partial class KeyGeneratorControl : UserControl
    {
        /// <summary>
        /// The keypair currently used
        /// </summary>
        public LicenseKeyPair KeyPair {
            get { return keyPair; }
            set
            {
                this.keyPair = value;
                if (value.name != null) keypairNameText.Text = value.name;
                if (value.publicKey != null) publicKey.Text = Convert.ToBase64String(value.publicKey);
                if (value.privateKey != null) privateKey.Text = Convert.ToBase64String(value.privateKey);
            }
        }

        private LicenseKeyPair keyPair;

        private const string DialogFilters = "keypair files (*.keypair)|*.keypair|All files (*.*)|*.*";


        private ILicenseManager licenseManager = new Ed25519LicenseManager();

        public KeyGeneratorControl()
        {
            InitializeComponent();
        }

        private void generateKeyButton_Click(object sender, EventArgs e)
        {
            GenerateNewKey();
        }

        /// <summary>
        /// Helper to generate a new key
        /// </summary>
        public void GenerateNewKey()
        {
            KeyPair = licenseManager.generateKey();
        }

        private void publicKey_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void saveKeypairButton_Click(object sender, EventArgs e)
        {
            // update the name of the keypair, but dont update the key text itself
            // (so the user cannot change it)
            var savedKeypair = KeyPair;
            savedKeypair.name = keypairNameText.Text;

            // serialize it
            var data = LicenseSerializer.keypairToString(savedKeypair);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = DialogFilters;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            using (var stream = saveFileDialog1.OpenFile())
            {
                if (stream == null) return;
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(data);
                }
            }
        }

        private void loadKeyPairButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = DialogFilters;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;


            try
            {
                using (var stream = openFileDialog1.OpenFile())
                {
                    if (stream == null) return;
                    using (var reader = new StreamReader(stream))
                    {
                        KeyPair = LicenseSerializer.keyPairFromString(reader.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }
    }
}

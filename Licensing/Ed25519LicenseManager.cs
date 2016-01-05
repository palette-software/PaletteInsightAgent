using Sodium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensing
{
    public class Ed25519LicenseManager : ILicenseManager
    {
        public LicenseKeyPair generateKey()
        {
            var keyPair = PublicKeyAuth.GenerateKeyPair();
            return new LicenseKeyPair
            {
                name = "NEW KEY @ " + DateTime.UtcNow.ToShortDateString(),
                publicKey = keyPair.PublicKey,
                privateKey = keyPair.PrivateKey
            };

        }

        public License generateLicense(string owner, string licenseId, DateTime validUntilUTC)
        {
            return new License
            {
                seed = SodiumCore.GetRandomNumber(2147483647),
                owner = owner,
                licenseId = licenseId,
                validUntilUTC = validUntilUTC
            };
        }

        public string serializeLicense(License license, byte[] privateKey)
        {
            //return Convert.ToBase64String(LicenseSerializer.licenseToBytes(license));
            return
                LicenseSerializer.toWrapped(
                    Convert.ToBase64String(
                        PublicKeyAuth.Sign(
                            LicenseSerializer.licenseToString(license),
                            privateKey)));
        }

        public ValidatedLicense isValidLicense(string licenseString, byte[] publicKey)
        {
            try
            {
                var licenseText = PublicKeyAuth.Verify(Convert.FromBase64String(LicenseSerializer.fromWrapped(licenseString)), publicKey);
                //var tmp = System.Text.Encoding.UTF8.GetString(licenseText);
                var license = LicenseSerializer.stringToLicense(licenseText);
                return new ValidatedLicense { isValid = (license.validUntilUTC > DateTime.UtcNow), license = license };
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return new ValidatedLicense { isValid = false, license = License.Invalid };
            }
        }
    }
}

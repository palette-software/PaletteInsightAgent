using Sodium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensing
{
    /// <summary>
    /// A license manager using the ED25519 digital signatures from NaCL.
    /// </summary>
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

        public License generateLicense(string owner, string licenseId, int coreCount, DateTime validUntilUTC)
        {
            return new License
            {
                // The maximum number available for SoudimCore.GetRandomNumber is 2147483647
                seed = SodiumCore.GetRandomNumber(2147483647),
                owner = owner,
                licenseId = licenseId,
                coreCount = coreCount,
                validUntilUTC = validUntilUTC
            };
        }

        public string serializeLicense(License license, byte[] privateKey)
        {
            return LicenseFormatting.toWrappedString(
                        PublicKeyAuth.Sign(
                            LicenseSerializer.licenseToString(license),
                            privateKey));
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
                return new ValidatedLicense { isValid = false, license = License.Invalid };
            }
        }
    }
}

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
            var seed = new Random().Next(0, 2147483647);
            return new License
            {
                token = generateBuffer(seed, License.TOKEN_LENGTH),
                seed = seed,
                owner = owner,
                licenseId = licenseId,
                coreCount = coreCount,
                validUntilUTC = validUntilUTC
            };
        }

        public string serializeLicense(License license, byte[] privateKey)
        {
            return LicenseFormatting.toWrappedString( PublicKeyAuth.Sign( LicenseSerializer.licenseToYamlBytes(license), privateKey));
        }

        public ValidatedLicense isValidLicense(string licenseString, int coreCount, byte[] publicKey)
        {
            try
            {
                // verify the signature
                var licenseText = PublicKeyAuth.Verify(LicenseFormatting.fromWrappedString(licenseString), publicKey);
                // deserialize the license
                var license = LicenseSerializer.yamlBytesToLicense(licenseText);
                Console.WriteLine(String.Format("Checking license: {0} / {1} cores / valid until {2}", license.owner, license.coreCount, license.validUntilUTC));
                // a license is valid if the core count is within limits and the time is ok
                var isValid = (license.coreCount >= coreCount) && (license.validUntilUTC > DateTime.UtcNow);
                return new ValidatedLicense { isValid = isValid, license = license };
            }
            catch (Exception)
            {
                return new ValidatedLicense { isValid = false, license = License.Invalid };
            }
        }


        /// <summary>
        /// Generates a buffer full of random bytes.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static byte[] generateBuffer(int seed, int size)
        {
            var o = new byte[size];
            new Random(seed).NextBytes(o);
            return o;
        }
    }
}

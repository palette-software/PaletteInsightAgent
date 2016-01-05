using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensing
{
    /// <summary>
    /// The license to be submitted
    /// </summary>
    [Serializable]
    public struct License
    {
        // Add some random data here, so that each license generated differs in some
        // bytes
        public int seed;

        public string owner;
        public string licenseId;

        /// <summary>
        /// The license is valid until this time.
        /// NOTE: the .NET XML serializer cannot serialize DateTimeOffset by default, so 
        /// we are using DateTime instead.
        /// </summary>
        public DateTime validUntilUTC;

        public static License Invalid = new License { seed = 0, owner = "", licenseId = "", validUntilUTC = new DateTime(1984,1,1) };
    }

    [Serializable]
    public struct LicenseKeyPair
    {
        public string name;

        public byte[] publicKey;
        public byte[] privateKey;
    }


    public struct ValidatedLicense
    {
        public bool isValid;
        public License license;
    }

    public interface ILicenseManager
    {
        /// <summary>
        /// Generates a new license key for signing licences
        /// </summary>
        /// <returns></returns>
        LicenseKeyPair generateKey();

        License generateLicense(string owner, string licenseId, DateTime validUntilUTC);

        /// <summary>
        /// Returns the crypted form of a license
        /// </summary>
        /// <returns></returns>
        string serializeLicense(License license, byte[] privateKey);

        /// <summary>
        /// Returns true if the license is valid.
        /// </summary>
        /// <param name="license"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        ValidatedLicense isValidLicense(string license, byte[] publicKey);
    }
}

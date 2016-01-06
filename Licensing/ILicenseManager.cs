using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licensing
{


    /// <summary>
    /// A combination struct so we can return a license + its validity
    /// </summary>
    public struct ValidatedLicense
    {
        public bool isValid;
        public License license;
    }

    /// <summary>
    /// A generic interface for license validation.
    /// 
    /// Since C# has no template support, and generics arent templates,
    /// we are bound to the License struct to use here, altough the code
    /// itself is generic enough
    /// </summary>
    public interface ILicenseManager
    {
        /// <summary>
        /// Generates a new license key for signing licences
        /// </summary>
        /// <returns></returns>
        LicenseKeyPair generateKey();

        /// <summary>
        /// Generate a new license
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="licenseId"></param>
        /// <param name="validUntilUTC"></param>
        /// <returns></returns>
        License generateLicense(string owner, string licenseId, int coreCount, DateTime validUntilUTC);

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
        ValidatedLicense isValidLicense(string license, int coreCount, byte[] publicKey);
    }
}

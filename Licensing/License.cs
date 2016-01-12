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
        /// The number of cores allowed
        /// </summary>
        public int coreCount;

        /// <summary>
        /// The license is valid until this time.
        /// NOTE: the .NET XML serializer cannot serialize DateTimeOffset by default, so 
        /// we are using DateTime instead.
        /// </summary>
        public DateTime validUntilUTC;

        public static License Invalid = new License { seed = 0, owner = "", licenseId = "", validUntilUTC = new DateTime(1984,1,1) };
    }
}

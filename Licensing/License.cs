using System;
using System.Linq;

namespace Licensing
{
    /// <summary>
    /// The license to be submitted
    /// </summary>
    public class License
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
        /// the token for sending as authentication key to the webservice.
        /// </summary>
        public byte[] token;

        /// <summary>
        /// The size of the token in bytes.
        /// </summary>
        public const int TOKEN_LENGTH = 128;

        /// <summary>
        /// The license is valid until this time.
        /// NOTE: the .NET XML serializer cannot serialize DateTimeOffset by default, so 
        /// we are using DateTime instead.
        /// </summary>
        public DateTime validUntilUTC;

        public static License Invalid = new License { seed = 0, owner = "", licenseId = "", validUntilUTC = new DateTime(1984,1,1) };

        /// <summary>
        /// Check if two licenses are the same
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            License l = (License)obj;

            // since the license expiration may be serialized/deserialized
            // to a unix timestamp, we have to make sure we ignore the
            // milisecond part here (and we cant use DateTime.Equals())

            var ticksThis = DateTimeConverter.ToTimestamp(validUntilUTC);
            var ticksThat = DateTimeConverter.ToTimestamp(l.validUntilUTC);
            

            return
                // NOTE: the seed may not has to match
                (seed == l.seed) &&
                (owner == l.owner) &&
                (licenseId == l.licenseId) &&
                (coreCount == l.coreCount) &&
                (token.SequenceEqual(l.token)) &&
                (ticksThis == ticksThat);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = hash * 16777619 ^ seed.GetHashCode();
                hash = hash * 16777619 ^ owner.GetHashCode();
                hash = hash * 16777619 ^ licenseId.GetHashCode();
                hash = hash * 16777619 ^ coreCount.GetHashCode();
                hash = hash * 16777619 ^ token.GetHashCode();
                hash = hash * 16777619 ^ validUntilUTC.GetHashCode();
                return hash;
            }
        }
    }
}

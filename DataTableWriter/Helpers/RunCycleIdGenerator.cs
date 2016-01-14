using System;

namespace DataTableWriter.Helpers
{
    public static class Generator
    {
        /// <summary>
        /// Create a starting ID based on the current epoch (unix timestamp).
        /// </summary>
        /// <returns>A 64 bit long integer, where the first 32 bits contains the current epoch
        /// and the last 32 bits are filled with zeroes.</returns>
        public static long CreteEpochPrefixedBaseId()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            long epoch = (long)t.TotalSeconds;
            long baseId = epoch << 32;
            return baseId;
        }
    }
}

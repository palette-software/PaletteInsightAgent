using System;
using System.Linq;

namespace PaletteInsightAgent.Helpers
{
    public class DateTimeConverter
    {

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a datetime to its unix timestamp equivalent (ignores timezones)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long ToTimestamp(DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Converts a unix timestamp to its datetime equivalent
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(long timestamp)
        {
            // make sure thee datetime is in UTC
            return DateTime.SpecifyKind(Epoch + TimeSpan.FromSeconds(timestamp), DateTimeKind.Utc);
        }

        /// <summary>
        /// This will return the Windows zone that matches the IANA zone, if one exists.
        /// </summary>
        /// <param name="ianaZoneId"></param>
        /// <returns></returns>
        public static string IanaToWindows(string ianaZoneId)
        {
            var utcZones = new[] { "Etc/UTC", "Etc/UCT", "Etc/GMT" };
            if (utcZones.Contains(ianaZoneId, StringComparer.Ordinal))
                return "UTC";

            var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;

            // resolve any link, since the CLDR doesn't necessarily use canonical IDs
            var links = tzdbSource.CanonicalIdMap
                .Where(x => x.Value.Equals(ianaZoneId, StringComparison.Ordinal))
                .Select(x => x.Key);

            // resolve canonical zones, and include original zone as well
            var possibleZones = tzdbSource.CanonicalIdMap.ContainsKey(ianaZoneId)
                ? links.Concat(new[] { tzdbSource.CanonicalIdMap[ianaZoneId], ianaZoneId })
                : links;

            // map the windows zone
            var mappings = tzdbSource.WindowsMapping.MapZones;
            var item = mappings.FirstOrDefault(x => x.TzdbIds.Any(possibleZones.Contains));
            if (item == null) return null;
            return item.WindowsId;
        }

        /// <summary>
        /// This will return the "primary" IANA zone that matches the given windows zone.
        /// If the primary zone is a link, it then resolves it to the canonical ID.
        /// </summary>
        /// <param name="windowsZoneId"></param>
        /// <returns></returns>
        public static string WindowsToIana(string windowsZoneId)
        {
            if (windowsZoneId.Equals("UTC", StringComparison.Ordinal))
                return "Etc/UTC";

            var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(windowsZoneId);
            if (tzi == null) return null;
            var tzid = tzdbSource.MapTimeZoneId(tzi);
            if (tzid == null) return null;
            return tzdbSource.CanonicalIdMap[tzid];
        }
    }
}

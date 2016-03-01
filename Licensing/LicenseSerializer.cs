using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Diagnostics;

namespace Licensing
{
    public class LicenseSerializer
    {

        /// <summary>
        /// The encoding to use to read the bytes from the serialized license
        /// </summary>
        private static readonly Encoding LicenseEncoding = new UTF8Encoding();

        #region YAML serialization of license

        /// <summary>
        /// Converts a license to its YAML equivalent, and returning it as UTF-8 bytes.
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        public static byte[] licenseToYamlBytes(License license)
        {
            using (var writer = new StringWriter())
            {
                var serializableLicense = YamlLicense.FromLicense(license);
                var yamlSerializer = new Serializer(namingConvention: new NullNamingConvention());
                yamlSerializer.Serialize(writer, serializableLicense);
                var licenseYaml = writer.ToString();
                Debug.WriteLine(licenseYaml);
                return LicenseEncoding.GetBytes(licenseYaml);
            }
        }

        /// <summary>
        /// Converts a list of UTF-8 bytes to a license
        /// </summary>
        /// <param name="avroBytes"></param>
        /// <returns></returns>
        public static License yamlBytesToLicense(byte[] avroBytes)
        {
            using (var reader = new StringReader(LicenseEncoding.GetString(avroBytes)))
            {
                var deserializer = new Deserializer(namingConvention: new NullNamingConvention());
                var serializedLicense = deserializer.Deserialize<YamlLicense>(reader);
                return new License
                {
                    seed = serializedLicense.seed,
                    owner = serializedLicense.owner,
                    coreCount = serializedLicense.coreCount,
                    licenseId = serializedLicense.licenseId,
                    token = Convert.FromBase64String(serializedLicense.token),
                    validUntilUTC = serializedLicense.validUntilUTC,
                };
            }
        }

        #endregion


        #region Keypairs

        public static string keypairToString(LicenseKeyPair keyPair)
        {
            XmlSerializer writer = new XmlSerializer(keyPair.GetType());
            using (var memoryStream = new MemoryStream())
            using (StreamWriter file = new StreamWriter(memoryStream))
            {
                writer.Serialize(file, keyPair);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public static LicenseKeyPair keyPairFromString(string keyPair)
        {
            return keyPairFromString(System.Text.Encoding.UTF8.GetBytes(keyPair));
        }

        public static LicenseKeyPair keyPairFromString(byte[] bytes)
        {
            XmlSerializer reader = new XmlSerializer(typeof(LicenseKeyPair));
            using (var memoryStream = new MemoryStream(bytes))
            using (StreamReader file = new StreamReader(memoryStream))
            {
                return (LicenseKeyPair)reader.Deserialize(file);
            }
        }

        #endregion

    }

    /// <summary>
    /// The serialized format of the license (differs from the actual license in handling the validity date
    /// and the token byte array)
    /// </summary>
    class YamlLicense
    {
        [YamlMember(Alias = "seed")]
        public int seed { get; set; }

        [YamlMember(Alias = "owner")]
        public string owner { get; set; }

        [YamlMember(Alias = "licenseId")]
        public string licenseId { get; set; }

        [YamlMember(Alias = "coreCount")]
        public int coreCount { get; set; }

        [YamlMember(Alias = "token")]
        public string token { get; set; }

        [YamlMember(Alias = "validUntilUTC")]
        public DateTime validUntilUTC { get; set; }

        /// <summary>
        /// Factory method for the serializable license
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        public static YamlLicense FromLicense(License license)
        {
            return new YamlLicense
            {
                seed = license.seed,
                owner = license.owner,
                licenseId = license.licenseId,
                token = Convert.ToBase64String(license.token),
                coreCount = license.coreCount,
                validUntilUTC = license.validUntilUTC,
            };
        }

    }

    public static class EnumerableHelpers
    {

        public static IEnumerable<List<T>> Partition<T>(this IList<T> source, Int32 size)
        {
            for (int i = 0; i < Math.Ceiling(source.Count / (Double)size); i++)
                yield return new List<T>(source.Skip(size * i).Take(size));
        }


        public static string Join(this IEnumerable<string> source, string joiner)
        {
            return String.Join(joiner, source);
        }
    }

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
    }

}

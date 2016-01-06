using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace Licensing
{
    public class LicenseSerializer
    {
        public static string licenseToString(License license)
        {
            XmlSerializer writer = new XmlSerializer(license.GetType());
            using (var memoryStream = new MemoryStream())
            {
                writer.Serialize(memoryStream, license);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public static byte[] licenseToBytes(License license)
        {
            XmlSerializer writer = new XmlSerializer(license.GetType());
            using (var memoryStream = new MemoryStream())
            {
                writer.Serialize(memoryStream, license);
                return memoryStream.ToArray();
            }
        }

        public static License stringToLicense(string licenseText)
        {
            return stringToLicense(System.Text.Encoding.UTF8.GetBytes(licenseText));
        }


        public static License stringToLicense(byte[] bytes)
        {
            XmlSerializer reader = new XmlSerializer(typeof(License));
            using (var memoryStream = new MemoryStream(bytes))
                return (License)reader.Deserialize(memoryStream);
        }


        #region Keypairs

        public static string keypairToString(LicenseKeyPair keyPair)
        {
            XmlSerializer writer = new XmlSerializer(keyPair.GetType());
            using (var memoryStream = new MemoryStream())
            using (StreamWriter file = new StreamWriter(memoryStream))
            {
                writer.Serialize(file, keyPair);
                //return memoryStream.ToArray();
                return Encoding.UTF8.GetString(memoryStream.ToArray());
                //return Encoding.UTF8.GetString(memoryStream.ToArray());
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

        #region width-wrapping


        public static string toWrapped(string source, int width = 60)
        {
            return source
                .ToList<char>()
                .Partition(width)
                .Select((line) => new String(line.ToArray()))
                .Join("\n");
        }

        const string WRAP_SPLIT_PATTERN = @"\s+";

        public static string fromWrapped(string source)
        {
            return Regex.Split(source, WRAP_SPLIT_PATTERN).Join("");
        }

        #endregion
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
}

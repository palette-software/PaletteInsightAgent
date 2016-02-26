using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Schema;

namespace Licensing
{
    public class LicenseSerializer
    {

        private const string LicenseAvroSchema = @"{
                            ""type"":""record"",
                            ""name"":""License"",
                            ""fields"":
                                [
                                    { ""name"":""seed"", ""type"":""int"" },
                                    { ""name"":""owner"", ""type"":""string"" },
                                    { ""name"":""licenseId"", ""type"":""string"" },
                                    { ""name"":""coreCount"", ""type"":""int"" },
                                    { ""name"":""token"", ""type"":""bytes"" },
                                    { ""name"":""validUntilUTC"", ""type"":""long"" }
                                ]
                        }";

        #region Avro serialization of license

        public static byte[] licenseToAvroBytes(License license)
        {
            var serializer = AvroSerializer.CreateGeneric(LicenseAvroSchema);
            var rootSchema = serializer.WriterSchema as RecordSchema;
            //Create a memory stream buffer
            using (var buffer = new MemoryStream())
            {
                dynamic output = new AvroRecord(serializer.WriterSchema);
                output.seed = license.seed;
                output.owner = license.owner;
                output.licenseId = license.licenseId;
                output.coreCount = license.coreCount;
                output.token = license.token;

                // convert the datetime to a timestamp
                output.validUntilUTC = DateTimeConverter.ToTimestamp(license.validUntilUTC);


                //Serialize the data to the specified stream
                serializer.Serialize(buffer, output);

                return buffer.ToArray();
            }
        }


        public static License avroBytesToLicense(byte[] avroBytes)
        {
            var serializer = AvroSerializer.CreateGeneric(LicenseAvroSchema);
            var rootSchema = serializer.WriterSchema as RecordSchema;
            using (var buffer = new MemoryStream(avroBytes))
            {
                dynamic o = serializer.Deserialize(buffer);
                return new License
                {
                    seed = o.seed,
                    owner = o.owner,
                    licenseId = o.licenseId,
                    coreCount = o.coreCount,
                    token = o.token,
                    validUntilUTC = DateTimeConverter.ToDateTime(o.validUntilUTC),
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

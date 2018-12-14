
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PaletteInsightAgent.Helpers
{
    class YamlDeserializer {
        public static IDeserializer Create(INamingConvention namingConvention = null) {
            if (namingConvention == null)
            {
                namingConvention = new NullNamingConvention();
            }
            return new DeserializerBuilder()
                        .WithNamingConvention(namingConvention)
                        .IgnoreUnmatchedProperties()
                        .Build();
        }
    }
}

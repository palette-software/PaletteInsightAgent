
using YamlDotNet.Serialization;

namespace PaletteInsightAgent.Helpers
{
    class YamlDeserializer {
        public static Deserializer Create(INamingConvention namingConvention) {
            return new DeserializerBuilder()
                        .WithNamingConvention(namingConvention)
                        .IgnoreUnmatchedProperties()
                        .Build();
        }
    }
}


using YamlDotNet.Serialization;

namespace PaletteInsightAgent.Helpers
{
    class YamlDeserializer {
        public static IDeserializer Create(INamingConvention namingConvention) {
            return new DeserializerBuilder()
                        .WithNamingConvention(namingConvention)
                        .IgnoreUnmatchedProperties()
                        .Build();
        }
    }
}

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using NJsonSchema.Generation;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace SchemaGenerator
{
    internal class SitemapSchemaGenerator
    {
        private readonly JsonSchemaGenerator _schemaGenerator;

        public SitemapSchemaGenerator()
        {
            _schemaGenerator = new JsonSchemaGenerator(new BlendSitemapSchemaGeneratorSettings());
        }

        public string Generate()
        {
            var schema = GenerateSitemapSchema();
            return schema.ToString();
        }

        private JObject GenerateSitemapSchema()
        {
            var schema = _schemaGenerator.Generate(typeof(SitemapAppSettings));
            return JsonConvert.DeserializeObject<JObject>(schema.ToJson());
        }
    }

    internal class BlendSitemapSchemaGeneratorSettings : JsonSchemaGeneratorSettings
    {
        public BlendSitemapSchemaGeneratorSettings()
        {
            AlwaysAllowAdditionalObjectProperties = true;
            SerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver(),
            };
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
            SchemaNameGenerator = new DefaultSchemaNameGenerator();
            SerializerSettings.Converters.Add(new StringEnumConverter());
            IgnoreObsoleteProperties = true;
            GenerateExamples = true;
        }
    }
}

using CommandLine;

namespace SchemaGenerator
{
    internal class Options
    {
        [Option('o', "outputFile", Required = false,
            HelpText = "",
            Default = "..\\Blend.Sitemap\\appsettings-schema.Blend.Sitemap.json")]
        public string OutputFile { get; set; }
    }
}

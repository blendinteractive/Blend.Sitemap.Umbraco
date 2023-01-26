using CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SchemaGenerator
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await Parser.Default.ParseArguments<Options>(args)
                    .WithParsedAsync(Execute);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static async Task Execute(Options options)
        {
            var generator = new SitemapSchemaGenerator();
            var schema = generator.Generate();
            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, options.OutputFile));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.WriteAllTextAsync(path, schema);
            Console.WriteLine("AppSettins Schema witten to {0}", path);
        }
    }
}

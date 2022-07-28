namespace Our.Umbraco.Blend.Sitemap
{
    public class SitemapOptions
    {
        public const string Sitemap = "Sitemap";

        public int CacheMinutes { get; set; }

        public string ExcludeBoolFieldAlias { get; set; }

        public bool IncludePageImages { get; set; }

        public bool IncludePageDocuments { get; set; }

        public SitemapDocumentTypeOptions[] DocumentTypes { get; set; }
    }

    public class SitemapDocumentTypeOptions
    {
        public string[] Aliases { get; set; }

        public string ChangeFrequency { get; set; }

        public string Priority { get; set; }
    }
}

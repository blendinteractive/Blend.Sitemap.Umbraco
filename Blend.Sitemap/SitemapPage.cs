using System.Collections.Generic;

namespace Blend.Sitemap
{
    public class SitemapPage
    {
        public SitemapPage()
        {
            ImageUrls = new List<string>();
            Alternates = new List<Alternate>();
        }

        public string Priority { get; set; }

        public string Url { get; set; }

        public string UpdateDate { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public List<string> ImageUrls { get; set; }

        public List<Alternate> Alternates { get; set; }
    }

    public class Alternate
    {
        public Alternate(string language, string url)
        {
            Language = language;
            Url = url;
        }

        public string Language { get; set; }

        public string Url { get; set; }
    }
}

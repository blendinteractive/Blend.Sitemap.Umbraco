using System.Collections.Generic;

namespace Blend.Sitemap
{
    public class SitemapPage
    {
        public SitemapPage()
        {
            ImageUrls = new List<string>();
        }

        public string Priority { get; set; }

        public string Url { get; set; }

        public string UpdateDate { get; set; }

        public ChangeFrequency ChangeFrequency { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}

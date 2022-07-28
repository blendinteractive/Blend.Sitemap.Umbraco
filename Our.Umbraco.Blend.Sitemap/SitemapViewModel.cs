using System.Collections.Generic;

namespace Our.Umbraco.Blend.Sitemap
{ 

    public class SitemapViewModel
    {
        public SitemapViewModel(List<SitemapPage> pages, bool includePageImages)
        {
            Pages = pages;
            IncludePageImages = includePageImages;
        }

        public List<SitemapPage> Pages { get; set; }

        public bool IncludePageImages { get; set; }
    }
}

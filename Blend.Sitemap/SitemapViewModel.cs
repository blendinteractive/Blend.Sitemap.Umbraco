using System.Collections.Generic;

namespace Blend.Sitemap
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

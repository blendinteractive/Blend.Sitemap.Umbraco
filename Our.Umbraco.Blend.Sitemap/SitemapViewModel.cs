using System.Collections.Generic;

namespace Our.Umbraco.Blend.Sitemap
{ 

    public class SitemapViewModel
    {
        public SitemapViewModel(List<SitemapPage> pages)
        {
            Pages = pages;
        }

        public List<SitemapPage> Pages { get; set; }
    }
}

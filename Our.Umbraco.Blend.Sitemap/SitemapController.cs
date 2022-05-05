using Microsoft.AspNetCore.Mvc;
using System.Text;
using Umbraco.Cms.Web.Common.Controllers;

namespace Our.Umbraco.Blend.Sitemap
{
    public class SitemapController : UmbracoApiController
    {
        private readonly ISitemapBuilder _sitemapBuilder;

        public SitemapController(ISitemapBuilder sitemapBuilder)
        {
            _sitemapBuilder = sitemapBuilder;
        }

        [Route("sitemap.xml")]
        public IActionResult Sitemap()
        {
            var pages = _sitemapBuilder.GetSitemap();
            
            return Content(pages.ToString(), "application/xml", Encoding.UTF8);
        }
    }
}

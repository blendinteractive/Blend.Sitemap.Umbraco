using Microsoft.AspNetCore.Mvc;

namespace Blend.Sitemap
{
    public class SitemapController : Controller
    {
        private readonly ISitemapBuilder _sitemapBuilder;

        public SitemapController(ISitemapBuilder sitemapBuilder)
        {
            _sitemapBuilder = sitemapBuilder;
        }
        [Route("{path?}/sitemap.xml")]
        [Route("sitemap.xml")]
        public IActionResult Sitemap(string path)
        {
            if (string.IsNullOrEmpty(path))
                path = string.Empty;

            var model = _sitemapBuilder.GetSitemap(path);

            return View(model);
        }
    }
}

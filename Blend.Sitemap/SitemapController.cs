using Microsoft.AspNetCore.Mvc;
using System.Text;
using Umbraco.Cms.Web.Common.Controllers;

namespace Blend.Sitemap
{
    public class SitemapController : Controller
    {
        private readonly ISitemapBuilder _sitemapBuilder;

        public SitemapController(ISitemapBuilder sitemapBuilder)
        {
            _sitemapBuilder = sitemapBuilder;
        }

        [Route("sitemap.xml")]
        public IActionResult Sitemap()
        {

            var model = _sitemapBuilder.GetSitemap();

            return View(model);
        }
    }
}

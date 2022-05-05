using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Our.Umbraco.Blend.Sitemap
{
    public class SitemapComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<SitemapOptions>().Bind(builder.Config.GetSection(SitemapOptions.Sitemap));
            builder.Services.AddTransient<ISitemapBuilder, SitemapBuilder>();
        }
    }
}

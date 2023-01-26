using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Blend.Sitemap
{
    public class SitemapComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<SitemapOptions>().Bind(builder.Config.GetSection(SitemapConstants.Sitemap));
            builder.Services.AddTransient<ISitemapBuilder, SitemapBuilder>();
            builder.AddNotificationHandler<ContentPublishedNotification, SitemapCacheClear>();
            builder.AddNotificationHandler<ContentUnpublishedNotification, SitemapCacheClear>();
            builder.AddNotificationHandler<ContentCopiedNotification, SitemapCacheClear>();
            builder.AddNotificationHandler<ContentMovedNotification, SitemapCacheClear>();
            builder.AddNotificationHandler<ContentMovedToRecycleBinNotification, SitemapCacheClear>();
            builder.AddNotificationHandler<ContentDeletedNotification, SitemapCacheClear>();
            builder.AddNotificationHandler<ContentRolledBackNotification, SitemapCacheClear>();
        }
    }
}


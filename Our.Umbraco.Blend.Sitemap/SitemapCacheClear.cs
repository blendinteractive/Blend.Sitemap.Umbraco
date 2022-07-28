using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Our.Umbraco.Blend.Sitemap
{
    public class SitemapCacheClear :
        INotificationHandler<ContentPublishedNotification>,
        INotificationHandler<ContentUnpublishedNotification>,
        INotificationHandler<ContentCopiedNotification>,
        INotificationHandler<ContentMovedNotification>,
        INotificationHandler<ContentMovedToRecycleBinNotification>,
        INotificationHandler<ContentDeletedNotification>,
        INotificationHandler<ContentRolledBackNotification>
    {
        private const string CacheKey = "sitemap";
        private readonly SitemapOptions _config;
        private readonly IAppPolicyCache _runtimeCache;
        private List<string> _documentTypeAliases;

        public SitemapCacheClear(IOptions<SitemapOptions> config, AppCaches appCaches)
        {
            _config = config.Value;
            _runtimeCache = appCaches.RuntimeCache;
            _documentTypeAliases = new List<string>();
            if (!_config.DocumentTypes.IsCollectionEmpty())
            {
                foreach (var docType in _config.DocumentTypes)
                {
                    _documentTypeAliases.AddRange(docType.Aliases);
                }
            }
        }

        private void InvalidateCache(IEnumerable<IContent> pages)
        {
            if (pages.Any(x => _documentTypeAliases.Contains(x.ContentType.Alias)))
            {
                _runtimeCache.ClearByKey(CacheKey);
            }
        }

        private void InvalidateCache(IContent page)
        {
            if (_documentTypeAliases.Contains(page.ContentType.Alias))
            {
                _runtimeCache.ClearByKey(CacheKey);
            }
        }

        private void InvalidateCache(IEnumerable<MoveEventInfo<IContent>> pages)
        {
            if (pages.Any(x => _documentTypeAliases.Contains(x.Entity.ContentType.Alias)))
            {
                _runtimeCache.ClearByKey(CacheKey);
            }
        }

        public void Handle(ContentPublishedNotification notification) => InvalidateCache(notification.PublishedEntities);
        public void Handle(ContentUnpublishedNotification notification) => InvalidateCache(notification.UnpublishedEntities);
        public void Handle(ContentCopiedNotification notification) => InvalidateCache(notification.Copy);
        public void Handle(ContentDeletedNotification notification) => InvalidateCache(notification.DeletedEntities);
        public void Handle(ContentRolledBackNotification notification) => InvalidateCache(notification.Entity);
        public void Handle(ContentMovedNotification notification) => InvalidateCache(notification.MoveInfoCollection);
        public void Handle(ContentMovedToRecycleBinNotification notification) => InvalidateCache(notification.MoveInfoCollection);
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Blend.Sitemap
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
        private readonly SitemapOptions _config;
        private readonly IAppPolicyCache _runtimeCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private List<string> _documentTypeAliases;

        private string GetCacheKey()
        {
            if (_httpContextAccessor is not null)
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request is not null)
                    return $"sitemap-{request.Scheme}://{request.Host}{request.PathBase}{request.Path}";
            }

            return "sitemap";
        }

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
                _runtimeCache.ClearByKey(GetCacheKey());
            }
        }

        private void InvalidateCache(IContent page)
        {
            if (_documentTypeAliases.Contains(page.ContentType.Alias))
            {
                _runtimeCache.ClearByKey(GetCacheKey());
            }
        }

        private void InvalidateCache(IEnumerable<MoveEventInfo<IContent>> pages)
        {
            if (pages.Any(x => _documentTypeAliases.Contains(x.Entity.ContentType.Alias)))
            {
                _runtimeCache.ClearByKey(GetCacheKey());
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

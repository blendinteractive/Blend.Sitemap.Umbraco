using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Blend.Sitemap
{
    public interface ISitemapBuilder
    {
        public SitemapViewModel GetSitemap();
    }

    public class SitemapBuilder : ISitemapBuilder
    {
        private readonly SitemapOptions _config;
        private readonly IContentTypeService _contentTypeService;
        private readonly IUmbracoContextFactory _factory;
        private readonly IAppPolicyCache _runtimeCache;
        private readonly IRelationService _relationService;
        private IPublishedContentCache _contentCache;
        private IPublishedMediaCache _mediaCache;
        private List<SitemapPage> _sitemapPages;
        private readonly TimeSpan _cacheDuration;
        private readonly string[] _imageAliases = { "Image", "umbracoMediaVectorGraphics" };

        private const string MediaRelationAlias = "umbMedia";

        public SitemapBuilder(IOptions<SitemapOptions> config, IContentTypeService contentTypeService, IUmbracoContextFactory factory, IRelationService relationService, AppCaches appCaches)
        {
            _config = config.Value;
            _contentTypeService = contentTypeService;
            _factory = factory;
            _relationService = relationService;
            _runtimeCache = appCaches.RuntimeCache;
            _sitemapPages = new List<SitemapPage>();
            _cacheDuration = TimeSpan.FromMinutes(_config.CacheMinutes > 0 ? _config.CacheMinutes : 15);
        }

        public SitemapViewModel GetSitemap()
        {
            return _runtimeCache.GetCacheItem("sitemap", () => {
                LoadPages();
                return new SitemapViewModel(_sitemapPages, _config.IncludePageImages);
            }, _cacheDuration);
        }

        private void LoadPages()
        {
            var reference = _factory.EnsureUmbracoContext();
            _contentCache = reference.UmbracoContext.Content;
            _mediaCache = reference.UmbracoContext.Media;
            _sitemapPages.Clear();
            if (_config.DocumentTypes is not null && _config.DocumentTypes.Any()) {
                foreach (var docType in _config.DocumentTypes)
                {
                    foreach (var alias in docType.Aliases)
                    {
                        var documentType = _contentCache.GetContentType(alias);
                        if (documentType is not null)
                        {
                            var pages = _contentCache.GetByContentType(documentType);
                            if (!_config.ExcludeBoolFieldAlias.IsNullOrWhiteSpace())
                            {
                                pages = pages.Where(x => !x.Value<bool>(_config.ExcludeBoolFieldAlias));
                            }
                            _sitemapPages.AddRange(pages.Select(x => LoadPage(x, docType)));
                        }
                    }
                }
            }
        }

        private SitemapPage LoadPage(IPublishedContent content, SitemapDocumentTypeOptions type)
        {
            var page = GetPage(content, type);
            if (_config.IncludePageImages || _config.IncludePageDocuments)
            {
                var media = GetMediaRelations(content.Id);
                foreach (var item in media)
                {
                    if (item.HasProperty("umbracoExtension") && item.HasValue("umbracoExtension"))
                    {
                        if (_config.IncludePageImages && _imageAliases.Contains(item.ContentType.Alias))
                        {
                            page.ImageUrls.Add(item.Url(mode: UrlMode.Absolute));
                        }
                        else if (_config.IncludePageDocuments)
                        {
                            _sitemapPages.Add(GetPage(item, type));
                        }
                    }
                }
            }
            
            return page;
        }

        private SitemapPage GetPage(IPublishedContent content, SitemapDocumentTypeOptions type)
        {
            var priority = "1.0";
            if (type.Priority < 10)
            {
                priority = $"0.{type.Priority}";
            }
            var page = new SitemapPage()
            {
                Url = content.Url(mode: UrlMode.Absolute),
                UpdateDate = string.Format("{0:s}+00:00", content.UpdateDate),
                ChangeFrequency = type.ChangeFrequency,
                Priority = priority
            };
            return page;
        }

        private List<IPublishedContent> GetMediaRelations(int pageId)
        {
            var mediaList = new List<IPublishedContent>();

            var relations = _relationService.GetByParentId(pageId, MediaRelationAlias);
            if (relations.Any())
            {
                foreach (var relation in relations)
                {
                    var media = _mediaCache.GetById(relation.ChildId);
                    if (media is not null)
                    {
                        mediaList.Add(media);
                    }
                }
            }

            return mediaList;
        }
    }
}

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

namespace Our.Umbraco.Blend.Sitemap
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
            //return _runtimeCache.GetCacheItem("sitemap", () => {
                LoadPages();
                return new SitemapViewModel(_sitemapPages);
            //}, _cacheDuration);
        }

        private void LoadPages()
        {
            var reference = _factory.EnsureUmbracoContext();
            _contentCache = reference.UmbracoContext.Content;
            _mediaCache = reference.UmbracoContext.Media;
            _sitemapPages.Clear();
            if (_config.DocumentTypes.IsCollectionEmpty())
            {
                var documentTypes = _contentTypeService.GetAllContentTypeAliases();
                if (documentTypes.Any())
                {
                    foreach (var type in documentTypes)
                    {
                        LoadDocumentTypePages(type);
                    }
                };
            }
            else
            {
                foreach (var type in _config.DocumentTypes)
                {
                    LoadDocumentTypePages(type.Alias, type);
                }
            }
        }

        private void LoadDocumentTypePages(string contentTypeAlias, SitemapDocumentTypeOptions type = null)
        {
            var documentType = _contentCache.GetContentType(contentTypeAlias);
            if (documentType is not null)
            {
                var pages = _contentCache.GetByContentType(documentType);
                if (!_config.ExcludeBoolFieldAlias.IsNullOrWhiteSpace())
                {
                    pages = pages.Where(x => !x.Value<bool>(_config.ExcludeBoolFieldAlias));
                }
                _sitemapPages.AddRange(pages.Select(x => LoadPage(x, type)));
            }
        }

        private SitemapPage LoadPage(IPublishedContent content, SitemapDocumentTypeOptions type = null)
        {
            var page = GetPage(content, type);
            var media = GetMediaRelations(content.Id);
            foreach (var item in media)
            {
                if (item.HasProperty("umbracoExtension") && item.HasValue("umbracoExtension"))
                {
                    if (_imageAliases.Contains(item.ContentType.Alias))
                    {
                        page.ImageUrls.Add(item.Url(mode: UrlMode.Absolute));
                    }
                    else
                    {
                        _sitemapPages.Add(GetPage(item, type));
                    }
                }
            }
            
            return page;
        }

        private SitemapPage GetPage(IPublishedContent content, SitemapDocumentTypeOptions type = null)
        {
            var page = new SitemapPage()
            {
                Url = content.Url(mode: UrlMode.Absolute),
                UpdateDate = string.Format("{0:s}+00:00", content.UpdateDate),
                Priority = _config.Priority,
                ChangeFrequency = _config.ChangeFrequency
            };
            if (type is not null)
            {
                page.Priority = type.Priority ?? _config.Priority;
                page.ChangeFrequency = type.ChangeFrequency ?? _config.ChangeFrequency;
            }
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

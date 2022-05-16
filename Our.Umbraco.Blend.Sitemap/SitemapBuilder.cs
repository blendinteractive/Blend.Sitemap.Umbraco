using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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
        public XDocument GetSitemap();
    }

    public class SitemapBuilder : ISitemapBuilder
    {
        private readonly SitemapOptions _config;
        private readonly IContentTypeService _contentTypeService;
        private readonly IUmbracoContextFactory _factory;
        private readonly IAppPolicyCache _runtimeCache;
        private IPublishedContentCache _contentCache;
        private List<SitemapPage> _sitemapPages;
        private readonly TimeSpan _cacheDuration;

        public SitemapBuilder(IOptions<SitemapOptions> config, IContentTypeService contentTypeService, IUmbracoContextFactory factory, AppCaches appCaches)
        {
            _config = config.Value;
            _contentTypeService = contentTypeService;
            _factory = factory;
            _runtimeCache = appCaches.RuntimeCache;
            _sitemapPages = new List<SitemapPage>();
            _cacheDuration = TimeSpan.FromMinutes(_config.CacheMinutes > 0 ? _config.CacheMinutes : 15);
        }

        public XDocument GetSitemap()
        {
            return _runtimeCache.GetCacheItem("sitemap", () => {
                var doc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", "no"),
                    LoadSitemap()
                );
                return doc;
            }, _cacheDuration);
        }

        private XElement LoadSitemap()
        {
            var reference = _factory.EnsureUmbracoContext();
            _contentCache = reference.UmbracoContext.Content;

            LoadPages();

            if (_sitemapPages.Any())
            {
                var items = _sitemapPages.Select(x => new XElement("url",
                    new XElement("loc", x.Url),
                    new XElement("lastmod", x.UpdateDate),
                    new XElement("changefreq", x.ChangeFrequency),
                    new XElement("priority", x.Priority)
                ));
                return new XElement("urlset", items);
            }
            return new XElement("urlset");
        }

        private void LoadPages()
        {
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
    }
}

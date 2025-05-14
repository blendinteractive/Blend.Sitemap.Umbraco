using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Blend.Sitemap
{
    public interface ISitemapBuilder
    {
        public SitemapViewModel GetSitemap(string sitepath);
    }

    public class SitemapBuilder : ISitemapBuilder
    {
        private readonly SitemapOptions config;
        private readonly IUmbracoContextFactory factory;
        private readonly IRelationService relationService;
        private readonly ILocalizationService localization;
        private readonly IAppPolicyCache runtimeCache;
        private readonly TimeSpan cacheDuration;
        private readonly IDomainService _domainService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private IPublishedContentCache contentCache;
        private IPublishedMediaCache mediaCache;
        private List<SitemapPage> sitemapPages;
        private ILanguage defaultLocal;
        private List<ILanguage> languages;

        private readonly string[] _imageAliases = { "Image", "umbracoMediaVectorGraphics" };
        private const string MediaRelationAlias = "umbMedia";

        public SitemapBuilder(IOptions<SitemapOptions> options, IUmbracoContextFactory factory, IRelationService relationService, ILocalizationService localization, AppCaches appCaches, IDomainService domainService, IHttpContextAccessor httpContextAccessor)
        {
            config = options.Value;
            this.factory = factory;
            this.relationService = relationService;
            this.localization = localization;
            runtimeCache = appCaches.RuntimeCache;
            sitemapPages = new List<SitemapPage>();
            languages = new List<ILanguage>();
            cacheDuration = TimeSpan.FromMinutes(config.CacheMinutes > 0 ? config.CacheMinutes : 15);
            _domainService = domainService;
            _httpContextAccessor = httpContextAccessor;
        }

        public SitemapViewModel GetSitemap(string path)
        {
            foreach (var local in localization.GetAllLanguages())
            {
                if (local.IsDefault)
                    defaultLocal = local;
                languages.Add(local);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}";
            var cacheKey = path.HasValue() ? $"sitemap-{baseUrl}" : "sitemap";

            var allDomains = _domainService.GetAll(false);
            var currentDomain = allDomains
                .FirstOrDefault(domain => baseUrl.StartsWith(domain.DomainName, StringComparison.OrdinalIgnoreCase));

            return runtimeCache.GetCacheItem(cacheKey, () =>
            {
                LoadPages(currentDomain);
                return new SitemapViewModel(sitemapPages, config.IncludePageImages);
            }, cacheDuration);
        }

        private void LoadPages(IDomain domain)
        {
            var reference = factory.EnsureUmbracoContext();
            contentCache = reference.UmbracoContext.Content;
            mediaCache = reference.UmbracoContext.Media;
            sitemapPages.Clear();

            if (domain is not null)
            {
                var rootNode = contentCache.GetById(domain.RootContentId.Value);
                if (rootNode is not null)
                    GetPages(rootNode, domain.LanguageIsoCode);
            }
            else
            {
                var roots = contentCache.GetAtRoot(defaultLocal.IsoCode);
                foreach (var root in roots)
                {
                    GetPages(root, defaultLocal.IsoCode);
                }
            }
        }

        private void GetPages(IPublishedContent root, string isoLanguageCode)
        {
            foreach (var docType in config.DocumentTypes)
            {
                foreach (var alias in docType.Aliases)
                {
                    var pages = root.DescendantsOrSelfOfType(alias, isoLanguageCode);
                    if (config.ExcludeBoolFieldAlias.HasValue())
                    {
                        pages = pages.Where(x =>
                            x.HasProperty(config.ExcludeBoolFieldAlias) &&
                            x.HasValue(config.ExcludeBoolFieldAlias, isoLanguageCode) &&
                            !x.Value<bool>(config.ExcludeBoolFieldAlias, isoLanguageCode)
                        );
                    }
                    sitemapPages.AddRange(pages.Select(x => LoadPage(x, docType, isoLanguageCode)));
                }
            }
        }

        private SitemapPage LoadPage(IPublishedContent content, SitemapDocumentTypeOptions type, string languageIsoCode)
        {
            var page = GetPage(content, type, languageIsoCode);
            if (config.IncludePageImages || config.IncludePageDocuments)
            {
                var media = GetMediaRelations(content.Id);
                foreach (var item in media)
                {
                    if (item.HasProperty("umbracoExtension") && item.HasValue("umbracoExtension"))
                    {
                        if (config.IncludePageImages && _imageAliases.Contains(item.ContentType.Alias))
                            page.ImageUrls.Add(item.Url(languageIsoCode, UrlMode.Absolute));
                        else if (config.IncludePageDocuments)
                            sitemapPages.Add(GetPage(item, type, languageIsoCode));
                    }
                }
            }
            return page;
        }

        private SitemapPage GetPage(IPublishedContent content, SitemapDocumentTypeOptions type, string languageIsoCode)
        {
            var priority = "";
            if (type.Priority != 0 && type.Priority < 10)
                priority = $"0.{type.Priority}";

            var page = new SitemapPage()
            {
                Url = content.Url(defaultLocal.IsoCode, UrlMode.Absolute),
                UpdateDate = string.Format("{0:s}+00:00", content.UpdateDate),
                ChangeFrequency = type.ChangeFrequency,
                Priority = priority
            };

            foreach (var culture in content.Cultures)
            {
                if (!culture.Key.Equals(defaultLocal.IsoCode, StringComparison.CurrentCultureIgnoreCase))
                    page.Alternates.Add(new Alternate(culture.Key, content.Url(culture.Key, UrlMode.Absolute)));
            }

            return page;
        }

        private List<IPublishedContent> GetMediaRelations(int pageId)
        {
            var mediaList = new List<IPublishedContent>();
            var relations = relationService.GetByParentId(pageId, MediaRelationAlias);
            if (relations.Any())
            {
                foreach (var relation in relations)
                {
                    var media = mediaCache.GetById(relation.ChildId);
                    if (media is not null)
                        mediaList.Add(media);
                }
            }
            return mediaList;
        }
    }
}

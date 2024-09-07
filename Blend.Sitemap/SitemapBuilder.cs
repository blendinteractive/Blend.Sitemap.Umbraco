using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
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
        private readonly SitemapOptions config;
        private readonly IUmbracoContextFactory factory;
        private readonly IRelationService relationService;
        private readonly ILocalizationService localization;
        private readonly IAppPolicyCache runtimeCache;
        private readonly TimeSpan cacheDuration;

        private IPublishedContentCache contentCache;
        private IPublishedMediaCache mediaCache;
        private List<SitemapPage> sitemapPages;
        private ILanguage defaultLocal;
        private List<ILanguage> languages;

        private readonly string[] _imageAliases = { "Image", "umbracoMediaVectorGraphics" };
        private const string MediaRelationAlias = "umbMedia";

        public SitemapBuilder(IOptions<SitemapOptions> options, IUmbracoContextFactory factory, IRelationService relationService, ILocalizationService localization, AppCaches appCaches)
        {
            config = options.Value;
            this.factory = factory;
            this.relationService = relationService;
            this.localization = localization;
            runtimeCache = appCaches.RuntimeCache;
            sitemapPages = new List<SitemapPage>();
            languages = new List<ILanguage>();
            cacheDuration = TimeSpan.FromMinutes(config.CacheMinutes > 0 ? config.CacheMinutes : 15);
        }

        public SitemapViewModel GetSitemap()
        {
            return runtimeCache.GetCacheItem("sitemap", () =>
            {
                LoadPages();
                return new SitemapViewModel(sitemapPages, config.IncludePageImages);
            }, cacheDuration);
        }

        private void LoadPages()
        {
            var reference = factory.EnsureUmbracoContext();
            contentCache = reference.UmbracoContext.Content;
            mediaCache = reference.UmbracoContext.Media;
            sitemapPages.Clear();
            foreach (var local in localization.GetAllLanguages())
            {
                if (local.IsDefault)
                    defaultLocal = local;
                else
                    languages.Add(local);
            }
            if (defaultLocal is not null)
            {
                var roots = contentCache.GetAtRoot(defaultLocal.IsoCode);
                foreach (var root in roots)
                {
                    foreach (var docType in config.DocumentTypes)
                    {
                        foreach (var alias in docType.Aliases)
                        {
                            var pages = root.DescendantsOrSelfOfType(alias, defaultLocal.IsoCode);
                            if (!config.ExcludeBoolFieldAlias.IsNullOrWhiteSpace())
                            {
                                pages = pages.Where(x => !x.HasProperty(config.ExcludeBoolFieldAlias) ||
                                    x.HasValue(config.ExcludeBoolFieldAlias, defaultLocal.IsoCode) ||
                                    (
                                        x.HasProperty(config.ExcludeBoolFieldAlias) &&
                                        x.HasValue(config.ExcludeBoolFieldAlias, defaultLocal.IsoCode) &&
                                        !x.Value<bool>(config.ExcludeBoolFieldAlias, defaultLocal.IsoCode)
                                    )
                                );
                            }
                            sitemapPages.AddRange(pages.Select(x => LoadPage(x, docType)));
                        }
                    }
                }
            }
        }

        private SitemapPage LoadPage(IPublishedContent content, SitemapDocumentTypeOptions type)
        {
            var page = GetPage(content, type);
            if (config.IncludePageImages || config.IncludePageDocuments)
            {
                var media = GetMediaRelations(content.Id);
                foreach (var item in media)
                {
                    if (item.HasProperty("umbracoExtension") && item.HasValue("umbracoExtension"))
                    {
                        if (config.IncludePageImages && _imageAliases.Contains(item.ContentType.Alias))
                        {
                            page.ImageUrls.Add(item.Url(defaultLocal.IsoCode, UrlMode.Absolute));
                        }
                        else if (config.IncludePageDocuments)
                        {
                            sitemapPages.Add(GetPage(item, type));
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
                Url = content.Url(defaultLocal.IsoCode, UrlMode.Absolute),
                UpdateDate = string.Format("{0:s}+00:00", content.UpdateDate),
                ChangeFrequency = type.ChangeFrequency,
                Priority = priority
            };
            if (languages.Any())
            {
                page.Alternates.Add(new Alternate(defaultLocal.CultureInfo.TwoLetterISOLanguageName, content.Url(defaultLocal.IsoCode, UrlMode.Absolute)));
                foreach (var item in languages)
                {
                    if (!config.ExcludeBoolFieldAlias.IsNullOrWhiteSpace() && !content.Value<bool>(config.ExcludeBoolFieldAlias, item.IsoCode))
                    {
                        page.Alternates.Add(new Alternate(item.CultureInfo.TwoLetterISOLanguageName, content.Url(item.IsoCode, UrlMode.Absolute)));
                    }
                }
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
                    {
                        mediaList.Add(media);
                    }
                }
            }
            return mediaList;
        }
    }
}

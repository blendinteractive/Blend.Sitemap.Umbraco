using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Blend.Sitemap
{
    /// <summary>
    /// Blend Sitemap Options
    /// </summary>
    [Description("Blend Sitemap Options")]
    public class SitemapOptions
    {
        /// <summary>
        /// Number of minutes to cache the sitemap for.
        /// </summary>
        [DefaultValue(15)]
        [Description("Number of minutes to cache the sitemap for")]
        public int CacheMinutes { get; set; }

        /// <summary>
        /// Alias of True/False field on document types to exclude spcecific pages
        /// </summary>
        [DefaultValue("")]
        [Description("Alias of True/False field on document types to exclude spcecific pages")]
        public string ExcludeBoolFieldAlias { get; set; }

        /// <summary>
        /// Include the page images in the sitemap. Media Type: Image, umbracoMediaVectorGraphics
        /// </summary>
        [DefaultValue(false)]
        [Description("Include the page images in the sitemap. Media Type: Image, umbracoMediaVectorGraphics")]
        public bool IncludePageImages { get; set; }

        /// <summary>
        /// Include the page documents in the sitemap. Will use the same Change Frequency and Priority as the parent page.
        /// </summary>
        [DefaultValue(false)]
        [Description("Include the page documents in the sitemap. Will use the same Change Frequency and Priority as the parent page.")]
        public bool IncludePageDocuments { get; set; }

        /// <summary>
        /// Array of specific docuemnt types for loading into the sitemap
        /// </summary>
        [DefaultValue(null)]
        [Description("Array of specific docuemnt types for loading into the sitemap")]
        public SitemapDocumentTypeOptions[] DocumentTypes { get; set; } = Array.Empty<SitemapDocumentTypeOptions>();
    }

    /// <summary>
    /// Decleration of Aliases with Change Frequecy and Priority for sitemap
    /// </summary>
    [Description("Decleration of Aliases with Change Frequecy and Priority for sitemap")]
    public class SitemapDocumentTypeOptions
    {
        /// <summary>
        /// Array of document type aliases. All the aliases will get the same Change Frequency and Priority
        /// </summary>
        [DefaultValue(new string[] { })]
        [Description("Array of document type aliases. All the aliases will get the same Change Frequency and Priority")]
        public string[] Aliases { get; set; } = Array.Empty<string>();

        /// <summary>
        /// How often the does the content in these document types change
        /// </summary>
        [DefaultValue(ChangeFrequency.monthly)]
        [Description("How often the does the content in these document types change")]
        public ChangeFrequency ChangeFrequency { get; set; }

        /// <summary>
        /// What is the priority of these document types
        /// </summary>
        [DefaultValue(Priority.Five)]
        [Description("What is the priority of these document types")]
        public Priority Priority { get; set; }
    }

    /// <summary>
    /// Options for Change Frequecy
    /// </summary>
    [Description("Options for Change Frequency")]
    public enum ChangeFrequency
    {
        always,
        hourly,
        daily,
        weekly,
        monthly,
        yearly,
        never
    }

    /// <summary>
    /// Options for Priority
    /// </summary>
    [Description("Options for Priority")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Priority
    {
        [EnumMember(Value = "0.1")]
        One,
        [EnumMember(Value = "0.2")]
        Two,
        [EnumMember(Value = "0.3")]
        Three,
        [EnumMember(Value = "0.4")]
        Four,
        [EnumMember(Value = "0.5")]
        Five,
        [EnumMember(Value = "0.6")]
        Six,
        [EnumMember(Value = "0.7")]
        Seven,
        [EnumMember(Value = "0.8")]
        Eight,
        [EnumMember(Value = "0.9")]
        Nine,
        [EnumMember(Value = "1.0")]
        Ten
    }
}

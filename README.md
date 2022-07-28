# Blend Sitemap
-----

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet version (Our.Umbraco.Blend.Sitemap)](https://img.shields.io/nuget/v/Our.Umbraco.Blend.Sitemap.svg?style=flat-square)](https://www.nuget.org/packages/Our.Umbraco.Blend.Sitemap/)

This is a lightweight package that enables /sitemap.xml at the root of an umbraco website. This package is configured using appSettings.

## Install in Umbraco CMS
---
Command Line
```
dotnet add package Our.Umbraco.Blend.Sitemap
```

Or Nuget
```
Install-Package Our.Umbraco.Blend.Sitemap
```

## Setup
---
In the `Startup.cs` there is a configuration you need to add for `/sitemap.xml` path to render.

In the `app.UseUmbraco()` Under `.WithEndpoints(u =>` add:
```
u.EndpointRouteBuilder.MapControllers();
```
This will use the route `/sitemap.xml` declared in the controller.

## Default
---
If there are no settings for the sitemap in the `appSettings.json` file nothing will be dispalyed.

## Configuration
---
In the root of your `appSettings.json` you can configure custom settings. You can also use `appSettings.[Environment].json` to have specific settings for every environment.
```
"Sitemap": {
    "ExcludeBoolFieldAlias": "aliasBoolField",
    "CacheMinutes": "15",
    "IncludePageImages": false,
    "IncludePageDocuments": false,
    "DocumentTypes: [
        {
            "Aliases": [ "homePage" ],
            "ChangeFrequency": "daily"
            "Priority": "1.0"
        },
        {
            "Aliases": [ "newsList", "eventsList", "landingPage" ],
            "ChangeFrequency": "weekly"
            "Priority": "0.9"
        },
        {
            "Aliases": [ "standardPage", "news", "event" ],
            "ChangeFrequency": "monthly"
            "Priority": "0.5"
        },
    ]
}
```

`Sitemap` is the root object and is required.

`Sitemap.CacheMinutes` is an optional integer. When filled in the sitemap will be cached for that many minutes before rebuilding. If a document type with an alias is Published, Unpublished, Copied, Moved, Moved To Recycling Deleted, or Rolled Back the cache will be cleared and regenerated on next load. If left blank the default is 15 minutes.

`Sitemap.ExcludeBoolFieldAlias` is an optional string. When filled in all documents to display will use this field to determine if that document should be excluded.

`Sitemap.IncludePageImage` is an optional boolean default false. When true will add image:image > image:loc into each page that are referenced on the page. `Image` amd `umbracoMediaVectorGraphics` are classified as image types.

`Sitemap.IncludePageDocuments` is an optional boolean default false. When true will add document that isn't an image type as a url with the same changeFrequency and priority as the document it was referenced on.

`Sitemap.DocumentTypes` is a required array of document type groups to be in the sitemap. Each group change frequency and priority will apply to that group's aliases.

`Sitemap.DocumentType.Aliases` is a required array of strings. The aliases of the document type to be included in sitemap.

`Sitemap.DocumentType.ChangeFrequency` is an optional string. Options for this are `always`, `hourly`, `daily`, `weekly`, `monthly`, `yearly`, and `never`. If not filled in these document types will not have the property.

`Sitemap.DocumentType.Priority` is an optional string. Options for this are `0.1` thorugh `0.9` and `1.0`. If not filled in these document types will not have the property.
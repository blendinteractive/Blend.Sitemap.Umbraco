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
If there are not any configurations in the the `appSettings.json` file this package will load all document types.

If there are no settings for the sitemap in the `appSettings.json` file nothing will be dispalyed.

## Configuration
---
In the root of your `appSettings.json` you can configure custom settings. You can also use `appSettings.[Environment].json` to have specific settings for every environment.
```
"Sitemap": {
    "ExcludeBoolFieldAlias": "aliasBoolField",
    "ChangeFrequency": "weekly",
    "Priority": "0.5",
    "DocumentTypes: [
        {
            "Alias": "homePage",
            "ChangeFrequency": "daily"
            "Priority": "1.0"
        },
        {
            "Alias": "standardPage"
        },
        {
            "Alias": "search"
        }
    ]
}
```

`Sitemap` is the root object and is required.

`Sitemap.ExcludeBoolFieldAlias` is an optional string. When filled in all documents to display will use this field to determine if that document should be excluded.

`Sitemap.ChangeFrequency` is a required string. Options for this are `always`, `hourly`, `daily`, `weekly`, `monthly`, `yearly`, and `never`.

`Sitemap.Priority` is a required string. Options for this are `0.1` thorugh `0.9` and `1.0`.

`Sitemap.DocumentTypes` is an optional array of document types to be in the sitemap.

`DocumentType.Alias` is a required string. The alias of the document type to be included in sitemap.

`DocumentType.ChangeFrequency` is an optional string. If not filled in will use the `Sitemap.ChangeFrequency` with the same options available.

`DocumentType.Priority` is an optional string. If not filled in will use the `Sitemap.Priority` with the same options available.
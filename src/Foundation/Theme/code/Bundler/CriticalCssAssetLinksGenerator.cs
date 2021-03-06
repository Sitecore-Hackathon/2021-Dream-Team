using Sitecore.XA.Foundation.Theming.Bundler;
using Sitecore.XA.Foundation.Theming.Configuration;
using Sitecore.XA.Foundation.Theming.EventHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using Sitecore;
using Sitecore.Data.Fields;
using Foundation.Theme.Extensions;
using Foundation.Theme.Helper;

namespace Foundation.Theme.Bundler
{
    public class CriticalCssAssetLinksGenerator : OptimizedAssetLinksGenerator
    {
        public CriticalCssAssetLinksGenerator()
            : base()
        {
        }

        //As usual we should use this static method to re-use in layout cshtml file
        public new static AssetLinks GenerateLinks(IThemesProvider themesProvider)
        {
            if (AssetContentRefresher.IsPublishing() || CriticalCssAssetLinksGenerator.IsAddingRendering())
            {
                return new AssetLinks();
            }

            return new CriticalCssAssetLinksGenerator().GenerateAssetLinks(themesProvider);
        }

        protected override string GenerateCacheKey(int hash)
        {
            return "criticalcss-" + base.GenerateCacheKey(hash);
        }

        protected virtual void GetLinks(IEnumerable<Item> themeItems, AssetServiceMode stylesMode, AssetLinks result)
        {
            foreach (var themeItem in themeItems)
            {
                var stylesItem = themeItem.Children.FirstOrDefault(x => x.InheritsFrom(Templates.Styles.TemplateId));
                if (stylesItem == null)
                {
                    continue;
                }

                var assetLinks = new AssetLinks();

                this.GetStylesLinks(stylesItem, stylesMode, assetLinks);

                foreach (string item in assetLinks.Styles.Select((string link) => "<link type=\"text/css\" as=\"style\" href=\"" + link + "\" rel=\"stylesheet\" />"))
                {
                    result.Styles.Add(item);
                }
            }
        }

        protected override void GetStylesLinks(Item stylesItem, AssetServiceMode assetServiceMode, AssetLinks assetLinks)
        {
            switch (assetServiceMode)
            {
                case AssetServiceMode.Disabled:
                    {
                        var criticalCssItems = CriticalCssHelper.GetCriticalCssItems(stylesItem)
                            .Select(x => x.BuildAssetPath());

                        foreach (var item in criticalCssItems)
                        {
                            assetLinks.Styles.Add(item);
                        }

                        break;
                    }
                case AssetServiceMode.Concatenate:
                case AssetServiceMode.ConcatenateAndMinify:
                    {
                        var criticalCssItemLink = this.GetItemLink(stylesItem, assetServiceMode);
                        if (!string.IsNullOrEmpty(criticalCssItemLink))
                        {
                            assetLinks.Styles.Add(criticalCssItemLink);
                        }

                        break;
                    }
            }
        }

        private string GetItemLink(Item themeItem, AssetServiceMode assetServiceMode)
        {
            var criticalCssItem = new AssetBundler()
                .GetOrCreateCriticalCssItemForDirectory(themeItem, assetServiceMode);
            return criticalCssItem != null && this.IsNotEmpty(criticalCssItem) ? criticalCssItem.BuildAssetPath(true) : null;
        }
    }
}
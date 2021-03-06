using Sitecore.XA.Foundation.Theming.Bundler;
using Sitecore.XA.Foundation.Theming.Configuration;
using Sitecore.XA.Foundation.Theming.EventHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

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
    }
}
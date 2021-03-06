using Sitecore.XA.Foundation.Theming.Bundler;
using Sitecore.XA.Foundation.Theming.EventHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

    }
}
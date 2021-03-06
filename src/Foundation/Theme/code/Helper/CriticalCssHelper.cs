using Sitecore;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Foundation.Theme.Extensions;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using Sitecore.XA.Foundation.Theming;
using Sitecore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.XA.Foundation.Theming.Bundler;
using Sitecore.Data;
using Sitecore.XA.Foundation.Theming.Extensions;

namespace Foundation.Theme.Helper
{
    public static class CriticalCssHelper
    {
        private static readonly IThemingContext _themingContext =
            ServiceLocator.ServiceProvider.GetService<IThemingContext>();

        public static IEnumerable<Item> GetCriticalCssItems(Item contextItem)
        {
            if (contextItem == null)
            {
                return Array.Empty<Item>();
            }

            var themeItem = contextItem.GetAncestorOrSelfOfTemplate(Sitecore.XA.Foundation.Theming.Templates._ProtectedTheme.ID);
            if (themeItem == null || !themeItem.InheritsFrom(Templates._ThemeCriticalCss.TemplateId))
            {
                return Array.Empty<Item>();
            }

            var criticalCssEnabled = MainUtil.GetBool(themeItem[Templates._ThemeCriticalCss.Fields.CriticalCssEnabled], false);
            if (!criticalCssEnabled)
            {
                return Array.Empty<Item>();
            }

            //This was a main pain during our testing since critical css optimized file contained original optimized item
            var criticalCssItems = ((MultilistField)themeItem.Fields[Templates._ThemeCriticalCss.Fields.CriticalCssItems]).GetItems()
                .Where(x => !x.InheritsFrom(Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.ID));

            return criticalCssItems;
        }

        public static IEnumerable<Item> GetAllThemes(IThemesProvider themesProvider, Item themeItem)
        {
            if (!Sitecore.Context.PageMode.IsExperienceEditor)
            {
                var contentDeliveryThemeItems = _themingContext.GetThemesWithBaseThemes(themeItem, new List<ID>(), Templates._ThemeSettings.Fields.LoadSelectedThemesOnContentDelivery);
                return (contentDeliveryThemeItems.FilterBaseThemes() ?? new List<Item>())
                    .Union(themesProvider.GetThemes(themeItem, contentDeliveryThemeItems));
            }
            else
            {
                var allThemes = _themingContext.GetAllThemes(themeItem);
                return (allThemes.FilterBaseThemes() ?? new List<Item>()).Union(themesProvider.GetThemes(themeItem, allThemes))
                    .ToList();
            }
        }
    }
}
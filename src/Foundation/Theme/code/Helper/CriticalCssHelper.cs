using Sitecore;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Foundation.Theme.Extensions;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

namespace Foundation.Theme.Helper
{
    public static class CriticalCssHelper
    {
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

            var criticalCssItems = ((MultilistField)themeItem.Fields[Templates._ThemeCriticalCss.Fields.CriticalCssItems]).GetItems()
                .Where(x => !x.InheritsFrom(Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.ID));
            return criticalCssItems;
        }
    }
}
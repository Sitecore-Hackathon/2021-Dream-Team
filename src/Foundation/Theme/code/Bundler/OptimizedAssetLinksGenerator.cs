using Sitecore.Configuration;
using Sitecore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.SecurityModel.License;
using Sitecore.XA.Foundation.Theming;
using Sitecore.XA.Foundation.Theming.Bundler;
using Sitecore.XA.Foundation.Theming.Configuration;
using Sitecore.XA.Foundation.Theming.EventHandlers;
using Sitecore.XA.Foundation.Theming.Pipelines.AssetService;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.XA.Foundation.Theming.Extensions;
using Sitecore.Data;

namespace Foundation.Theme.Bundler
{
    public class OptimizedAssetLinksGenerator : Sitecore.XA.Foundation.Theming.Bundler.AssetLinksGenerator
    {
        protected readonly AssetConfiguration _configuration;

        protected OptimizedAssetLinksGenerator()
            : base()
        {
            this._configuration = AssetConfigurationReader.Read();
        }

        //We overriding standard static method from base class
        public new static AssetLinks GenerateLinks(IThemesProvider themesProvider)
        {
            if (AssetContentRefresher.IsPublishing() || OptimizedAssetLinksGenerator.IsAddingRendering())
            {
                return new AssetLinks();
            }

            return new OptimizedAssetLinksGenerator().GenerateAssetLinks(themesProvider);
        }

        public override AssetLinks GenerateAssetLinks(IThemesProvider themesProvider)
        {
            if (!License.HasModule("Sitecore.SXA"))
            {
                HttpContext.Current.Response.Redirect(Settings.NoLicenseUrl + "?license=Sitecore.SXA");
                return null;
            }

            var assetsArgs = new AssetsArgs();
            CorePipeline.Run("assetService", assetsArgs);

            var cacheKey = this.GenerateCacheKey(assetsArgs.GetHashCode());

            if (!(HttpContext.Current.Cache[cacheKey] is AssetLinks assetLinks) || this._configuration.RequestAssetsOptimizationDisabled)
            {
                assetLinks = new AssetLinks();
                if (!assetsArgs.AssetsList.Any())
                {
                    return assetLinks;
                }

                assetsArgs.AssetsList = assetsArgs.AssetsList.OrderBy((AssetInclude a) => a.SortOrder).ToList();
                foreach (AssetInclude assetInclude in assetsArgs.AssetsList)
                {
                    this.AddAssetInclude(themesProvider, assetLinks, assetInclude);
                }

                var dependencyKeys = DatabaseRepository.GetContentDatabase().Name.ToLower() == "master"
                        ? AssetContentRefresher.MasterCacheDependencyKeys : AssetContentRefresher.WebCacheDependencyKeys;

                this.CacheLinks(cacheKey, assetLinks, dependencyKeys);
            }

            return assetLinks;
        }

        private void AddAssetInclude(IThemesProvider themesProvider, AssetLinks assetLinks, AssetInclude assetInclude)
        {
            if (assetInclude is ThemeInclude)
            {
                this.AddThemeInclude(assetInclude as ThemeInclude, assetLinks, themesProvider);
            }
            else if (assetInclude is UrlInclude)
            {
                this.AddUrlInclude(assetInclude as UrlInclude, assetLinks);
            }
            else if (assetInclude is PlainInclude)
            {
                this.AddPlainInclude(assetInclude as PlainInclude, assetLinks);
            }
        }

        protected override void AddThemeInclude(ThemeInclude themeInclude, AssetLinks result, IThemesProvider themesProvider)
        {
            var themeItem = themeInclude.Theme;
            if (themeItem == null && !themeInclude.ThemeId.IsNull)
            {
                themeItem = this.ContentRepository.GetItem(themeInclude.ThemeId);
            }

            if (themeItem == null)
            {
                return;
            }

            Log.Debug($"Starting optimized files generation process for {themeItem.Name} with following configuration {this._configuration}");

            var themingContext = ServiceLocator.ServiceProvider.GetService<IThemingContext>();
            var allThemes = themingContext.GetAllThemes(themeItem);

            IList<Item> selectedThemes;
            
            //Above part was reused from standard AssetLinksGenerator. Bellow part checking if this is Exp Editor or not, base on this info, use different field for loading base themes
            if (!Sitecore.Context.PageMode.IsExperienceEditor)
            {
                var allThemesForContentDelivery = themingContext.GetThemesWithBaseThemes(themeItem, new List<ID>(), Templates._ThemeSettings.Fields.LoadSelectedThemesOnContentDelivery);
                selectedThemes = (allThemesForContentDelivery.FilterBaseThemes() ?? new List<Item>()).Union(themesProvider.GetThemes(themeItem, allThemesForContentDelivery)).ToList();
            }
            else
            {
                selectedThemes = (allThemes.FilterBaseThemes() ?? new List<Item>()).Union(themesProvider.GetThemes(themeItem, allThemes)).ToList();
            }

            this.GetLinks(selectedThemes, this._configuration.ScriptsMode, this._configuration.StylesMode, result);
            this.GetLinks(selectedThemes, this._configuration.ScriptsMode, this._configuration.StylesMode, result);
        }
    }
}
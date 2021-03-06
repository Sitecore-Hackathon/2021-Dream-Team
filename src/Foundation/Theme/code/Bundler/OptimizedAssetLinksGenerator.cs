using Sitecore.Configuration;
using Sitecore.Pipelines;
using Sitecore.SecurityModel.License;
using Sitecore.XA.Foundation.Theming.Bundler;
using Sitecore.XA.Foundation.Theming.Configuration;
using Sitecore.XA.Foundation.Theming.EventHandlers;
using Sitecore.XA.Foundation.Theming.Pipelines.AssetService;
using System.Linq;
using System.Web;

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
    }
}
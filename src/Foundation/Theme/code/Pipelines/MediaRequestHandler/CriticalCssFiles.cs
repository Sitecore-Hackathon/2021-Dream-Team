using Foundation.Theme.Bundler;
using Sitecore.Resources.Media;
using Sitecore.XA.Foundation.MediaRequestHandler.Pipelines.MediaRequestHandler;
using Sitecore.XA.Foundation.Theming.Configuration;
using System.Linq;
using Sitecore.Data.Items;

namespace Foundation.Theme.Pipelines.MediaRequestHandler
{
    public class CriticalCssFiles : Sitecore.XA.Foundation.Theming.Pipelines.MediaRequestHandler.OptimizedFiles
    {
        public override void Process(MediaRequestHandlerArgs handlerArgs)
        {
            if (handlerArgs.Media != null)
            {
                return;
            }

            var path = handlerArgs.Context.Request.Path;
            var request = MediaManager.ParseMediaRequest(handlerArgs.Context.Request);

            //Only this part was modified from standard base class
            var source = new string[]
                {
                    Constants.ConcatenatedAndMinifiedFilename,
                    Constants.ConcatenatedFilename
                };

            if (!source.Any(path.Contains))
            {
                return;
            }

            var item = this.ReGenerateOptimizedItem(path, request);
            if (item != null)
            {
                handlerArgs.Media = MediaManager.GetMedia(item);
            }
        }

        protected override Item ReGenerateOptimizedItem(string path, MediaRequest request)
        {
            var assetBundler = new CriticalCssAssetBundler();
            var assetServiceMode = path.Contains("-min.css") ? AssetServiceMode.ConcatenateAndMinify : AssetServiceMode.Concatenate;

            var localPath = path.Substring(0, path.LastIndexOf('/'));
            var database = request.MediaUri.Database;
            var item = database.GetItem(GetMediaPath(localPath, database));

            if (item == null)
            {
                return null;
            }

            var result = assetBundler.CreateCriticalCssItemForDirectory(item, assetServiceMode);
            return result;
        }

    }
}
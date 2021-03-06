using Sitecore.Resources.Media;
using Sitecore.XA.Foundation.MediaRequestHandler.Pipelines.MediaRequestHandler;
using System.Linq;

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
    }
}
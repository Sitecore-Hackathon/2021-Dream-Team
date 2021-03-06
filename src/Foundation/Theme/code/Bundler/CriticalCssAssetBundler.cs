using Foundation.Theme.Extensions;
using Foundation.Theme.Helper;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using Sitecore.XA.Foundation.Theming.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Ajax.Utilities;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Exceptions;

namespace Foundation.Theme.Bundler
{
    /// <summary>
    /// This is a copy of AssetBundler class with some specific for critical css stuff
    /// </summary>
    public class CriticalCssAssetBundler
    {
        private static readonly object ItemLock = new object();

        public Item GetOrCreateCriticalCssItemForDirectory(Item contextItem, AssetServiceMode assetServiceMode)
        {
            var criticalCssItem = this.GetCriticalCssItemForDirectory(contextItem, assetServiceMode);
            if (criticalCssItem != null)
            {
                return criticalCssItem;
            }

            return this.CreateCriticalCssItemForDirectory(contextItem, assetServiceMode);
        }

        public Item CreateCriticalCssItemForDirectory(Item contextItem, AssetServiceMode assetServiceMode)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            {
                var minify = assetServiceMode == AssetServiceMode.ConcatenateAndMinify;
                var criticalCssItems = CriticalCssHelper.GetCriticalCssItems(contextItem);

                this.PrepareContent(criticalCssItems, minify, writer);
                return this.CreateItem(contextItem, minify, memoryStream);
            }
        }

        public Item GetCriticalCssItemForDirectory(Item contextItem, AssetServiceMode assetServiceMode)
        {
            var itemName = this.GetCriticalCssItemName(assetServiceMode);
            var itemNameWithoutExtension = Path.GetFileNameWithoutExtension(itemName);

            var themeItem = this.GetThemeItemByContextItem(contextItem);
            if (themeItem == null)
            {
                return null;
            }

            return themeItem.Axes.SelectSingleItem(
                string.Format("./Styles//*[@@templateid='{0}' AND @@name='{1}']", Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.ID, itemNameWithoutExtension));
        }

        #region Helpers

        private string GetCriticalCssItemName(AssetServiceMode assetServiceMode)
        {
            return assetServiceMode == AssetServiceMode.ConcatenateAndMinify ?
                Constants.ConcatenatedAndMinifiedFilename : Constants.ConcatenatedFilename;
        }

        private Item GetThemeItemByContextItem(Item contextItem)
        {
            if (contextItem == null)
            {
                return null;
            }

            return contextItem.GetAncestorOrSelfOfTemplate(Sitecore.XA.Foundation.Theming.Templates._ProtectedTheme.ID);
        }

        private void PrepareContent(IEnumerable<Item> items, bool minify, StreamWriter writer)
        {
            var minifier = new Minifier();

            items.ForEach(delegate (Item item)
            {
                string text = GetMediaItemContent(item);
                if (minify && !item.InheritsFrom(Sitecore.XA.Foundation.Theming.Templates.UnminifiedFile.ID))
                {
                    text = minifier.MinifyStyleSheet(text);
                }

                writer.Write(text);
                writer.Flush();
            });
        }

        private string GetMediaItemContent(MediaItem item)
        {
            string empty = string.Empty;
            try
            {
                if (item != null)
                {
                    var media = MediaManager.GetMedia(item);
                    if (media != null)
                    {
                        var mediaStream = media.GetStream();
                        if (mediaStream != null)
                        {
                            using (var stream = mediaStream.Stream)
                            {
                                return empty + this.ReadDocument(stream);
                            }
                        }

                        return empty;
                    }

                    return empty;
                }

                return empty;
            }
            catch (Exception innnerException)
            {
                throw new ItemNotFoundException($"Unable to retrieve file from Sitecore Media Library. ItemId: {item?.ID}", innnerException);
            }
        }

        private string ReadDocument(Stream stream)
        {
            using (StreamReader streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd() + Environment.NewLine + Environment.NewLine;
            }
        }

        private Item CreateItem(Item directory, bool minify, MemoryStream memoryStream)
        {
            try
            {
                lock (CriticalCssAssetBundler.ItemLock)
                {
                    var fileName = minify ? Constants.ConcatenatedAndMinifiedFilename : Constants.ConcatenatedFilename;
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                    var mediaItem = directory.Children
                        .FirstOrDefault(item =>
                            item.InheritsFrom(Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.ID) &&
                            item[Templates.File.Fields.Title] == fileNameWithoutExtension);
                    if (mediaItem != null && IsNonEmptyStream(new MediaItem(mediaItem)))
                    {
                        return mediaItem;
                    }

                    return this.CreateItemInternal(directory, minify, memoryStream);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could create optimized item in" + directory.Paths.FullPath + ": " + ex.Message, this);
            }

            return null;
        }

        private Item CreateItemInternal(Item directory, bool minify, MemoryStream memoryStream)
        {
            string fileName = minify ? Constants.ConcatenatedAndMinifiedFilename : Constants.ConcatenatedFilename;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string itemName = fileNameWithoutExtension.Replace('.', '-');
            string destination = directory.Paths.FullPath + "/" + itemName;

            using (new SecurityDisabler())
            {
                var options = new MediaCreatorOptions
                {
                    FileBased = false,
                    IncludeExtensionInItemName = false,
                    OverwriteExisting = true,
                    Versioned = false,
                    Destination = destination,
                    Database = directory.Database
                };

                var item = new MediaCreator().CreateFromStream(memoryStream, $"{itemName}.sxa_opt_css", options);

                item.Editing.BeginEdit();
                item.Name = itemName;
                item.Fields[Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.Fields.Title].Value = fileNameWithoutExtension;
                item.Fields[Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.Fields.Description].Value = "Critical CSS optimized file";
                item.Fields[Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.Fields.__Icon].Value = "People/32x32/package.png";
                item.Fields[Sitecore.XA.Foundation.Theming.Templates.OptimizedFile.Fields.Extension].Value = "css";
                item.Editing.EndEdit();

                return item;
            }
       }

        private static bool IsNonEmptyStream(MediaItem requestedOptimizedItem)
        {
            bool result;
            using (Stream mediaStream = requestedOptimizedItem.GetMediaStream())
            {
                result = (mediaStream != null && mediaStream.Length > 0L);
            }
            return result;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Resources.Media;

namespace SitecoreCommon.Helpers
{
    /// <summary>
    /// Helper to use with Sitecore fields
    /// </summary>
    public static class FieldsHelper
    {
        #region Check methods

        /// <summary>
        /// Checks if Item has specified field
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>True if Item has specified field, False otherwise</returns>
        public static bool HasField(Item item, String key)
        {
            if (item.Fields[key] == null)
            {
                Sitecore.Diagnostics.Log.Warn(
                    String.Format("FieldsHelper: There is no field '{0}' in item {1} ({2}) based on template {3} ({4})",
                            key,
                            item.ID.ToString(),
                            item.Paths.FullPath,
                            item.TemplateID.ToString(),
                            item.TemplateName),
                        item);
            }

            return (item.Fields[key] != null);
        }

        /// <summary>
        /// Checks if Item has specified field and that field is not empty
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>True if Item has specified field and that field is not empty, False otherwise</returns>
        public static bool HasNotEmptyField(Item item, String key)
        {
            return (HasField(item, key) && item.Fields[key].Value.Length > 0);
        }

        /// <summary>
        /// Returns true if Item has checked specified CheckboxField
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified CheckboxField if exists</returns>
        public static bool HasCheckedCheckboxField(Item item, String key)
        {
            return HasField(item, key) && ((CheckboxField)item.Fields[key]).Checked;
        }

        /// <summary>
        /// Checks if specified Item is actual (between specified date fields)
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="fromKey">Item from-date-field</param>
        /// <param name="toKey">Item to-date-field</param>
        /// <returns>True if actual, False otherwise</returns>
        public static Boolean IsActual(Item item, String fromKey, String toKey)
        {
            return
                (
                // - If there is no From field OR (There is From field AND (From field has date earlier or equal now OR field From has null date))
                    GetDateField(item, fromKey) == null
                    ||
                    GetDateField(item, fromKey).DateTime.CompareTo(DateTime.Now) <= 0
                    ||
                    GetDateField(item, fromKey).DateTime.CompareTo(DateTime.MinValue) == 0
                )
                &&
                (
                    GetDateField(item, toKey) == null
                    ||
                    GetDateField(item, toKey).DateTime.CompareTo(DateTime.Now) >= 0
                    ||
                    GetDateField(item, toKey).DateTime.CompareTo(DateTime.MinValue) == 0
                );
        }

        #endregion Check methods

        #region Get methods

        /// <summary>
        /// Returns string value from specified field, default specified string
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <param name="defaultValue">Default value to return</param>
        /// <returns>Field string value</returns>
        public static String GetValue(Item item, String key, String defaultValue = "")
        {
            return HasField(item, key) ? item.Fields[key].Value : defaultValue;
        }

        /// <summary>
        /// Returns value from specified field or default specified string if field does not exists or is empty
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <param name="defaultValue">Default value to return</param>
        /// <returns>Field value</returns>
        public static String GetValueOrDefaultIfEmpty(Item item, String key, String defaultValue)
        {
            return HasNotEmptyField(item, key) ? item.Fields[key].Value : defaultValue;
        }

        /// <summary>
        /// Returns url from specified field
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static String GetUrl(Item item, String key)
        {
            String url = String.Empty;

            UrlOptions opt = new UrlOptions
            {
                AlwaysIncludeServerUrl = true,
                LanguageEmbedding = LanguageEmbedding.Never
            };

            if (HasNotEmptyField(item, key))
            {
                LinkField extLink = (LinkField)item.Fields[key];

                if (extLink != null)
                {
                    if (extLink.IsInternal && extLink.TargetItem != null)
                    {
                        url = LinkManager.GetItemUrl(extLink.TargetItem, opt);
                    }

                    else if (extLink.IsMediaLink && extLink.TargetItem != null)
                    {
                        Sitecore.Resources.Media.MediaUrlOptions mediaOpt = new Sitecore.Resources.Media.MediaUrlOptions();
                        mediaOpt.AbsolutePath = true;

                        url = Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(extLink.TargetItem, mediaOpt));
                    }

                    else if (extLink.TargetItem != null)
                    {
                        url = LinkManager.GetItemUrl(extLink.TargetItem, opt);
                    }

                    else if (extLink.Url.Length > 0)
                    {
                        url = extLink.Url;
                    }
                }

                //else
                //{
                //    url = LinkManager.GetItemUrl(item, opt);
                //}
            }

            //else
            //{
            //    url = LinkManager.GetItemUrl(item, opt);
            //}

            return url;
        }

        /// <summary>
        /// Returns specified LinkField
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified LinkField if exists</returns>
        public static LinkField GetLinkField(Item item, String key)
        {
            return HasField(item, key) ? (LinkField)item.Fields[key] : null;
        }

        /// <summary>
        /// Returns specified LinkField content Url
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <param name="replaceSpaces">Replace spaces in url with %20</param>
        /// <returns>Specified LinkField content Url</returns>
        public static String GetLinkFieldUrl(Item item, String key, bool replaceSpaces = false)
        {
            String url = String.Empty;

            if (HasField(item, key))
            {
                if (FieldTypeManager.GetField(item.Fields[key]) is LinkField)
                {
                    var linkField = (LinkField)item.Fields[key];

                    if (linkField != null)
                    {
                        // - Internal link -

                        if (linkField.IsInternal && linkField.TargetItem != null)
                        {
                            url = LinkManager.GetItemUrl(linkField.TargetItem);
                        }

                        // - External link -

                        else
                        {
                            url = linkField.Url;
                        }   
                    }
                }

                else if (FieldTypeManager.GetField(item.Fields[key]) is ReferenceField)
                {
                    var referenceField = (ReferenceField)item.Fields[key];

                    if (referenceField != null && referenceField.TargetItem != null)
                    {
                        url = LinkManager.GetItemUrl(referenceField.TargetItem);
                    }
                }
            }

            if (replaceSpaces)
            {
                url = url.Replace(" ", "%20");
            }

            return url;
        }

        /// <summary>
        /// Returns specified ImageField
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified ImageField if exists</returns>
        public static ImageField GetImageField(Item item, String key)
        {
            return HasField(item, key) ? (ImageField)item.Fields[key] : null;
        }

        /// <summary>
        /// Returns specified Item image source
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <param name="replaceSpaces">Replace spaces with %20?</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns>Specified ImageField image url if exists</returns>
        public static String GetImageFieldUrl(Item item, String key, bool replaceSpaces = false, Int32 width = Int32.MinValue, Int32 height = Int32.MinValue)
        {
            String imageSrc = String.Empty;

            if (HasField(item, key))
            {
                var mediaItem = ((ImageField)item.Fields[key]).MediaItem;

                if (mediaItem != null)
                {
                    if (width != Int32.MinValue && height != Int32.MinValue)
                    {
                        var options = new MediaUrlOptions
                        {
                            Height = height,
                            Width = width
                        };

                        imageSrc = MediaManager.GetMediaUrl(mediaItem, options);
                    }

                    else
                    {
                        imageSrc = MediaManager.GetMediaUrl(mediaItem);
                    }

                    if (replaceSpaces)
                    {
                        imageSrc = imageSrc.Replace(" ", "%20");
                    }
                }
            }

            return imageSrc;
        }

        /// <summary>
        /// Returns specified Item media url
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <param name="replaceSpaces">Replace spaces with %20?</param>
        /// <returns>Specified LinkField url if exists</returns>
        public static String GetMediaFileUrl(Item item, String key, bool replaceSpaces = false)
        {
            String fileSrc = String.Empty;

            if (HasField(item, key))
            {
                if (FieldTypeManager.GetField(item.Fields[key]) is ReferenceField)
                {
                    var referenceField = (ReferenceField)item.Fields[key];

                    if (referenceField != null && referenceField.TargetItem != null)
                    {
                        fileSrc = MediaManager.GetMediaUrl(referenceField.TargetItem);
                    }
                }

                else if (FieldTypeManager.GetField(item.Fields[key]) is LinkField)
                {
                    var linkField = (LinkField)item.Fields[key];

                    // - Link is internal media item -

                    if (linkField != null && linkField.IsInternal)
                    {
                        var mediaItem = ((LinkField)item.Fields[key]).TargetItem;

                        if (mediaItem != null)
                        {
                            fileSrc = MediaManager.GetMediaUrl(mediaItem);
                        }
                    }

                    // - Link is external -

                    else
                    {
                        fileSrc = linkField.Url;
                    }
                }

                if (replaceSpaces)
                {
                    fileSrc = fileSrc.Replace(" ", "%20");
                }
            }

            return fileSrc;
        }

        /// <summary>
        /// Returns specified CheckboxField
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified CheckboxField if exists</returns>
        public static CheckboxField GetCheckboxField(Item item, String key)
        {
            return HasField(item, key) ? (CheckboxField)item.Fields[key] : null;
        }

        /// <summary>
        /// Returns specified DateField
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified DateField if exists</returns>
        public static DateField GetDateField(Item item, String key)
        {
            return HasField(item, key) ? (DateField)item.Fields[key] : null;
        }

        /// <summary>
        /// Returns specified DateField System.DateTime value
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified DateField System.DateTime value if exists</returns>
        public static DateTime GetDateFieldValue(Item item, String key)
        {
            DateTime result = DateTime.MinValue;

            if (HasField(item, key) && ((DateField)item.Fields[key]).DateTime != null)
            {
                result = ((DateField)item.Fields[key]).DateTime;
            }

            return result;
        }

        /// <summary>
        /// Returns specified Integer field value
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified Integer field if exists</returns>
        public static int GetInteger(Item item, String key)
        {
            int result = -1;

            if (HasField(item, key))
            {
                if (!Int32.TryParse(item.Fields[key].Value, out result))
                {
                    Sitecore.Diagnostics.Log.Warn(
                        String.Format("FieldsHelper: Could not parse field {0} as Integer on item {1} ({2}) based on template {3} ({4})", 
                            key, 
                            item.ID.ToString(), 
                            item.Paths.FullPath, 
                            item.TemplateID.ToString(),
                            item.TemplateName), 
                        item);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns specified Double field value
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <param name="format">Double number format</param>
        /// <returns>Specified Double field value if field exists</returns>
        public static Double GetDouble(Item item, String key, NumberFormatInfo format = null)
        {
            Double result = -1.0;

            if (HasField(item, key))
            {
                // - Read number format info - as default number in format: 120000.123 -
                format = format ?? new NumberFormatInfo
                {
                    NumberDecimalSeparator = ".",
                    NumberGroupSeparator = "",
                };

                if (!Double.TryParse(item.Fields[key].Value, NumberStyles.Any, format, out result))
                {
                    Sitecore.Diagnostics.Log.Warn(
                        String.Format("FieldsHelper: Could not parse field {0} as Double on item {1} ({2}) based on template {3} ({4})",
                            key,
                            item.ID.ToString(),
                            item.Paths.FullPath,
                            item.TemplateID.ToString(),
                            item.TemplateName),
                        item);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns specified Integer field
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Specified Integer field if exists</returns>
        public static int GetIntegerField(Item item, String key)
        {
            return GetInteger(item, key);
        }

        /// <summary>
        /// Returns items list from specified field
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>List of Items from specified field</returns>
        public static List<Item> GetMultilistFieldItems(Item item, String key, Sitecore.Data.Database database = null, Sitecore.Globalization.Language language = null)
        {
            List<Item> itemsList = new List<Item>();

            if (HasField(item, key))
            {
                MultilistField multilistField = (MultilistField)(item.Fields[key]);

                foreach (String id in multilistField.Items)
                {
                    Sitecore.Data.ID itemId = Sitecore.Data.ID.Parse(id);

                    if (database == null)
                    {
                        database = Sitecore.Context.Database;
                    }

                    Item currentItem = database.Items.GetItem(itemId, language);

                    if (currentItem != null)
                    {
                        itemsList.Add(currentItem);
                    }
                }

            }

            return itemsList;
        }

        /// <summary>
        /// Returns IDs list from specified field
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>List of IDs from specified field</returns>
        public static List<String> GetMultilistFieldItemsIDs(Item item, String key)
        {
            List<String> idsList = new List<String>();

            if (HasField(item, key))
            {
                MultilistField multilistField = (MultilistField)(item.Fields[key]);

                foreach (String id in multilistField.Items)
                {
                    idsList.Add(id);
                }
            }

            return idsList;
        }

        /// <summary>
        /// Returns ReferenceField
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Reference field</returns>
        public static ReferenceField GetReferenceField(Item item, String key)
        {
            return HasField(item, key) ? (ReferenceField)item.Fields[key] : null;
        }

        /// <summary>
        /// Returns ReferenceField Target Item
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Key name</param>
        /// <returns>Reference field</returns>
        public static Item GetReferenceFieldTargetItem(Item item, String key)
        {
            ReferenceField referenceField = GetReferenceField(item, key);

            return referenceField != null && referenceField.TargetItem != null
                ? referenceField.TargetItem
                : null;
        }

        /// <summary>
        /// Returns ReferenceField Target Item specified key value
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="key">Reference field key</param>
        /// <param name="referencedItemKey">Field key in referenced Item</param>
        /// <returns>ReferenceField Target Item specified key value</returns>
        public static String GetReferenceFieldTargetItemFieldValue(Item item, String key, String referencedItemKey)
        {
            String value = String.Empty;

            ReferenceField referenceField = GetReferenceField(item, key);

            if (referenceField != null && referenceField.TargetItem != null && HasNotEmptyField(referenceField.TargetItem, referencedItemKey))
            {
                value = GetValue(referenceField.TargetItem, referencedItemKey);
            }

            return value;
        }

        /// <summary>
        /// Returns simplified Item ID (without "}", "{", "-")
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <returns>Simplified Item ID</returns>
        public static String GetItemSimpleID(Item item)
        {
            Regex rgx = new Regex("[-{}]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            
            return rgx.Replace(item.ID.ToString(), String.Empty);
        }

        #endregion Get methods
    }
}
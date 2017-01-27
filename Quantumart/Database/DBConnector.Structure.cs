using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.Database
{
    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {
        #region GetSite...

        internal Site GetSite(int siteId)
        {
            var sites = GetSiteHashTable();
            if (sites.ContainsKey(siteId))
            {
                return (Site)sites[siteId];
            }

            throw new Exception($"Site (id = {siteId}) is not found");
        }

        public int GetSiteId(string name)
        {
            var sites = GetSiteIdHashTable();
            var key = name.ToLowerInvariant();
            return sites.ContainsKey(key) ? (int)sites[key] : 0;
        }

        public int GetSiteIdByContentId(int contentId)
        {
            var content = GetContentObject(contentId);
            return content?.SiteId ?? 0;
        }

        public string GetSiteName(int siteId)
        {
            var site = GetSite(siteId);
            return site == null ? string.Empty : site.Name;
        }

        #endregion

        #region GetContent...

        public Content GetContentObject(int id)
        {
            var hash = GetContentHashTable();
            var key = id.ToString();
            if (hash.ContainsKey(key))
            {
                return (Content)hash[key];
            }

            return CacheManager.AddContentHashEntry(key);
        }

        public int GetContentId(int siteId, string contentName)
        {
            var functionReturnValue = 0;
            var contentId = GetDynamicContentId(contentName, 0, siteId);
            if (contentId != 0)
            {
                functionReturnValue = contentId;
            }
            return functionReturnValue;
        }

        public int GetContentIdBySimpleName(int siteId, string contentName)
        {
            var hash = GetContentIdHashTable();
            Hashtable localHash;

            var siteKey = siteId.ToString();
            if (!hash.ContainsKey(siteKey))
            {
                localHash = CacheManager.AddContentIdHashEntry(siteKey);
            }
            else
            {
                localHash = (Hashtable)hash[siteKey];
            }

            var contentKey = contentName.ToLowerInvariant();
            if (localHash.ContainsKey(contentKey))
            {
                return (int)localHash[contentKey];
            }

            return 0;
        }

        public int GetContentIdByNetName(int siteId, string netName)
        {
            var key = netName.ToLowerInvariant();
            var localHash = CacheManager.GetContentIdForLinqHashTable(siteId);
            return localHash.ContainsKey(key) ? (int)localHash[key] : 0;
        }

        public int GetContentIdForAttribute(int id)
        {
            return GetContentAttributeObject(id).ContentId;
        }

        public int GetContentIdForItem(string itemIds)
        {
            return GetContentIdForItem(int.Parse(itemIds.Split(',')[0]));
        }

        public int GetContentIdForItem(int itemId)
        {
            var hash = GetItemHashTable();
            var key = itemId.ToString();
            return hash.ContainsKey(key) ? (int)hash[key] : CacheManager.AddItemHashEntry(key);
        }

        // ReSharper disable once RedundantAssignment
        public int GetDynamicContentId(string contentName, int defaultContentId, int siteId, ref int returnSiteId)
        {
            if (contentName.Contains("."))
            {
                var parts = contentName.Split(new[] { '.' }, 2);
                siteId = GetSiteId(parts[0]);
                contentName = parts[1];
            }

            returnSiteId = siteId;

            var id = GetContentIdBySimpleName(siteId, contentName);
            return id != 0 ? id : defaultContentId;
        }

        public int GetDynamicContentId(string contentName, int defaultContentId, int siteId)
        {
            var returnSiteId = 0;
            return GetDynamicContentId(contentName, defaultContentId, siteId, ref returnSiteId);
        }

        public string GetContentName(int contentId)
        {
            var content = GetContentObject(contentId);
            return content == null ? string.Empty : content.Name;
        }

        public int GetContentVirtualType(int contentId)
        {
            var content = GetContentObject(contentId);
            return content?.VirtualType ?? LegacyNotFound;
        }


        public string GetContentFieldValue(int itemId, string fieldName)
        {
            var result = "";
            var contentId = GetContentIdForItem(itemId);
            if (contentId != 0)
            {
                var tableName = "content_" + contentId;
                if (IsStage)
                {
                    tableName += "_united";
                }
                var targetTable = GetCachedData(
                    $"EXEC sp_executesql N'SELECT [{fieldName}] FROM {tableName} WITH(NOLOCK) WHERE content_item_id = @itemId', N'@itemId NUMERIC', @itemId = {itemId}");
                if (targetTable.Rows.Count > 0)
                {
                    result = targetTable.Rows[0][fieldName].ToString();
                }
            }
            return result;
        }

        #endregion

        #region GetAttribute...

        public ContentAttribute GetContentAttributeObject(int id)
        {
            var hash = GetAttributeHashTable();
            var key = id.ToString();
            if (hash.ContainsKey(key))
            {
                return (ContentAttribute)hash[key];
            }

            return CacheManager.AddAttributeHashEntry(key);
        }

        public IEnumerable<ContentAttribute> GetContentAttributeObjects(int contentId)
        {
            var hash = GetAttributeIdHashTable();
            var key = contentId.ToString();
            var attrs = hash.ContainsKey(key) ? (ArrayList)hash[key] : CacheManager.AddAttributeIdHashEntry(key);
            return from object elem in attrs select GetContentAttributeObject((int)elem);
        }

        public int GetAttributeIdByNetName(int contentId, string netName)
        {
            var key = netName.ToLowerInvariant();
            var localHash = CacheManager.GetAttributeIdForLinqHashTable(contentId);
            return localHash.ContainsKey(key) ? (int)localHash[key] : 0;
        }

        public int GetAttributeIdByNetNames(int siteId, string netContentName, string netFieldName)
        {
            return GetAttributeIdByNetName(GetContentIdByNetName(siteId, netContentName), netFieldName);
        }

        public int GetValidContentAttributeId(string valueFieldName, int contentId)
        {
            var result = 0;
            var attrId = GetAttributeIdFromFieldName(valueFieldName);
            if (attrId > 0)
            {
                var attr = GetContentAttributeObject(attrId);
                if (attr != null && attr.ContentId == contentId)
                {
                    result = attrId;
                }
            }
            return result;
        }

        private static int GetAttributeIdFromFieldName(string valueFieldName)
        {
            var result = 0;
            if (!string.IsNullOrEmpty(valueFieldName) && valueFieldName.IndexOf("field_", StringComparison.Ordinal) == 0)
            {
                var strAttrId = valueFieldName.ToLowerInvariant().Replace("field_", "");
                if (Information.IsNumeric(strAttrId))
                {
                    result = int.Parse(strAttrId);
                }
            }
            return result;
        }

        public string GetFormNameByNetNames(int siteId, string netContentName, string netFieldName)
        {
            return FieldName(GetAttributeIdByNetNames(siteId, netContentName, netFieldName));
        }
        #endregion
    }
}

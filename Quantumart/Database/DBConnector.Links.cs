using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.Database
{
    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {

        #region Links

        #region GetContentItemLinkIds

        public string GetContentItemLinkIDs(string linkFieldName, long itemId)
        {
            return GetContentItemLinkIDs(linkFieldName, itemId.ToString());
        }

        public string GetContentItemLinkIDs(string linkFieldName, string itemIds)
        {
            var info = GetRelationInfoForItem(linkFieldName, itemIds);
            return info == null ? "0" : GetContentItemLinkIDs(info.LinkId, itemIds, info.IsManyToMany);
        }

        public string GetContentItemLinkIDs(int linkId, long itemId)
        {
            return GetContentItemLinkIDs(linkId, itemId.ToString());
        }

        public string GetContentItemLinkIDs(int linkId, string itemIds)
        {
            return GetContentItemLinkIDs(linkId, itemIds, true);
        }

        public string GetContentItemLinkIDs(int linkId, long itemId, bool isManyToMany)
        {
            return GetContentItemLinkIDs(linkId, itemId.ToString(), isManyToMany);
        }

        public string GetContentItemLinkIDs(int linkId, string itemIds, bool isManyToMany)
        {
            var itemLinkHash = GetItemLinkHashTable();
            var key = CacheManager.GetItemLinkElementHashKey(linkId, itemIds, isManyToMany);
            if (itemLinkHash.ContainsKey(key))
            {
                return itemLinkHash[key].ToString();
            }
            else
            {
                return CacheManager.AddItemLinkHashEntry(linkId, itemIds, isManyToMany);
            }
        }

        #region IdsToXml

        public static string IdsToXml(IEnumerable<int> ids)
        {
            return new XElement("items", ids.Select(n => new XElement("item", n))).ToString();
        }

        public static IEnumerable<int> CommaListToIds(string commaList)
        {
            var re = new Regex(@"^[\d]+$");
            return commaList.Split(',').Where(n => re.IsMatch(n)).Select(int.Parse);
        }

        public static string CommaListToXml(string commaList)
        {

            return IdsToXml(CommaListToIds(commaList));
        }

        public static DataTable CommaListToDataTable(string commaList)
        {
            return IdsToDataTable(CommaListToIds(commaList));
        }

        public static SqlXml XmlToSqlXml(string xml)
        {
            return new SqlXml(new XmlTextReader(new StringReader(xml)));
        }

        public static SqlXml IdsToSqlXml(IEnumerable<int> ids)
        {
            return XmlToSqlXml(IdsToXml(ids));
        }

        public static DataTable IdsToDataTable(IEnumerable<int> ids)
        {
            var dt = new DataTable();
            dt.Columns.Add("id");
            if (ids != null)
                foreach (var id in ids)
                    dt.Rows.Add(id);
            return dt;
        }

        public static SqlXml CommaListToSqlXml(string commaList)
        {
            return XmlToSqlXml(CommaListToXml(commaList));
        }

        #endregion

        #endregion

        #region GetRealContentItemLinkIds

        public string GetRealContentItemLinkIDs(string linkFieldName, long itemId)
        {
            return GetRealContentItemLinkIDs(linkFieldName, itemId.ToString());
        }

        public string GetRealContentItemLinkIDs(string linkFieldName, string itemIds)
        {
            var info = GetRelationInfoForItem(linkFieldName, itemIds);
            return info == null ? String.Empty : GetRealContentItemLinkIDs(info.LinkId, itemIds, info.IsManyToMany);
        }

        public string GetRealContentItemLinkIDs(int linkId, long itemId)
        {
            return GetRealContentItemLinkIDs(linkId, itemId.ToString());
        }

        public string GetRealContentItemLinkIDs(int linkId, string itemIds)
        {
            return GetRealContentItemLinkIDs(linkId, itemIds, true);
        }

        public string GetRealContentItemLinkIDs(int linkId, long itemId, bool isManyToMany)
        {
            return GetRealContentItemLinkIDs(linkId, itemId.ToString(), isManyToMany);
        }

        public string GetRealContentItemLinkIDs(int linkId, string itemIds, bool isManyToMany)
        {
            var sql = GetContentItemLinkQuery(linkId, itemIds, isManyToMany);
            if (String.IsNullOrEmpty(sql))
                return String.Empty;
            else
            {
                var dt = GetRealData(sql);
                var result = new List<string> {"0"};
                result.AddRange(dt.Rows.OfType<DataRow>().Select(n => n[0].ToString()));
                return String.Join(",", result.ToArray());
            }
        }

        public Dictionary<int, string> GetRealContentItemLinkIDsMultiple(int linkId, IEnumerable<int> ids, bool isManyToMany)
        {
            var result = new Dictionary<int, List<string>>();
            var idstr = String.Join(",", ids.Select(n => n.ToString()).ToArray());
            var sql = GetContentItemLinkQuery(linkId, idstr, isManyToMany, true);
            if (!String.IsNullOrEmpty(sql))
            {
                var dt = GetRealData(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    var itemId = (int)(decimal)dr["item_id"];
                    var linkedItemId = (int)(decimal)dr["linked_item_id"];
                    if (!result.ContainsKey(itemId))
                    {
                        result.Add(itemId, new List<string>() { linkedItemId.ToString() });
                    }
                    else
                    {
                        result[itemId].Add(linkedItemId.ToString());
                    }
                }
            }
            return result.Select(n => new KeyValuePair<int, string>(n.Key, String.Join(",", n.Value.ToArray())))
                .ToDictionary(n => n.Key, n => n.Value);


        }


        #endregion

        #region GetContentItemLinkQuery

        public string GetContentItemLinkQuery(string linkFieldName, long itemId)
        {
            return GetContentItemLinkQuery(linkFieldName, itemId.ToString());
        }

        public string GetContentItemLinkQuery(string linkFieldName, string itemIds)
        {
            var info = GetRelationInfoForItem(linkFieldName, itemIds);
            return info == null ? String.Empty : GetContentItemLinkQuery(info.LinkId, itemIds, info.IsManyToMany);
        }

        public string GetContentItemLinkQuery(int linkId, long itemId)
        {
            return GetContentItemLinkQuery(linkId, itemId.ToString());
        }

        public string GetContentItemLinkQuery(int linkId, string itemIds)
        {
            return GetContentItemLinkQuery(linkId, itemIds, true);
        }

        public string GetContentItemLinkQuery(int linkId, long itemId, bool isManyToMany)
        {
            return GetContentItemLinkQuery(linkId, itemId.ToString(), isManyToMany);
        }

        public string GetContentItemLinkQuery(int linkId, string itemIds, bool isManyToMany, bool returnAll = false)
        {
            if (linkId == LegacyNotFound)
                return String.Empty;

            var ids = itemIds.Split(',');
            string table;

            if (isManyToMany)
            {
                table = IsStage ? "item_link_united" : "item_link";
                if (ids.Length == 1)
                {
                    return string.Format("EXEC sp_executesql N'SELECT linked_item_id FROM {2} WITH (NOLOCK) WHERE item_id = @itemId AND link_id = @linkId', N'@itemId NUMERIC, @linkId NUMERIC', @itemId = {0}, @linkId = {1};", ids[0], linkId, table);
                }
                else
                {
                    var select = returnAll ? "item_id, linked_item_id" : "DISTINCT linked_item_id";
                    return string.Format("SELECT {3} FROM {2} WITH (NOLOCK) where item_id in ({0}) AND link_id = {1}", itemIds, linkId, table, select);
                }
            }
            else
            {
                var attr = GetContentAttributeObject(linkId);
                if (attr == null)
                    return String.Empty;
                table = IsStage ? "content_{0}_united" : "content_{0}";
                table = String.Format(table, attr.ContentId);
                if (ids.Length == 1)
                {
                    return
                        $"EXEC sp_executesql N'SELECT content_item_id FROM {table} WITH(NOLOCK) WHERE [{attr.Name}] = @itemId', N'@itemId NUMERIC', @itemId = {ids[0]}";
                }
                else
                {
                    var select = returnAll ?
                        $"[{attr.Name}] as item_id, content_item_id as linked_item_id"
                        :
                        "DISTINCT content_item_id";
                    return string.Format("SELECT {3} FROM {0} WITH (NOLOCK) WHERE [{1}] in ({2})", table, attr.Name, itemIds, select);
                }
            }

        }

        #endregion

        #region GetLinkID
        // ReSharper disable once InconsistentNaming
        public int GetLinkIDForItem(string linkFieldName, int itemId)
        {
            return GetLinkID(linkFieldName, GetContentIdForItem(itemId));
        }
        // ReSharper disable once InconsistentNaming
        public int GetLinkIDForItem(string linkFieldName, string itemIds)
        {
            return GetLinkID(linkFieldName, GetContentIdForItem(itemIds));
        }
        // ReSharper disable once InconsistentNaming
        public int GetLinkID(string linkFieldName, int contentId)
        {
            var info = GetRelationInfo(linkFieldName, contentId);
            if (info != null && info.IsManyToMany)
                return info.LinkId;
            else
                return LegacyNotFound;
        }

        public int GetLinkIdByNetName(int siteId, string netName)
        {
            var key = netName.ToLowerInvariant();
            var localHash = CacheManager.GetLinkForLinqHashTable(siteId);
            return localHash.ContainsKey(key) ? (int)localHash[key] : 0;
        }

        #endregion

        #region GetRelationInfo

        internal RelationInfo GetRelationInfoForItem(string linkFieldName, string itemId)
        {
            return GetRelationInfo(linkFieldName, GetContentIdForItem(itemId));
        }

        internal RelationInfo GetRelationInfo(string linkFieldName, int contentId)
        {
            var linkHash = GetLinkHashTable();
            var contentKey = contentId.ToString();
            var nameKey = linkFieldName.ToLowerInvariant();
            var localHash = (Hashtable)linkHash[contentKey];
            if (localHash == null)
                return CacheManager.AddLinkHashEntry(contentKey, nameKey);
            else if (localHash.ContainsKey(nameKey))
                return (RelationInfo)localHash[nameKey];
            else
                return null;
        }

        #endregion

        #endregion
    }
}

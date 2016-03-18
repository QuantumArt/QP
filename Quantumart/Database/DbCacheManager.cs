using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.Info;
using Quantumart.QPublishing.Pages;

namespace Quantumart.QPublishing.Database
{
    public class DbCacheManager
    {

        #region fields and properties

        private static readonly object NullObject = new object();

        private readonly Dictionary<string, string> _queries = new Dictionary<string, string>();

        private readonly Dictionary<string, string[]> _fieldsToValidate = new Dictionary<string, string[]>();

        private readonly Dictionary<string, DataTable> _dataTables = new Dictionary<string, DataTable>();

        private static readonly Hashtable LockObjects = new Hashtable();

        private static readonly object Locklock = new object();

        private bool _storeInDictionary;

        private readonly string _webSpecificString = "DbConnector should be provided with page-specific information.";

        private readonly Hashtable _localCache = new Hashtable();


        internal const int DefaultLongExpirationTime = 60;
        
        internal const int DefaultExpirationTime = 10;
        
        internal const int DefaultShortExpirationTime = 1;
        
        internal const int MinExpirationTime = 1;
        
        internal const int DefaultPrefetchLimit = 1000;
        
        public bool StoreInDictionary {
            get { return _storeInDictionary; }
            set
            {
                _storeInDictionary = value;
                _storeInDictionary = true;
            }
        }
        
        public DBConnector Cnn { get; set; }

        public QPageEssential Page { get; set; }

        #endregion

        #region constructors

        public DbCacheManager(DBConnector newCnn)
        {
            Cnn = newCnn;
            
            _queries.Add(ConstraintKey, "SELECT CCR.CONSTRAINT_ID, CCR.ATTRIBUTE_ID, CC.CONTENT_ID FROM CONTENT_CONSTRAINT_RULE CCR WITH(NOLOCK) INNER JOIN CONTENT_CONSTRAINT CC WITH(NOLOCK) ON CC.CONSTRAINT_ID = CCR.CONSTRAINT_ID ");
            _queries.Add(StatusKey, " SELECT C.SITE_ID, C.STATUS_TYPE_ID, C.STATUS_TYPE_NAME, C.WEIGHT, C.DESCRIPTION FROM STATUS_TYPE AS C WITH(NOLOCK)");
            
            _fieldsToValidate.Add(ConstraintKey, new[] { "attribute_id" });
            _fieldsToValidate.Add(StatusKey, new[] { "status_type_id" });
        }

        #endregion

        #region General operations

        public void ResetCacheItem(string key)
        {
            if (_dataTables.ContainsKey(key))
            {
                _dataTables.Remove(key);
            }
            if (GetDataFromCache(key) != null)
            {
                RemoveDataFromCache(key);
            }
        }

        public void ClearLocalCache()
        {
            _localCache.Clear();
        }

        private bool IsPageSpecificKey(string key)
        {
            return string.IsNullOrEmpty(key);
        }

        private T GetDataFromCache<T>(string key)
        {
            return (T)GetDataFromCache(key);
        }

        private object GetDataFromCache(string key)
        {
            return Cnn.UseLocalCache ? _localCache[key] : HttpRuntime.Cache[key];
        }

        private void AddEntityToCache<T>(string cacheKey, T ht, double cacheInterval, SqlCacheDependency extDep) where T : class
        {
            object obj = ht;
            if (ht == null) obj = NullObject;
            if (Cnn.UseLocalCache)
            {
                _localCache[cacheKey] = obj;
            }
            else
            {
                if (extDep != null)
                {
                    HttpRuntime.Cache.Insert(cacheKey, obj, extDep);
                }
                else
                {
                    var dep = GetCacheDependency(cacheKey);
                    if (dep == null)
                    {
                        HttpRuntime.Cache.Insert(cacheKey, obj, null, DateTime.Now.AddMinutes(cacheInterval), Cache.NoSlidingExpiration);
                    }
                    else
                    {
                        HttpRuntime.Cache.Insert(cacheKey, obj, dep);
                    }
                }
            }
        }

        private void RemoveDataFromCache(string key)
        {
            if (Cnn.UseLocalCache)
                _localCache.Remove(key);
            else
                HttpRuntime.Cache.Remove(key);
        }

        private object GetLockObject(string key)
        {
            if (LockObjects.ContainsKey(key))
            {
                return LockObjects[key];
            }
            else
            {
                lock (Locklock)
                {
                    if (!LockObjects.ContainsKey(key))
                    {
                        var obj = new object();
                        LockObjects.Add(key, obj);
                    }
                    return LockObjects[key];
                }
            }
        }

        private CacheDependency GetCacheDependency(string cacheKey)
        {
            var cacheFilePath = GetCacheFilePath(cacheKey);
            if (!string.IsNullOrEmpty(cacheFilePath) && File.Exists(cacheFilePath))
            {
                //Dump.DumpStr(cacheKey + " dependency : " + cacheFilePath);
                return new CacheDependency(cacheFilePath);
            }
            else
            {
                //Dump.DumpStr(cacheKey + " : " + cacheFilePath);
                return null;
            }
        }

        #endregion

        #region Caching objects

        #region DataTables

        #region Tables

        internal DataTable GetCachedTable(string key)
        {
            return GetCachedTable(key, GetInternalExpirationTime(key), false);
        }
        
        internal DataTable GetCachedTable(string key, double cacheInterval, bool useDependency)
        {
            if (useDependency) {
                return GetCachedTableWithDependency(key);
            }
            else {
                return GetCachedEntity(key, cacheInterval, GetRealData);
            }
        }

        private DataTable GetRealData(string key)
        {
            return Cnn.GetRealData(GetQuery(key));
        }

        public DataTable GetCachedTableWithDependency(string key)
        {
            DataTable ht;
            if (!Cnn.CacheData)
            {
                SqlCacheDependency dep = null;
                ht = Cnn.GetRealDataWithDependency(GetQuery(key), ref dep);
            }
            else
            {
                var obj = GetDataFromCache(key);
                if (obj == NullObject)
                {
                    return null;
                }
                ht = (DataTable)obj;
                if (ht == null)
                {
                    lock (GetLockObject(key))
                    {
                        ht = GetDataFromCache<DataTable>(key);
                        if (ht == null)
                        {
                            SqlCacheDependency dep = null;
                            ht = Cnn.GetRealDataWithDependency(GetQuery(key), ref dep);
                            AddEntityToCache(key, ht, 0, dep);
                        }
                    }
                }
            }
            return ht;
        }

        internal DataTable GetDataTable(string key)
        {
            if (_dataTables.ContainsKey(key))
            {
                return _dataTables[key];
            }
            else
            {
                var obj = GetCachedTable(key);
                _dataTables.Add(key, obj);
                return obj;
            }
        }

        #endregion

        #region Views

        internal DataView GetDataView(string key, string rowFilter)
        {
            var dv = new DataView(GetDataTable(key));
            try
            {
                dv.RowFilter = rowFilter;
            }
            catch (EvaluateException)
            {
                Dump.DumpDataTable(dv.ToTable(), key);
                dv = ReGetDataView(key, rowFilter);
            }
            if (!ValidateView(dv, key))
            {
                Dump.DumpDataTable(dv.ToTable(), key);
                dv = ReGetDataView(key, rowFilter);
            }
            return dv;
        }

        private DataView ReGetDataView(string key, string rowFilter)
        {
            ResetCacheItem(key);
            var dv = new DataView(GetDataTable(key)) {RowFilter = rowFilter};
            return dv;
        }

        private bool ValidateView(DataView view, string key)
        {
            var fieldsArray = GetFieldsToValidate(key);
            if (view.Count == 0)
            {
                return true;
            }
            else
            {
                return fieldsArray.All(field => view.Table.Columns.Contains(field));
            }
        }

        private string[] GetFieldsToValidate(string key)
        {
            var newKey = key.Replace(GetDataKeyPrefix, string.Empty);
            if (string.Equals(key, newKey))
            {
                Debug.Assert(_fieldsToValidate.ContainsKey(key), "_fieldsToValidate.ContainsKey(key):" + key);
                return _fieldsToValidate[key];
            }
            else
            {
                return new string[0];
            }
        }

        #endregion

        #region Queries

        private string GetBaseObjectsQuery()
        {
            return "SELECT PT.TEMPLATE_NAME, PT.NET_TEMPLATE_NAME, PT.SITE_ID, PT.PAGE_TEMPLATE_ID, P.PAGE_ID, OBJ.[OBJECT_NAME], OBJ.[OBJECT_ID], OBJ.NET_OBJECT_NAME, OBJF.FORMAT_NAME, OBJF.NET_FORMAT_NAME, OBJ.OBJECT_FORMAT_ID AS DEFAULT_FORMAT_ID, P.PAGE_FOLDER, OBJF.OBJECT_FORMAT_ID AS CURRENT_FORMAT_ID FROM OBJECT AS OBJ INNER JOIN OBJECT_FORMAT AS OBJF ON OBJ.OBJECT_ID = OBJF.OBJECT_ID INNER JOIN PAGE_TEMPLATE PT ON OBJ.PAGE_TEMPLATE_ID = PT.PAGE_TEMPLATE_ID LEFT JOIN PAGE AS P ON P.PAGE_ID = OBJ.PAGE_ID";
        }

        private string GetPageQuery()
        {
            return "SELECT P.PAGE_ID, P.PAGE_TEMPLATE_ID, P.PAGE_NAME, PAGE_FILENAME, P.PROXY_CACHE, P.CACHE_HOURS, P.CHARSET, P.GENERATE_TRACE, P.PAGE_FOLDER, P.DISABLE_BROWSE_SERVER, P.SET_LAST_MODIFIED_HEADER, P.SEND_NOCACHE_HEADERS FROM PAGE P INNER JOIN PAGE_TEMPLATE PT ON P.PAGE_TEMPLATE_ID = PT.PAGE_TEMPLATE_ID";
        }

        private string GetPageTemplateQuery()
        {
            return "SELECT PT.SITE_ID, PT.PAGE_TEMPLATE_ID, PT.TEMPLATE_FOLDER, PT.NET_TEMPLATE_NAME, PT.TEMPLATE_NAME, PT.CHARSET, PT.SEND_NOCACHE_HEADERS FROM PAGE_TEMPLATE PT";
        }

        private string GetContentQuery()
        {
            return "SELECT C.CONTENT_ID, C.CONTENT_NAME, C.NET_CONTENT_NAME, C.VIRTUAL_TYPE, C.SITE_ID, C.MAX_NUM_OF_STORED_VERSIONS, S.SITE_NAME, CWB.WORKFLOW_ID FROM CONTENT AS C WITH(NOLOCK) INNER JOIN SITE AS S  WITH(NOLOCK) ON C.SITE_ID = S.SITE_ID LEFT JOIN CONTENT_WORKFLOW_BIND CWB on CWB.CONTENT_ID = C.CONTENT_ID";
        }

        private string GetAttributeQuery()
        {
            var sb = new StringBuilder();
            sb.Append("SELECT C.SITE_ID, AT.TYPE_NAME, AT.DATABASE_TYPE, AT.INPUT_TYPE, CA.ATTRIBUTE_ID, CA.CONTENT_ID, CA.ATTRIBUTE_NAME, CA.NET_ATTRIBUTE_NAME, ");
            sb.Append(" CA.INPUT_MASK, CA.ATTRIBUTE_SIZE, CA.DEFAULT_VALUE, CA.ATTRIBUTE_TYPE_ID, CA.INDEX_FLAG, CA.ATTRIBUTE_ORDER, CA.DESCRIPTION, ");
            sb.Append(" CA.REQUIRED, CA.IS_CLASSIFIER, CA.AGGREGATED, CA.READONLY_FLAG, CA.RELATED_IMAGE_ATTRIBUTE_ID, CA.PERSISTENT_ATTR_ID, CA.JOIN_ATTR_ID, CA.LINK_ID, CA.USE_SITE_LIBRARY, CA.SUBFOLDER, CA.DISABLE_VERSION_CONTROL, ");
            sb.Append(" DIA.WIDTH, DIA.HEIGHT, DIA.TYPE, DIA.QUALITY, DIA.MAX_SIZE, DIA.ATTRIBUTE_ID AS DYNAMIC_IMAGE_ATTRIBUTE_ID, ");
            sb.Append(" S.USE_SITE_LIBRARY AS SOURCE_USE_SITE_LIBRARY, S.CONTENT_ID AS SOURCE_CONTENT_ID, ");
            sb.Append(" CA.BACK_RELATED_ATTRIBUTE_ID AS BASE_RELATION_ATTRIBUTE_ID, RCA.CONTENT_ID AS BASE_RELATION_CONTENT_ID, RCA.ATTRIBUTE_NAME AS BASE_RELATION_ATTRIBUTE_NAME, ");
            sb.Append(" CL.LINKED_CONTENT_ID, COALESCE(RA.CONTENT_ID, CL.LINKED_CONTENT_ID, RCA.CONTENT_ID) AS RELATED_CONTENT_ID");
            sb.Append(" FROM CONTENT_ATTRIBUTE AS CA WITH(NOLOCK) ");
            sb.Append(" INNER JOIN ATTRIBUTE_TYPE AS AT WITH(NOLOCK) ON AT.ATTRIBUTE_TYPE_ID=CA.ATTRIBUTE_TYPE_ID ");
            sb.Append(" INNER JOIN CONTENT AS C WITH(NOLOCK) ON C.CONTENT_ID = CA.CONTENT_ID ");
            sb.Append(" LEFT JOIN DYNAMIC_IMAGE_ATTRIBUTE AS DIA WITH(NOLOCK) ON CA.ATTRIBUTE_ID=DIA.ATTRIBUTE_ID ");
            sb.Append(" LEFT JOIN CONTENT_ATTRIBUTE S ON CA.PERSISTENT_ATTR_ID = S.ATTRIBUTE_ID ");
            sb.Append(" LEFT JOIN CONTENT_ATTRIBUTE RA ON CA.RELATED_ATTRIBUTE_ID = RA.ATTRIBUTE_ID ");
            sb.Append(" LEFT JOIN CONTENT_ATTRIBUTE RCA ON CA.BACK_RELATED_ATTRIBUTE_ID = RCA.ATTRIBUTE_ID ");
            sb.Append(" LEFT JOIN CONTENT_LINK CL ON CA.LINK_ID = CL.LINK_ID AND CA.CONTENT_ID = CL.CONTENT_ID");

            return sb.ToString();
        }

        public string GetQuery(string key)
        {
            var newKey = key.Replace(GetDataKeyPrefix, string.Empty);
            if (string.Equals(key, newKey))
            {
                Debug.Assert(_queries.ContainsKey(key), "_queries.ContainsKey(key):" + key);
                return _queries[key];
            }
            else
            {
                return newKey;
            }
        }

        #endregion

        #endregion

        #region Hashtables

        internal Hashtable GetCachedHashTable(string key)
        {
            return GetCachedHashTable(key, GetInternalExpirationTime(key));
        }

        internal Hashtable GetCachedHashTable(string key, double cacheInterval)
        {
            if (Page == null && IsPageSpecificKey(key)) throw new Exception(_webSpecificString);
            return GetCachedEntity(key, cacheInterval, FillHashTable);
        }

        internal DualHashTable GetCachedDualHashTable(string key)
        {
            return GetCachedDualHashTable(key, GetInternalExpirationTime(key));
        }

        internal DualHashTable GetCachedDualHashTable(string key, double cacheInterval)
        {
            if (Page == null && IsPageSpecificKey(key)) throw new Exception(_webSpecificString);
            return GetCachedEntity(key, cacheInterval, FillDualHashTable);
        }

        internal Hashtable GetAttributeIdForLinqHashTable(int contentId)
        {
            var key = contentId.ToString();
            var attributeIdForLinqHash = GetCachedHashTable(AttributeIdForLinqHashKey);
            if (attributeIdForLinqHash.ContainsKey(key))
                return (Hashtable)attributeIdForLinqHash[key];
            else
            {
                var dualHash = GetCachedDualHashTable(AttributeHashKey);
                var resultHash = new Hashtable();
                var idHash = dualHash.Ids.ContainsKey(key) ? (ArrayList)dualHash.Ids[key] : AddAttributeIdHashEntry(key);
                foreach (var item in idHash)
                {
                    if (item != null)
                    {
                        var itemKey = ((int)item).ToString();
                        var attr = (ContentAttribute)dualHash.Items[itemKey];
                        if (attr?.LinqName != null)
                        {
                            var linqKey = attr.LinqName.ToLowerInvariant();
                            resultHash[linqKey] = attr.Id;
                        }
                        else if (attr == null)
                        {
                            Dump.DumpStr($"{DateTime.Now}: Attribute returned for key '{key}' is null");
                        }
                    }
                    else
                    {
                        Dump.DumpStr($"{DateTime.Now}: Item in arraylist for key '{key}' is null");
                    }

                }
                attributeIdForLinqHash[key] = resultHash;
                return resultHash;
            }
        }

        internal Hashtable GetLinkForLinqHashTable(int siteId)
        {
            var key = siteId.ToString();
            var linkForLinqHash = GetCachedHashTable(LinkForLinqHashKey);
            if (linkForLinqHash.ContainsKey(key))
                return (Hashtable)linkForLinqHash[key];
            else
            {
                var localHash = new Hashtable();
                var dt2 = Cnn.GetRealData(
                    $"EXEC sp_executesql N'SELECT LINK_ID, NET_LINK_NAME FROM CONTENT_TO_CONTENT CC INNER JOIN CONTENT C ON CC.L_CONTENT_ID = C.CONTENT_ID WHERE SITE_ID = @Id', N'@Id NUMERIC', @Id = {siteId}");
                foreach (DataRow row in dt2.Rows)
                {
                    var itemKey = Convert.ToString(row["NET_LINK_NAME"]);
                    if (!String.IsNullOrEmpty(itemKey))
                    {
                        itemKey = itemKey.ToLowerInvariant();
                        var linkId = (int)(decimal)row["LINK_ID"];
                        localHash[itemKey] = linkId;
                    }
                }
                linkForLinqHash[key] = localHash;
                return localHash;
            }
        }

        internal Hashtable GetContentIdForLinqHashTable(int siteId)
        {
            var key = siteId.ToString();
            var contentIdForLinqHash = GetCachedHashTable(ContentIdForLinqHashKey);
            if (contentIdForLinqHash.ContainsKey(key))
                return (Hashtable)contentIdForLinqHash[key];
            else
            {
                var resultHash = new Hashtable();
                var dualHash = GetCachedDualHashTable(ContentHashKey);
                var idHash = dualHash.Ids.ContainsKey(key) ? (Hashtable)dualHash.Ids[key] : AddContentIdHashEntry(key);

                foreach (var item in idHash.Values)
                {
                    var itemKey = ((int)item).ToString();
                    var content = (Content)dualHash.Items[itemKey];
                    if (content.LinqName != null)
                    {
                        var linqKey = content.LinqName.ToLowerInvariant();
                        resultHash[linqKey] = content.Id;
                    }
                }
                contentIdForLinqHash[key] = resultHash;
                return resultHash;
            }

        }

        #region Add hash entries

        internal ContentAttribute AddAttributeHashEntry(string itemKey)
        {
            var dt = Cnn.GetRealData("EXEC sp_executesql N'SELECT CONTENT_ID FROM CONTENT_ATTRIBUTE WITH(NOLOCK) WHERE ATTRIBUTE_ID = @attrId', N'@attrId NUMERIC', @attrId = " + itemKey);
            var contentId = dt.Rows.Count == 0 ? 0 : Int32.Parse(dt.Rows[0]["CONTENT_ID"].ToString());
            ContentAttribute result = null;
            AddAttributeIdHashEntry(contentId.ToString(), itemKey, ref result);
            return result;
        }

        internal RelationInfo AddLinkHashEntry(string contentKey, string nameKey)
        {
            RelationInfo result = null;
            var linkHash = GetCachedHashTable(LinkHashKey);
            var attributeHash = new Hashtable();

            var dt2 = Cnn.GetRealData(
                $"EXEC sp_executesql N'SELECT ATTRIBUTE_NAME, LINK_ID, BACK_RELATED_ATTRIBUTE_ID FROM CONTENT_ATTRIBUTE WHERE (LINK_ID IS NOT NULL OR BACK_RELATED_ATTRIBUTE_ID IS NOT NULL) AND CONTENT_ID = @Id', N'@Id NUMERIC', @Id = {contentKey}");
            foreach (DataRow row in dt2.Rows)
            {
                var key = row["ATTRIBUTE_NAME"].ToString().ToLowerInvariant();
                var linkId = (int?)CastDbNull.To<decimal?>(row["LINK_ID"]);
                var info = new RelationInfo
                {
                    LinkId = linkId ?? (int) (decimal) row["BACK_RELATED_ATTRIBUTE_ID"],
                    IsManyToMany = linkId.HasValue
                };
                attributeHash[key] = info;
                if (key == nameKey) result = info;
            }

            lock (GetLockObject(LinkHashKey))
            {
                linkHash[contentKey] = attributeHash;
            }
            return result;
        }

        internal Int32 AddItemHashEntry(string itemKey)
        {
            var dt = Cnn.GetRealData("EXEC sp_executesql N'SELECT CONTENT_ID FROM CONTENT_ITEM WITH(NOLOCK) WHERE CONTENT_ITEM_ID = @itemId', N'@itemId NUMERIC', @itemId = " + itemKey);
            var contentId = dt.Rows.Count == 0 ? 0 : Int32.Parse(dt.Rows[0]["CONTENT_ID"].ToString());

            var contentPrefetchKey = "content" + contentId;
            var hash = GetCachedHashTable(ItemHashKey);

            lock (GetLockObject(ItemHashKey))
            {
                hash[itemKey] = contentId;
                if (!hash.ContainsKey(contentPrefetchKey))
                {
                    var dt2 = Cnn.GetRealData(
                        $"EXEC sp_executesql N'SELECT TOP {GetPrefetchLimit()} CONTENT_ITEM_ID FROM CONTENT_ITEM WITH(NOLOCK) WHERE CONTENT_ID = @Id ORDER BY CONTENT_ITEM_ID DESC', N'@Id NUMERIC', @Id = {contentId}");
                    foreach (DataRow row in dt2.Rows)
                    {
                        hash[row["content_item_id"].ToString()] = contentId;
                    }
                    hash[contentPrefetchKey] = 1;
                }
            }
            return contentId;
        }

        internal Content AddContentHashEntry(string itemKey)
        {
            var dt = Cnn.GetRealData("EXEC sp_executesql N'SELECT SITE_ID FROM CONTENT WITH(NOLOCK) WHERE CONTENT_ID = @contentId', N'@contentId NUMERIC', @contentId = " + itemKey);
            var siteId = dt.Rows.Count == 0 ? 0 : Int32.Parse(dt.Rows[0]["SITE_ID"].ToString());
            Content content = null;
            AddContentIdHashEntry(siteId.ToString(), itemKey, ref content);
            return content;
        }

        internal Hashtable AddContentIdHashEntry(string siteKey)
        {
            Content content = null;
            return AddContentIdHashEntry(siteKey, "", ref content);
        }

        internal Hashtable AddContentIdHashEntry(string siteKey, string itemKey, ref Content result)
        {
            var dualHash = GetCachedDualHashTable(ContentHashKey);
            var localHash = new Hashtable();
            lock (GetLockObject(ContentHashKey))
            {
                var dt2 = Cnn.GetRealData(
                    $"EXEC sp_executesql N'{GetContentQuery()} WHERE C.SITE_ID = @Id', N'@Id NUMERIC', @Id = {siteKey}");
                foreach (DataRow row in dt2.Rows)
                {
                    var current = new Content
                    {
                        Id = (int) (decimal) row["CONTENT_ID"],
                        Name = row["CONTENT_NAME"].ToString(),
                        SiteId = (int) (decimal) row["SITE_ID"],
                        VirtualType = (int) (decimal) row["VIRTUAL_TYPE"]
                    };
                    var linqName = Convert.ToString(row["NET_CONTENT_NAME"]);
                    current.LinqName = !String.IsNullOrEmpty(linqName) ? linqName : DefaultLinqNameGenerator.GetMappedName(current.Name, current.Id, true) + "Article";
                    current.MaxVersionNumber = (byte)row["MAX_NUM_OF_STORED_VERSIONS"];
                    current.WorkflowId = (int?)CastDbNull.To<decimal?>(row["WORKFLOW_ID"]);

                    var idKey = current.Id.ToString();
                    var nameKey = current.Name.ToLowerInvariant();

                    dualHash.Items[idKey] = current;

                    if (idKey == itemKey) result = current;

                    localHash[nameKey] = current.Id;
                }
                dualHash.Ids[siteKey] = localHash;
            }
            return localHash;
        }

        internal ArrayList AddAttributeIdHashEntry(string contentKey)
        {
            ContentAttribute fake = null;
            return AddAttributeIdHashEntry(contentKey, "", ref fake);
        }

        internal ArrayList AddAttributeIdHashEntry(string contentKey, string itemKey, ref ContentAttribute result)
        {

            lock (GetLockObject(AttributeHashKey))
            {
                var dualHash = GetCachedDualHashTable(AttributeHashKey);

                var dt2 = Cnn.GetRealData(
                    $"EXEC sp_executesql N'{GetAttributeQuery()} WHERE CA.CONTENT_ID = @Id', N'@Id NUMERIC', @Id = {contentKey}");
                var attrs = new ArrayList(dt2.Rows.Count);
                foreach (DataRow row in dt2.Rows)
                {

                    var id = (int)(decimal)row["attribute_id"];
                    attrs.Add(id);
                    var key = id.ToString();

                    var current = new ContentAttribute()
                    {
                        Id = id,
                        Name = row["ATTRIBUTE_NAME"].ToString(),
                        Description = row["DESCRIPTION"].ToString(),
                        Type = (AttributeType)(int)(decimal)row["ATTRIBUTE_TYPE_ID"],
                        Size = (int)(decimal)row["ATTRIBUTE_SIZE"],
                        ContentId = (int)(decimal)row["CONTENT_ID"],
                        SiteId = (int)(decimal)row["SITE_ID"],
                        SubFolder = row["SUBFOLDER"].ToString(),
                        DisableVersionControl = (bool)row["DISABLE_VERSION_CONTROL"],
                        UseSiteLibrary = (bool)row["USE_SITE_LIBRARY"],
                        DbTypeName = row["DATABASE_TYPE"].ToString().ToUpperInvariant(),
                        RelatedImageId = (int?)CastDbNull.To<decimal?>(row["RELATED_IMAGE_ATTRIBUTE_ID"]),
                        RelatedContentId = (int?)CastDbNull.To<decimal?>(row["RELATED_CONTENT_ID"]),
                        DefaultValue = row["DEFAULT_VALUE"].ToString(),
                        LinkId = (int?)CastDbNull.To<decimal?>(row["LINK_ID"]),
                        Required = DBConnector.GetNumBool(row["REQUIRED"]),
                        IsClassifier = (bool)row["IS_CLASSIFIER"],
                        Aggregated = (bool)row["AGGREGATED"],
                        InputMask = row["INPUT_MASK"].ToString()
                    };

                    var linqName = Convert.ToString(row["NET_ATTRIBUTE_NAME"]);
                    current.LinqName = !String.IsNullOrEmpty(linqName) ? linqName : DefaultLinqNameGenerator.GetMappedName(current.Name, current.Id, false);

                    if (row["SOURCE_CONTENT_ID"] != DBNull.Value)
                    {
                        current.SourceAttribute = new SourceAttribute()
                        {
                            Id = (int)(decimal)row["PERSISTENT_ATTR_ID"],
                            ContentId = (int)(decimal)row["SOURCE_CONTENT_ID"],
                            UseSiteLibrary = (bool)row["SOURCE_USE_SITE_LIBRARY"]
                        };
                    }

                    if (row["DYNAMIC_IMAGE_ATTRIBUTE_ID"] != DBNull.Value)
                    {
                        if (current.RelatedImageId != null)
                            current.DynamicImage = new DynamicImageAttribute()
                            {
                                Id = current.Id,
                                BaseImageId = (int)current.RelatedImageId,
                                Type = row["TYPE"].ToString(),
                                MaxSize = (bool)row["MAX_SIZE"],
                                Quality = CastDbNull.To<short>(row["QUALITY"]),
                                Width = CastDbNull.To<short>(row["WIDTH"]),
                                Height = CastDbNull.To<short>(row["HEIGHT"])
                            };
                    }

                    if (row["BASE_RELATION_ATTRIBUTE_ID"] != DBNull.Value)
                    {
                        current.BackRelation = new BackRelation()
                        {
                            Id = (int)(decimal)row["BASE_RELATION_ATTRIBUTE_ID"],
                            ContentId = (int)(decimal)row["BASE_RELATION_CONTENT_ID"],
                            Name = row["BASE_RELATION_ATTRIBUTE_NAME"].ToString()
                        };
                    }

                    dualHash.Items[key] = current;
                    if (itemKey == key) result = current;
                }

                dualHash.Ids[contentKey] = attrs;
                return attrs;
            }
        }

        internal string AddItemLinkHashEntry(int linkId, string itemId, bool isManyToMany)
        {
            var result = Cnn.GetRealContentItemLinkIDs(linkId, itemId, isManyToMany);
            lock (GetLockObject(ItemLinkHashKey))
            {
                GetCachedHashTable(ItemLinkHashKey)[GetItemLinkElementHashKey(linkId, itemId, isManyToMany)] = result;
            }
            return result;
        }

        #endregion

        #region Fill

        internal Hashtable FillTemplateObjectsHashTable()
        {
            var dv = Page.UseMultiSiteLogic ? Cnn.GetAllTemplateObjects("") : Cnn.GetTemplateObjects("");

            var templateObjects = new Hashtable(dv.Count * 5);
            foreach (DataRowView drv in dv)
            {
                var url = Page.UseMultiSiteLogic ? Page.GetControlUrl(drv, Int32.Parse(drv["SITE_ID"].ToString())) : Page.GetControlUrl(drv);
                var tKey = drv["TEMPLATE_NAME"].ToString().ToLowerInvariant();
                var fKey = drv["FORMAT_NAME"].ToString().ToLowerInvariant();
                var oKey = drv["OBJECT_NAME"].ToString().ToLowerInvariant();
                string ofKey = $"{oKey}.{fKey}";
                string tofKey = $"{tKey}.{oKey}.{fKey}";
                string toKey = $"{tKey}.{oKey}";
                var isCurrentTemplate = Page.page_template_id.ToString() == drv["PAGE_TEMPLATE_ID"].ToString();
                var isDefaultFormat = drv["CURRENT_FORMAT_ID"].ToString() == drv["DEFAULT_FORMAT_ID"].ToString();
                if (Page.UseMultiSiteLogic)
                {
                    var id = drv["PAGE_TEMPLATE_ID"].ToString();
                    oKey = $"{id},{oKey}";
                    ofKey = $"{id},{ofKey}";
                    tofKey = $"{id},{tofKey}";
                    toKey = $"{id},{toKey}";
                    isCurrentTemplate = true;
                }
                if (!templateObjects.Contains(tofKey)) templateObjects.Add(tofKey, url);
                if (!templateObjects.Contains(ofKey) && isCurrentTemplate) templateObjects.Add(ofKey, url);
                if (!templateObjects.Contains(toKey) && isDefaultFormat) templateObjects.Add(toKey, url);

                if (!templateObjects.Contains(oKey) && isDefaultFormat && isCurrentTemplate) templateObjects.Add(oKey, url);
            }
            return templateObjects;
        }

        internal Hashtable FillPageObjectsHashTable()
        {
            var dv = Page.UseMultiSiteLogic ? Cnn.GetAllPageObjects("") : Cnn.GetPageObjects("");

            var pageObjects = new Hashtable(dv.Count * 2);
            foreach (DataRowView drv in dv)
            {

                var siteId = drv["SITE_ID"].ToString();


                var url = Page.UseMultiSiteLogic ? Page.GetControlUrl(drv, Int32.Parse(siteId)) : Page.GetControlUrl(drv);
                var oKey = drv["OBJECT_NAME"].ToString().ToLowerInvariant();
                string ofKey = $"{oKey}.{drv["FORMAT_NAME"].ToString().ToLowerInvariant()}";
                if (Page.UseMultiSiteLogic)
                {
                    var id = drv["PAGE_ID"].ToString();
                    oKey = $"{id},{oKey}";
                    ofKey = $"{id},{ofKey}";
                }
                var isDefaultFormat = drv["CURRENT_FORMAT_ID"].ToString() == drv["DEFAULT_FORMAT_ID"].ToString();
                if (!pageObjects.Contains(ofKey)) pageObjects.Add(ofKey, url);
                if (!pageObjects.Contains(oKey) && isDefaultFormat) pageObjects.Add(oKey, url);
            }
            return pageObjects;
        }

        private Hashtable FillTemplateHashTable()
        {
            var dv = Page.UseMultiSiteLogic ? Cnn.GetAllTemplates("") : Cnn.GetTemplates(""); 
            var templates = new Hashtable(dv.Count);
            foreach (DataRowView drv in dv)
            {
                var key = drv["TEMPLATE_NAME"].ToString().ToLowerInvariant();
                if (Page.UseMultiSiteLogic)
                {
                    var id = drv["SITE_ID"].ToString();
                    key = $"{id},{key}";
                }
                if (!templates.Contains(key))
                {
                    templates.Add(key, new Template { Id = DBConnector.GetNumInt(drv["PAGE_TEMPLATE_ID"]), Folder = drv["TEMPLATE_FOLDER"].ToString() });
                }
            }
            return templates;
        }

        private Hashtable FillPageHashTable()
        {
            var dv = Cnn.GetAllPages("");
            var allPages = new Hashtable(dv.Count);
            foreach (DataRowView drv in dv)
            {
                var key = drv["PAGE_ID"].ToString().ToLowerInvariant();
                if (!allPages.Contains(key))
                {
                    allPages.Add(key, drv["PAGE_FOLDER"].ToString());
                }
            }
            return allPages;
        }

        private Hashtable FillPageMapping()
        {
            var dv = Cnn.GetPageMapping("[PAGE_ID1] = " + Page.PageId);
            var pageMapping = new Hashtable(dv.Count);
            foreach (DataRowView drv in dv)
            {
                var key = Int32.Parse(drv["SITE_ID2"].ToString());
                if (!pageMapping.Contains(key))
                {
                    pageMapping.Add(key, DBConnector.GetNumInt(drv["PAGE_ID2"]));
                }
            }
            return pageMapping;
        }

        private Hashtable FillTemplateMapping()
        {
            var dv = Cnn.GetTemplateMapping("");
            var templateMapping = new Hashtable(dv.Count);
            foreach (DataRowView drv in dv)
            {
                string key = $"{drv["PAGE_TEMPLATE_ID1"]},{drv["SITE_ID2"]}";
                if (!templateMapping.Contains(key))
                {
                    templateMapping.Add(key, DBConnector.GetNumInt(drv["PAGE_TEMPLATE_ID2"]));
                }
            }
            return templateMapping;
        }

        private Hashtable FillStatusHashTable()
        {
            var weightSql = "SELECT MAX(WEIGHT) AS MAX_WEIGHT, SITE_ID FROM STATUS_TYPE WITH(NOLOCK) GROUP BY SITE_ID";
            string sql =
                $"WITH WEIGHTS AS({weightSql}) SELECT ST.SITE_ID, STATUS_TYPE_ID AS ID, STATUS_TYPE_NAME AS NAME FROM STATUS_TYPE ST WITH(NOLOCK) INNER JOIN WEIGHTS W ON ST.SITE_ID = W.SITE_ID AND ST.WEIGHT = W.MAX_WEIGHT";
            var dt = Cnn.GetRealData(sql);
            var statuses = new Hashtable(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                var key = DBConnector.GetNumInt(row["SITE_ID"]);
                if (!statuses.ContainsKey(key)) statuses.Add(key, new StatusType { Id = DBConnector.GetNumInt(row["ID"]), Name = row["NAME"].ToString() });
            }
            return statuses;
        }

        private Hashtable FillSiteIdHashTable()
        {
            var dt = Cnn.GetRealData("SELECT SITE_ID, SITE_NAME FROM SITE");
            var siteIds = new Hashtable(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                siteIds[row["SITE_NAME"].ToString().ToLowerInvariant()] = DBConnector.GetNumInt(row["SITE_ID"]);
            }
            return siteIds;
        }

        private Hashtable FillSiteHashTable()
        {
            var dt = Cnn.GetRealData("SELECT SITE_NAME, SITE_ID, DNS, STAGE_DNS, LIVE_DIRECTORY, STAGE_DIRECTORY, ASSEMBLY_PATH, STAGE_ASSEMBLY_PATH, UPLOAD_DIR, TEST_DIRECTORY, UPLOAD_URL, UPLOAD_URL_PREFIX, LIVE_VIRTUAL_ROOT, STAGE_VIRTUAL_ROOT, STAGE_EDIT_FIELD_BORDER, ASSEMBLE_FORMATS_IN_LIVE, USE_ABSOLUTE_UPLOAD_URL, ALLOW_USER_SESSIONS, IS_LIVE, SCRIPT_LANGUAGE, CONTEXT_CLASS_NAME, ENABLE_ONSCREEN FROM SITE");
            var sites = new Hashtable(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                var key = DBConnector.GetNumInt(row["SITE_ID"]);
                sites[key] = new Site
                {
                    Name = row["SITE_NAME"].ToString(),
                    Dns = row["DNS"].ToString(),
                    StageDns = row["STAGE_DNS"].ToString(),
                    LiveDirectory = row["LIVE_DIRECTORY"].ToString(),
                    StageDirectory = row["STAGE_DIRECTORY"].ToString(),
                    UploadDir = row["UPLOAD_DIR"].ToString(),
                    TestDirectory = row["TEST_DIRECTORY"].ToString(),
                    AssemblyDirectory = row["ASSEMBLY_PATH"].ToString(),
                    StageAssemblyDirectory = row["STAGE_ASSEMBLY_PATH"].ToString(),
                    UploadUrl = row["UPLOAD_URL"].ToString(),
                    UploadUrlPrefix = row["UPLOAD_URL_PREFIX"].ToString(),
                    LiveVirtualRoot = row["LIVE_VIRTUAL_ROOT"].ToString(),
                    StageVirtualRoot = row["STAGE_VIRTUAL_ROOT"].ToString(),
                    FieldBorderMode = DBConnector.GetNumInt(row["STAGE_EDIT_FIELD_BORDER"]),
                    AssembleFormatsInLive = (bool)row["ASSEMBLE_FORMATS_IN_LIVE"],
                    UseAbsoluteUploadUrl = DBConnector.GetNumBool(row["USE_ABSOLUTE_UPLOAD_URL"]),
                    AllowUserSessions = DBConnector.GetNumBool(row["ALLOW_USER_SESSIONS"]),
                    ContextClassName = DBConnector.GetString(row["CONTEXT_CLASS_NAME"], "QPDataContext"),
                    IsLive = row["IS_LIVE"].ToString() == "1",
                    ScriptLanguage = row["SCRIPT_LANGUAGE"].ToString(),
                    EnableOnScreen = (bool)row["ENABLE_ONSCREEN"]
                };
            }
            return sites;
        }

        private Hashtable FillHashTable(string key)
        {

            if (String.Equals(key, ContentIdForLinqHashKey))
                return new Hashtable();
            else if (String.Equals(key, StatusHashKey))
                return FillStatusHashTable();
            else if (String.Equals(key, SiteHashKey))
                return FillSiteHashTable();
            else if (String.Equals(key, SiteIdHashKey))
                return FillSiteIdHashTable();
            else if (String.Equals(key, PageHashKey))
                return FillPageHashTable();
            else if (String.Equals(key, TemplateHashKey))
                return FillTemplateHashTable();
            else if (String.Equals(key, TemplateMappingHashKey))
                return FillTemplateMapping();
            else if (String.Equals(key, LinkHashKey))
                return new Hashtable();
            else if (String.Equals(key, LinkForLinqHashKey))
                return new Hashtable();
            else if (String.Equals(key, AttributeIdForLinqHashKey))
                return new Hashtable();
            else if (String.Equals(key, ItemLinkHashKey))
                return new Hashtable();
            else if (String.Equals(key, ItemHashKey))
                return new Hashtable();
            else if (String.Equals(key, PageMappingHashKey))
                return FillPageMapping();
            else if (String.Equals(key, PageObjectHashKey))
                return FillPageObjectsHashTable();
            else if (String.Equals(key, TemplateObjectHashKey))
                return FillTemplateObjectsHashTable();
            else
                throw new Exception("Incorrect key for saved hashtable: " + key);
        }

        private DualHashTable FillDualHashTable(string key)
        {
            return new DualHashTable();
        }

        #endregion

        #endregion

        #region Generics

        public T GetCachedEntity<T>(string key, Func<T> fillAction) where T : class
        {
            return GetCachedEntity(key, GetInternalExpirationTime(key), fillAction);
        }

        public T GetCachedEntity<T>(string key, Func<string, T> fillAction) where T : class
        {
            return GetCachedEntity(key, GetInternalExpirationTime(key), fillAction);
        }

        public T GetCachedEntity<T>(string key, double cacheInterval, Func<string, T> fillAction) where T : class
        {
            T ht;
            if (!Cnn.CacheData)
            {
                ht = fillAction.Invoke(key);
            }
            else
            {
                var obj = GetDataFromCache(key);
                if (obj == NullObject)
                {
                    return null;
                }
                ht = (T)obj;
                if (ht == null)
                {
                    lock (GetLockObject(key))
                    {
                        ht = GetDataFromCache<T>(key);
                        if (ht == null)
                        {
                            ht = fillAction.Invoke(key);
                            AddEntityToCache(key, ht, cacheInterval, null);
                        }
                    }
                }
            }
            return ht;
        }

        public T GetCachedEntity<T>(string key, double cacheInterval, Func<T> fillAction) where T : class
        {
            T ht;
            if (!Cnn.CacheData)
            {
                ht = fillAction.Invoke();
            }
            else
            {
                var obj = GetDataFromCache(key);
                if (obj == NullObject)
                {
                    return null;
                }
                ht = (T)obj;
                if (ht == null)
                {
                    lock (GetLockObject(key))
                    {
                        ht = GetDataFromCache<T>(key);
                        if (ht == null)
                        {
                            ht = fillAction.Invoke();
                            AddEntityToCache(key, ht, cacheInterval, null);
                        }
                    }
                }
            }
            return ht;
        }

        #endregion

        #region IQueryObject

        internal DataTable GetQueryResult(IQueryObject obj, out long totalRecords)
        {
            QueryResult qr;
            if (!Cnn.CacheData || !obj.CacheResult)
            {
                qr = Cnn.GetFilledDataTable(obj);
            }
            else
            {
                var key = obj.GetKey(CacheKeyPrefix);
                qr = GetDataFromCache<QueryResult>(key);
                if (qr == null || obj.WithReset)
                {
                    lock (GetLockObject(key))
                    {
                        qr = GetDataFromCache<QueryResult>(key);
                        if (qr == null || obj.WithReset)
                        {
                            qr = Cnn.GetFilledDataTable(obj);
                            AddEntityToCache(key, qr, obj.CacheInterval, null);
                        }
                    }
                }
            }
            totalRecords = qr.TotalRecords;
            return qr.DataTable;
        }

        #endregion

        #endregion

        #region Expiration and limits

        private double GetInternalExpirationTime(string cacheKey)
        {

            if (cacheKey == ItemLinkHashKey) {
                return GetShortExpirationTime();
            }
            if (cacheKey == ItemHashKey)
            {
                return GetLongExpirationTime();
            }
            else
            {
                return GetExpirationTime();
            }
        }

        private double GetShortExpirationTime()
        {
            return GetExpirationTime("InternalShortExpirationTime", DefaultShortExpirationTime);
        }

        private double GetLongExpirationTime()
        {
            return GetExpirationTime("InternalLongExpirationTime", DefaultLongExpirationTime);
        }

        private double GetExpirationTime(string key = "InternalExpirationTime", double defaultValue = DefaultExpirationTime)
        {
            var expireInMinutes = DBConnector.AppSettings[key];
            double result;
            if (double.TryParse(expireInMinutes, out result))
            {
                if (result < MinExpirationTime) result = MinExpirationTime;
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        private int GetPrefetchLimit()
        {
            int result;
            var prefetchLimitString = DBConnector.AppSettings["PrefetchLimit"];
            if (int.TryParse(prefetchLimitString, out result))
            {
                if (result < 1) result = DefaultPrefetchLimit;
                return result;
            }
            else
            {
                return DefaultPrefetchLimit;
            }
        }

        #endregion

        #region Cache Keys

        #region prefixes

        public string CacheKeyPrefix => "QA.dll." + Cnn.InstanceCachePrefix;


        public string GetDataKeyPrefix => $"{CacheKeyPrefix}GetData.";

        public string FileContentsCacheKeyPrefix => CacheKeyPrefix + ".FileContents.";

        #endregion

        public string ConstraintKey => $"{CacheKeyPrefix}constraintList";

        public string StatusKey => $"{CacheKeyPrefix}statusList";

        private string GetPageObjectKey(Int32 pageId)
        {
            return $"{CacheKeyPrefix}pageObjects{pageId}";
        }

        public string PageObjectKey => GetPageObjectKey(Page?.page_id ?? 0);

        private string GetPageObjectHashKey(Int32 pageId)
        {
            return $"{CacheKeyPrefix}pageObjectsHash{pageId}";
        }

        public string PageObjectHashKey => GetPageObjectHashKey(Page?.page_id ?? 0);

        public string AllPageObjectsKey => $"{CacheKeyPrefix}allPageObjects";

        public string AllPageObjectsHashKey => $"{CacheKeyPrefix}allPageObjectsHash";

        public string AllTemplateObjectsKey => $"{CacheKeyPrefix}allTemplateObjects";

        public string AllTemplateObjectsHashKey => $"{CacheKeyPrefix}allTemplateObjectsHash";

        public string TemplateObjectKey => $"{CacheKeyPrefix}templateObjects";

        public string TemplateObjectHashKey => GetTemplateObjectHashKey(Page?.page_template_id ?? 0);

        private string GetTemplateObjectHashKey(Int32 pageTemplateId)
        {
            return $"{CacheKeyPrefix}templateObjectsHash{pageTemplateId}";
        }
        
        public string TemplateKey => $"{CacheKeyPrefix}templates";

        public string TemplateHashKey => $"{CacheKeyPrefix}templatesHash";

        public string AllTemplatesKey => $"{CacheKeyPrefix}allTemplates";

        public string AllTemplatesHashKey => $"{CacheKeyPrefix}allTemplatesHash";

        public string PageKey => $"{CacheKeyPrefix}pages";

        public string PageHashKey => $"{CacheKeyPrefix}pagesHash";

        public string AllPagesKey => $"{CacheKeyPrefix}allPages";

        public string AllPagesHashKey => $"{CacheKeyPrefix}allPagesHash";

        public string TemplateMappingKey => $"{CacheKeyPrefix}templateMapping";

        public string TemplateMappingHashKey => $"{CacheKeyPrefix}templateMappingHash";

        public string PageMappingKey => $"{CacheKeyPrefix}pageMapping";

        public string PageMappingHashKey => GetPageMappingHashKey(Page?.PageId ?? 0);

        private string GetPageMappingHashKey(Int32 pageId)
        {
            return $"{CacheKeyPrefix}pageMappingHash{pageId}";
        }

        public string ContentHashKey => $"{CacheKeyPrefix}contentHash";

        public string ContentIdForLinqHashKey => $"{CacheKeyPrefix}contentIdForLinqHash";

        public string LinkHashKey => $"{CacheKeyPrefix}linkHash";

        public string LinkForLinqHashKey => $"{CacheKeyPrefix}linkForLinqHash";

        public string ItemLinkHashKey => $"{CacheKeyPrefix}itemLinkHash";

        public string GetItemLinkElementHashKey(int linkId, string itemId, bool isManyToMany)
        {
            var hashKeyPart = isManyToMany ? "link" : "attribute";
            return $"item{itemId}{hashKeyPart}{linkId}";
        }

        public string ItemHashKey => $"{CacheKeyPrefix}itemHash";

        public string StatusHashKey => $"{CacheKeyPrefix}statusHash";

        public string SiteHashKey => $"{CacheKeyPrefix}siteHash";

        public string SiteIdHashKey => $"{CacheKeyPrefix}siteIdHash";

        public string AttributeHashKey => $"{CacheKeyPrefix}attributeHash";

        public string AttributeIdForLinqHashKey => $"{CacheKeyPrefix}attributeIdForLinqHash";

        #endregion

        #region Cache files

        private string GetCacheFilePath(string cacheKey)
        {
            if (cacheKey == PageObjectKey || cacheKey == PageObjectHashKey)
            {
                return PageObjectCacheFile;
            }
            else if (cacheKey == TemplateObjectKey || cacheKey == TemplateObjectHashKey)
            {
                return TemplateObjectCacheFile;
            }
            else if (cacheKey == AllPageObjectsKey || cacheKey == AllPageObjectsHashKey)
            {
                return AllPageObjectsCacheFile;
            }
            else if (cacheKey == AllTemplateObjectsKey || cacheKey == AllTemplateObjectsHashKey)
            {
                return AllTemplateObjectsCacheFile;
            }
            else if (cacheKey == AllTemplatesKey || cacheKey == AllPagesKey || cacheKey == AllTemplatesHashKey || cacheKey == AllPagesHashKey || cacheKey == TemplateMappingKey || cacheKey == PageMappingKey || cacheKey == TemplateMappingHashKey || cacheKey == PageMappingHashKey)
            {
                return AllStructureCacheFile;
            }
            else if (cacheKey == TemplateKey || cacheKey == PageKey || cacheKey == PageHashKey || cacheKey == TemplateHashKey)
            {
                return StructureCacheFile;
            }
            else if (cacheKey.Contains(FileContentsCacheKeyPrefix))
                return cacheKey.Replace(FileContentsCacheKeyPrefix, String.Empty);
            else
            {
                return "";
            }
        }

        public string CacheFilePath => $"{Cnn.GetSiteDirectory(Page.site_id, !Page.IsStage, Page.IsTest)}\\dependencies";

        public string AllTemplateObjectsCacheFile => $"{CacheFilePath}\\\\{"all_templates.dep"}";

        public string AllTemplateObjectsCacheDataFile => $"{CacheFilePath}\\\\{"all_templates.dat"}";

        public string AllPageObjectsCacheFile => $"{CacheFilePath}\\\\{"all_pages.dep"}";

        public string TemplateObjectCacheFile => $"{CacheFilePath}\\\\{"templates.dep"}";

        public string PageObjectCacheFile => $"{CacheFilePath}\\\\{Page.page_id}{".dep"}";

        public string StructureCacheFile => $"{CacheFilePath}\\\\{"structure.dep"}";

        public string AllStructureCacheFile => $"{CacheFilePath}\\\\{"all_structure.dep"}";

        #endregion

        #region SetWebSpecificInformation

        public void SetWebSpecificInformation(QPageEssential page)
        {
            Page = page;
            _queries.Add(TemplateKey,
                $"EXEC sp_executesql N'{GetPageTemplateQuery()} WHERE PT.SITE_ID = @siteId', N'@siteId NUMERIC', @siteId = {page.site_id}");
            _queries.Add(PageKey,
                $"EXEC sp_executesql N'{GetPageQuery()} WHERE PT.SITE_ID = @siteId', N'@siteId NUMERIC', @siteId = {page.site_id}");

            _fieldsToValidate.Add(TemplateKey, new[] { "siteId" });
            _fieldsToValidate.Add(PageKey, new[] { "page_name" });

            if (!page.UseMultiSiteLogic)
            {
                _queries.Add(PageObjectKey,
                    $"EXEC sp_executesql N'{GetBaseObjectsQuery()} WHERE PT.SITE_ID = @siteId AND OBJ.PAGE_ID = @pageId', N'@siteId NUMERIC, @pageId NUMERIC', @siteId = {page.site_id}, @pageId = {page.page_id}");
                _queries.Add(TemplateObjectKey,
                    $"EXEC sp_executesql N'{GetBaseObjectsQuery()} WHERE PT.SITE_ID = @siteId AND OBJ.PAGE_ID IS NULL', N'@siteId NUMERIC', @siteId = {page.site_id}");

                _fieldsToValidate.Add(PageObjectKey, new[] { "siteId" });
                _fieldsToValidate.Add(TemplateObjectKey, new[] { "siteId" });
            }
            else
            {
                _queries.Add(AllPageObjectsKey, $"{GetBaseObjectsQuery()} WHERE OBJ.PAGE_ID IS NOT NULL ");
                _queries.Add(AllTemplateObjectsKey, $"{GetBaseObjectsQuery()} WHERE OBJ.PAGE_ID IS NULL ");
                _queries.Add(AllTemplatesKey, GetPageTemplateQuery());
                _queries.Add(AllPagesKey, GetPageQuery());
                _queries.Add(TemplateMappingKey, "SELECT PT1.TEMPLATE_NAME, PT1.PAGE_TEMPLATE_ID AS PAGE_TEMPLATE_ID1, PT1.SITE_ID AS SITE_ID1, PT2.PAGE_TEMPLATE_ID AS PAGE_TEMPLATE_ID2, PT2.SITE_ID AS SITE_ID2 FROM PAGE_TEMPLATE PT1 INNER JOIN PAGE_TEMPLATE PT2 ON PT1.TEMPLATE_NAME = PT2.TEMPLATE_NAME AND PT1.PAGE_TEMPLATE_ID <> PT2.PAGE_TEMPLATE_ID AND PT1.TEMPLATE_NAME <> 'Default Notification Template'");
                _queries.Add(PageMappingKey, "SELECT P1.PAGE_NAME, PT1.TEMPLATE_NAME, P1.PAGE_ID AS PAGE_ID1, P2.PAGE_ID AS PAGE_ID2, PT1.PAGE_TEMPLATE_ID AS PAGE_TEMPLATE_ID1, PT2.PAGE_TEMPLATE_ID AS PAGE_TEMPLATE_ID2, PT1.SITE_ID AS SITE_ID1, PT2.SITE_ID AS SITE_ID2 FROM PAGE P1 INNER JOIN PAGE_TEMPLATE PT1 ON P1.PAGE_TEMPLATE_ID = PT1.PAGE_TEMPLATE_ID INNER JOIN PAGE P2 ON P1.PAGE_NAME = P2.PAGE_NAME AND P1.PAGE_ID <> P2.PAGE_ID INNER JOIN PAGE_TEMPLATE PT2 ON PT1.TEMPLATE_NAME = PT2.TEMPLATE_NAME AND P2.PAGE_TEMPLATE_ID = PT2.PAGE_TEMPLATE_ID AND PT1.SITE_ID <> PT2.SITE_ID WHERE PT1.TEMPLATE_NAME <> 'Default Notification Template'");

                _fieldsToValidate.Add(AllPageObjectsKey, new[] { "siteId" });
                _fieldsToValidate.Add(AllTemplateObjectsKey, new[] { "siteId" });
                _fieldsToValidate.Add(TemplateMappingKey, new[] { "template_name" });
                _fieldsToValidate.Add(PageMappingKey, new[] { "template_name" });
                _fieldsToValidate.Add(AllTemplatesKey, new[] { "siteId" });

                _fieldsToValidate.Add(AllPagesKey, new[] { "page_name" });
            }
        }

        #endregion

    }
}
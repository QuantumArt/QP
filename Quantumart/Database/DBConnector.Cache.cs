using System;
using System.Collections;
using System.Data;
using System.Text;

namespace Quantumart.QPublishing.Database
{
    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {
        public DataTable GetDataTable(string key)
        {
            return CacheManager.GetDataTable(key);
        }

        #region DataViews
        public DataView GetDataView(string key, string rowFilter)
        {
            return CacheManager.GetDataView(key, rowFilter);
        }

        internal DataView GetConstraints(string rowFilter)
        {
            return GetDataView(CacheManager.ConstraintKey, rowFilter);
        }

        internal DataView GetStatuses(string rowFilter)
        {
            return GetDataView(CacheManager.StatusKey, rowFilter);
        }

        internal DataView GetTemplates(string rowFilter)
        {
            return GetDataView(CacheManager.TemplateKey, rowFilter);
        }

        internal DataView GetAllTemplates(string rowFilter)
        {
            return GetDataView(CacheManager.AllTemplatesKey, rowFilter);
        }

        internal DataView GetPages(string rowFilter)
        {
            return GetDataView(CacheManager.PageKey, rowFilter);
        }

        internal DataView GetAllPages(string rowFilter)
        {
            return GetDataView(CacheManager.AllPagesKey, rowFilter);
        }

        internal DataView GetAllTemplateObjects(string rowFilter)
        {
            return GetDataView(CacheManager.AllTemplateObjectsKey, rowFilter);
        }

        internal DataView GetAllTemplateObjects(string rowFilter, int pageTemplateId)
        {
            return GetAllTemplateObjects(AppendFilter(rowFilter, "PAGE_TEMPLATE_ID", pageTemplateId));
        }

        internal DataView GetAllPageObjects(string rowFilter, int pageId)
        {
            return GetAllPageObjects(AppendFilter(rowFilter, "PAGE_ID", pageId));
        }

        internal DataView GetAllPageObjects(string rowFilter)
        {
            return GetDataView(CacheManager.AllPageObjectsKey, rowFilter);
        }

        internal DataView GetTemplateObjects(string rowFilter)
        {
            return GetDataView(CacheManager.TemplateObjectKey, rowFilter);
        }

        internal DataView GetPageObjects(string rowFilter)
        {
            return GetDataView(CacheManager.PageObjectKey, rowFilter);
        }

        internal DataView GetTemplateMapping(string rowFilter)
        {
            return GetDataView(CacheManager.TemplateMappingKey, rowFilter);
        }

        internal DataView GetPageMapping(string rowFilter)
        {
            return GetDataView(CacheManager.PageMappingKey, rowFilter);
        }

        private static string AppendFilter(string rowFilter, string key, int value)
        {
            var sb = new StringBuilder(rowFilter);
            if (!string.IsNullOrEmpty(rowFilter))
            {
                sb.Append(" AND ");
            }

            sb.AppendFormat("{0} = {1}", key, value);
            return sb.ToString();
        }
        #endregion

        #region Hashtables
        internal Hashtable GetContentHashTable()
        {
            return CacheManager.GetCachedDualHashTable(CacheManager.ContentHashKey).Items;
        }

        internal Hashtable GetContentIdHashTable()
        {
            return CacheManager.GetCachedDualHashTable(CacheManager.ContentHashKey).Ids;
        }

        internal Hashtable GetContentIdForLinqHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.ContentIdForLinqHashKey);
        }

        internal Hashtable GetTemplateHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.TemplateHashKey);
        }

        internal Hashtable GetPageHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.PageHashKey);
        }

        internal Hashtable GetTemplateMappingHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.TemplateMappingHashKey);
        }

        internal Hashtable GetPageMappingHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.PageMappingHashKey);
        }

        internal Hashtable GetTemplateObjectHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.TemplateObjectHashKey);
        }

        internal Hashtable GetPageObjectHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.PageObjectHashKey);
        }

        internal Hashtable GetLinkHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.LinkHashKey);
        }

        internal Hashtable GetLinkForLinqHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.LinkForLinqHashKey);
        }

        internal Hashtable GetItemLinkHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.ItemLinkHashKey);
        }

        internal Hashtable GetItemHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.ItemHashKey);
        }

        internal Hashtable GetStatusHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.StatusHashKey);
        }

        internal Hashtable GetSiteHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.SiteHashKey);
        }

        internal Hashtable GetSiteIdHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.SiteIdHashKey);
        }

        internal Hashtable GetAttributeHashTable()
        {
            return CacheManager.GetCachedDualHashTable(CacheManager.AttributeHashKey).Items;
        }

        internal Hashtable GetAttributeIdHashTable()
        {
            return CacheManager.GetCachedDualHashTable(CacheManager.AttributeHashKey).Ids;
        }

        internal Hashtable GetAttributeIdForLinqHashTable()
        {
            return CacheManager.GetCachedHashTable(CacheManager.AttributeIdForLinqHashKey);
        }
        #endregion

        public T GetCachedEntity<T>(string key, Func<string, T> fillAction) where T : class
        {
            return CacheManager.GetCachedEntity(key, fillAction);
        }

        public T GetCachedEntity<T>(string key, double interval, Func<string, T> fillAction) where T : class
        {
            return CacheManager.GetCachedEntity(key, interval, fillAction);
        }

        public T GetCachedEntity<T>(string key, Func<T> fillAction) where T : class
        {
            return CacheManager.GetCachedEntity(key, fillAction);
        }

        public T GetCachedEntity<T>(string key, double interval, Func<T> fillAction) where T : class
        {
            return CacheManager.GetCachedEntity(key, interval, fillAction);
        }
    }
}

using System;
using System.Collections;
using System.Data;
using System.Web;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Info;
using Quantumart.QPublishing.Pages;

namespace Quantumart.QPublishing.Controls
{
    public class QPublishControlEssential
    {
        private string _contentName;
        private string _contentUploadUrl;

        private Hashtable _fieldInfoDictionary;
        private readonly IQUserControl _currentControl;

        public QPublishControlEssential(IQUserControl control)
        {
            _currentControl = control;
        }

        public IQPage Page => _currentControl.QPage;

        public DataTable Data { get; set; }

        public long TotalRecords { get; set; }

        public long AbsoluteTotalRecords { get; set; }

        public long RecordsPerPage { get; set; }

        public string DynamicVariable { get; set; } = string.Empty;

        public bool RotateContent { get; set; }

        public bool IsRoot { get; set; }

        public bool ForceUnited { get; set; } = false;

        public bool UseSchedule { get; set; } = true;

        public bool ShowArchive { get; set; } = false;

        public bool UseSecurity { get; set; } = false;

        public bool UseLevelFiltration { get; set; } = false;

        public string StartLevel { get; set; } = "0";

        public string EndLevel { get; set; } = "0";

        public string StartRow { get; set; } = "1";

        public string PageSize { get; set; } = "0";

        public string CustomFilter { get; set; }

        public string DynamicOrder { get; set; }

        public string StaticOrder { get; set; } = "c.content_item_id asc";

        public string Statuses { get; set; } = "'Published'";

        public int ContentId { get; set; }

        public int ExtSiteId { get; set; }

        public int Duration { get; set; } = 0;

        public bool EnableCaching => Duration > 0;

        public bool EnableCacheInvalidation { get; set; } = false;

        public string ContentName
        {
            get
            {
                if (string.IsNullOrEmpty(_contentName))
                {
                    _contentName = Page.Cnn.GetContentName(Convert.ToInt32(ContentId));
                }
                return _contentName;
            }
            set { _contentName = value; }
        }

        public new Hashtable FieldInfoDictionary
        {
            get { return _fieldInfoDictionary ?? (_fieldInfoDictionary = new Hashtable()); }
            set { _fieldInfoDictionary = value; }
        }

        public string ContentUploadUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_contentUploadUrl))
                {
                    _contentUploadUrl = Page.Cnn.GetContentUploadUrlByID(ExtSiteId, ContentId);
                }
                return _contentUploadUrl;
            }
            set { _contentUploadUrl = value; }
        }

        public string CacheKey { get; set; }

        private string Select
        {
            get
            {
                var select = "c.*";
                select = UseSecurity && !UseLevelFiltration ?
                    $"{select}, IsNull(pi.permission_level,0) as current_permission_level "
                    : select;
                return select;
            }
        }

        private string From
        {
            get
            {
                string from =
                    $" content_{ContentId}{(Page.IsStage || ForceUnited ? "_UNITED" : string.Empty)} AS c WITH (NOLOCK)";
                if (UseSecurity)
                {
                    from = UseLevelFiltration ?
                        $"{from} inner join ({MagicString}) as pi on c.content_item_id = pi.content_item_id"
                        : $"{from} left outer join ({MagicString}) as pi on c.content_item_id = pi.content_item_id";
                }
                return from;
            }
        }

        private string Where
        {
            get
            {
                var visibleFilter = UseSchedule ? "c.visible = 1" : "1 = 1";
                var archiveFilter = ShowArchive ? string.Empty : "and c.archive = 0";
                var statusFilter = Page.IsStage ? string.Empty :
                    $"AND c.status_type_id in (select status_type_id from status_type where status_type_name in ({Statuses}))";
                if (IsRoot)
                {
                    return !string.IsNullOrEmpty(CustomFilter) ? CustomFilter :
                        $"c.content_item_id = {Page.NumValue("id")}";
                }

                return
                    $" {visibleFilter} {archiveFilter} {statusFilter} {QPageEssential.GetSimpleContainerFilterExpression(CustomFilter)} ";
            }
        }

        private string OrderBy => QPageEssential.GetSimpleContainerOrderExpression(StaticOrder, DynamicOrder);

        private void DetermineExtSiteId()
        {
            var extSiteId = Page.site_id;
            if (!string.IsNullOrEmpty(DynamicVariable))
            {
                ContentId = Page.Cnn.GetDynamicContentId(Page.InternalStrValue(DynamicVariable), ContentId, Page.site_id, ref extSiteId);
            }
            ExtSiteId = extSiteId;
        }

        public string GetBackendUrlForNotification(string defaultBackendUrl)
        {
            var backendUrl = DBConnector.AppSettings["BackendUrlForNotification"] ?? "http://" + defaultBackendUrl;
            return backendUrl;
        }

        public string GetFieldUploadUrl(string fieldName)
        {
            return _currentControl.QPage.GetFieldUploadUrl(fieldName, ContentId);
        }

        // ReSharper disable once RedundantAssignment
        protected long[] GetRandomIds(string from, string where, int count, ref long totalCount)
        {
            var ids = GetIds(from, where);

            if (count > ids.Length || count == 0)
            {
                count = ids.Length;
            }
            totalCount = ids.Length;

            var rnd = new Random(DateTime.Now.Millisecond);
            var result = new long[count];

            for (var i = 0; i <= count - 1; i++)
            {
                var j = rnd.Next(i, ids.Length);
                if (j == ids.Length)
                {
                    j -= 1;
                }
                var temp = ids[i];
                ids[i] = ids[j];
                ids[j] = temp;
                result[i] = ids[i];
            }

            return result;
        }

        protected long[] GetIds(string from, string where)
        {
            // ReSharper disable once PossibleLossOfFraction
            var dt = Page.Cnn.GetCachedData($"select content_item_id as id from {from} where {where}", Duration / 60);
            var result = new long[dt.Rows.Count];
            for (var i = 0; i <= dt.Rows.Count - 1; i++)
            {
                result[i] = long.Parse(dt.Rows[i]["id"].ToString());
            }

            return result;
        }

        private static string MagicString => "<$_security_insert_$>";

        public void FillData()
        {
            DetermineExtSiteId();
            FillDataTable(Select, From, Where, OrderBy);
        }

        private static string GetSessionVariable(string name, string defaultValue)
        {
            var result = defaultValue;
            if (HttpContext.Current.Session != null)
            {
                var obj = HttpContext.Current.Session[name];
                if (obj != null)
                {
                    return obj.ToString();
                }
            }
            return result;
        }

        public void FillDataTable(string select, string from, string where, string orderBy)
        {
            long totalRecords = 0;
            if (string.IsNullOrEmpty(select))
            {
                select = " c.* ";
            }
            var uid = int.Parse(GetSessionVariable("@qp_UID", "0"));
            var gid = int.Parse(GetSessionVariable("@qp_GID", "0"));
            var queryObj = new ContainerQueryObject(Page.Cnn, select, from, where, orderBy, StartRow, PageSize, !RotateContent && PageSize != "0", UseSecurity, Duration, StartLevel, EndLevel, ContentId, uid, gid);
            if (!RotateContent)
            {
                Data = Page.Cnn.GetContainerQueryResultTable(queryObj, out totalRecords);
                AbsoluteTotalRecords = totalRecords;
            }
            else
            {
                var ids = GetRandomIds(from, where, int.Parse(PageSize), ref totalRecords);
                AbsoluteTotalRecords = totalRecords;
                queryObj.OrderBy = " ";
                queryObj.StartRow = "1";
                queryObj.PageSize = "1";
                if (ids.Length == 0)
                {
                    queryObj.Where = "c.content_item_id = 0";
                    Data = Page.Cnn.GetContainerQueryResultTable(queryObj, out totalRecords);
                }
                for (var i = 0; i <= ids.Length - 1; i++)
                {
                    queryObj.Where = $"c.content_item_id = {ids[i]}";
                    var dt = Page.Cnn.GetContainerQueryResultTable(queryObj, out totalRecords);
                    if (Data == null)
                    {
                        Data = dt;
                    }
                    else
                    {
                        Data.Merge(dt);
                    }
                }
            }
            TotalRecords = Data.Rows.Count;
            RecordsPerPage = int.Parse(PageSize);
        }
    }
}

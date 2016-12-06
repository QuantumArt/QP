using System;
using System.Data;
using System.Globalization;
using System.Text;
using Quantumart.QP8.Assembling;

namespace Quantumart.QP8.Assembling.Info
{
    public class ContainerInfo
    {
        public ContainerInfo(ControlInfo control)
        {
            Control = control;
            if (Control.ContentSelected)
            {
                Statuses = GetStatuses();
                Workflow = GetWorkflow();
                MaxStatusId = GetMaxStatus();

            }
        }

        public int ContentId()
        {
            if (Control.ContentSelected)
            {
                return Control.GetInt32("CONTENT_ID");
            }
            else
            {
                throw new DataException(Control.MissedContentExceptionString);
            }
        }



        ~ContainerInfo()
        {
            Statuses?.Dispose();
            Workflow?.Dispose();
        }

        public AssembleInfo Info => Control.Info;

        public int MaxStatusId { get; }

        private int GetMaxStatus()
        {
            int maxStatusId;
            if (GetWorkflowField("CONTENT_ID") != 0 && Statuses.Count > 0)
            {
                maxStatusId = GetWorkflowField("STATUS_TYPE_ID");
            }
            else if (Control.GetInt32("CONTAINER_SITE_ID") != Info.SiteId)
            {
                maxStatusId = Info.GetMaxStatus(Control.GetInt32("CONTAINER_SITE_ID")).Id;
            }
            else
            {
                maxStatusId = Info.GetMaxStatus().Id;
            }
            return maxStatusId;
        }

        private bool ContainsMaxStatus => Statuses.Count == 0 || StatusExists(MaxStatusId);

        private ControlInfo Control { get; }


        public bool IsRandom => String.Equals(Control.GetString("ROTATE_CONTENT"), "1");

        public bool IsNewAssembling => true;


        public bool AllowOrderDynamic => Control.GetObject("ALLOW_ORDER_DYNAMIC") != DBNull.Value && Control.GetNumericBoolean("ALLOW_ORDER_DYNAMIC");

        private string VisibleFilter => UseSchedule ? "\" c.visible = 1 \"" : "\"c.visible IS NOT NULL\"";

        public bool UseSchedule => Control.GetInt32("SCHEDULE_DEPENDENCE") == 1;

        private string ArchiveFilter => ShowArchive ? "\"\"" : "\"and c.archive = 0\"";

        public bool ShowArchive => Control.GetInt32("SHOW_ARCHIVED") == 1;

        public string CustomFilter
        {
            get
            {
                var value = Control.GetString("FILTER_VALUE").Trim();
                if (!AssembleRootObject)
                    return value;
                else if (Info.Mode == AssembleMode.PreviewById || Info.Mode == AssembleMode.Notification) // will be replaced in the runtime
                    return String.Empty;
                else if (Info.Mode == AssembleMode.PreviewAll || String.IsNullOrEmpty(value)) // will remain in the runtime
                    return @"""1 = 1""";
                else
                    return value;
            }
        }

        private string MainFilter
        {
            get
            {
                var filter = CustomFilter;
                if (String.IsNullOrEmpty(filter))
                {
                    filter = "\"\"";
                }
                return "GetContainerFilterExpression(" + filter + ")";
            }
        }

        public static string SelectClause => "\" c.* \"";

        public string FromClause => "\" " + SqlSource + " AS c WITH (NOLOCK) \"";

        public string WhereClause(bool isCSharp)
        {
            var concat = AssembleControllerBase.ConcatChar(isCSharp);
            if (Info.Mode == AssembleMode.PreviewAll && AssembleRootObject)
            {
                return "\"1=1\"";
            }
            else if ((Info.Mode == AssembleMode.PreviewById || Info.Mode == AssembleMode.Notification) && AssembleRootObject)
            {
                return String.Format(CultureInfo.InvariantCulture, "\" c.content_item_id=\" {0} NumValue(\"id\")", concat);
            }
            else
            {
                return String.Format(CultureInfo.InvariantCulture, "String.Format({0}, {1}, {2}, {3}, {4})", "\" {0} {1} {2} {3} \"", VisibleFilter, ArchiveFilter, StatusFilter, MainFilter);
            }
        }

        public string DynamicOrder
        {
            get
            {
                var orderDynamic = Control.GetString("ORDER_DYNAMIC").Trim();
                return AllowOrderDynamic && !String.IsNullOrEmpty(orderDynamic) ? orderDynamic : "\"\"";
            }
        }

        public string StaticOrder => Control.GetString("ORDER_STATIC");

        public string OrderClause
        {
            get
            {
                if (IsRandom)
                {

                    return IsNewAssembling ? "\"\"" : "\"NewId()\"";
                }
                else
                {

                    return String.Format(CultureInfo.InvariantCulture, "GetContainerOrderExpression(\"{0}\", {1})", StaticOrder, DynamicOrder);
                }
            }
        }

        public string StartRow
        {
            get
            {
                if (String.IsNullOrEmpty(Control.GetString("SELECT_START")) || AssembleRootObjectInFormatMode || IsRandom)
                {
                    return "1";
                }
                else
                {
                    return Control.GetString("SELECT_START");
                }
            }
        }

        public string PageSize
        {
            get
            {
                if (AssembleRootObjectInFormatMode || String.IsNullOrEmpty(Control.GetString("SELECT_TOTAL")))
                {
                    return "0";
                }
                else
                {
                    return Control.GetString("SELECT_TOTAL");
                }
            }
        }

        public string GetCountWithoutPaging => PageSize == "0" ? "0" : "1";

        public bool WorkflowAssigned => Workflow.Count > 0;

        private string StatusFilter => !Info.IsAssembleFormatMode && Info.IsLive
            ? $"\"AND c.status_type_id in ({StatusQueryString})\""
            : "\"\"";

        public bool AssembleRootObjectInFormatMode => Info.IsAssembleFormatMode && AssembleRootObject;

        public bool AssembleRootObject => Info.GetString("OBJECT_ID") == Control.GetString("OBJECT_ID");


        public static string SecurityMagicString => "<$_security_insert_$>";


        public bool ApplySecurity => Control.GetNumericBoolean("APPLY_SECURITY");

        public string StartLevel
        {
            get
            {
                if (ApplySecurity)
                {
                    return (string)Info.PermissionLevels[Control.GetString("START_LEVEL")];
                }
                else
                {
                    return "0";
                }
            }
        }

        public string EndLevel
        {
            get
            {
                if (ApplySecurity)
                {
                    return (string)Info.PermissionLevels[Control.GetString("END_LEVEL")];
                }
                else
                {
                    return "0";
                }
            }
        }

        public bool UseLevelFiltration
        {
            get
            {
                if (ApplySecurity)
                {
                    return Control.GetBoolean("USE_LEVEL_FILTRATION");
                }
                else
                {
                    return false;
                }
            }
        }

        public int Duration
        {
            get
            {
                if (!Info.IsLive || String.IsNullOrEmpty(Control.GetString("DURATION")))
                {
                    return 0;
                }
                else
                {
                    return Control.GetInt32("DURATION") * 60;
                }
            }
        }

        public bool EnableCacheInvalidation => Control.GetBoolean("ENABLE_CACHE_INVALIDATION");


        public DataView Statuses { get; }

        private DataView GetStatuses()
        {
            var dv = new DataView(Info.ContainerStatuses)
            {
                RowFilter = "OBJECT_ID = " + Control.GetInt32("OBJECT_ID")
            };
            return dv;
        }

        private string GetStatusName(string statusId)
        {
            var dv = new DataView(Info.Statuses) {RowFilter = "STATUS_TYPE_ID = " + statusId};
            if (dv.Count > 0)
                return dv[0]["STATUS_TYPE_NAME"].ToString();
            else
                return String.Empty;
        }

        public bool StatusExists(int statusId)
        {
            for (var i = 0; i < Statuses.Count; i++)
            {
                if (Convert.ToInt32((decimal)Statuses[i]["STATUS_TYPE_ID"], CultureInfo.InvariantCulture) == statusId)
                {
                    return true;
                }
            }
            return false;
        }

        public string StatusName(int statusId)
        {
            var dv = new DataView(Info.Statuses) {RowFilter = "status_type_id = " + statusId};
            return dv.Count == 0 ? String.Empty : dv[0]["STATUS_TYPE_NAME"].ToString();
        }

        private string StatusQueryString =>
            $"select status_type_id from status_type where status_type_name in ({StatusString})";

        public string StatusString
        {
            get
            {

                var sb = new StringBuilder();
                if (WorkflowAssigned && Statuses.Count > 0)
                {
                    for (var i = 0; i < Statuses.Count; i++)
                    {
                        if (i != 0) { sb.Append(", "); }
                        sb.AppendFormat("'{0}'", GetStatusName(Statuses[i]["STATUS_TYPE_ID"].ToString()));
                    }
                }
                else
                {
                    sb.AppendFormat("'{0}'", StatusName(MaxStatusId));
                }
                return sb.ToString();
            }
        }

        public DataView Workflow { get; }

        public bool ForceUnited => !ContainsMaxStatus || AssembleRootObjectInFormatMode;

        private string SqlSource
        {
            get
            {
                if (Info.IsLive && !ForceUnited)
                {
                    return "CONTENT_" + ContentId();
                }
                else
                {
                    return "CONTENT_" + ContentId() + "_UNITED";
                }
            }
        }



        private DataView GetWorkflow()
        {
            return new DataView(Info.Workflow) {RowFilter = "CONTENT_ID = " + ContentId()};
        }

        private int GetWorkflowField(string fieldName)
        {
            if (WorkflowAssigned)
            {
                return Convert.ToInt32((decimal)Workflow[0][fieldName]);
            }
            else
            {
                return 0;
            }
        }

    }

}

using System;
using System.Data;
using System.Text;

namespace Assembling.Info
{

    public class ControlSetInfo
    {

        public ControlSetInfo(AssembleInfo info)
        {
            FillControlSet(info, true);
        }
        public ControlSetInfo(AssembleInfo info, bool isDbConnected)
        {
            FillControlSet(info, isDbConnected);
        }

        private void FillControlSet(AssembleInfo info, bool isDbConnected)
        {
            Info = info;
            InitBaseFormatSql();
            if (isDbConnected)
            {
                Data = Info.IsAssembleObjectsMode ? Info.Controller.Cnn.GetDataTable(SiteObjectsSql + Info.Controller.GetFilter()) : Info.Controller.Cnn.GetDataTable(SiteObjectsSql + " and objf.object_format_id = 0");

                if (Info.Mode == AssembleMode.Page || Info.Mode == AssembleMode.AllTemplateObjects || Info.IsAssembleFormatMode)
                {
                    FillTemplateRow();
                }
            }
            RowIndex = 0;
        }

        public void FillRowSpecificInfo()
        {
            Current = new ControlInfo(Reader, Info);
        }

        public ControlInfo Current { get; set; }


        public AssembleInfo Info { get; private set; }

        public DataTable Data { get; set; }

        public DataTable Statuses { get; set; }

        public DataTable ObjectValues { get; set; }

        public DataTable ThankYouPages { get; set; }

        public int RowIndex { get; set; }

        public int Count => Data.Rows.Count;

        public DataRow Reader => Data.Rows[RowIndex];

        private void InitBaseFormatSql()
        {
            var sb = new StringBuilder();
            sb.Append("select objf.object_format_id as current_format_id");
            sb.Append(", objf.format_name, objf.net_format_name, objf.format_body, objf.code_behind, objf.net_language_id, objf.assemble_in_live, objf.assemble_in_stage, objf.assemble_notification_in_live, objf.assemble_notification_in_stage, objf.assemble_preview_in_live, objf.assemble_preview_in_stage");
            sb.Append(", obj.object_id, obj.object_format_id as default_format_id, obj.object_type_id, obj.use_default_values, obj.object_name, obj.net_object_name, obj.enable_viewstate, obj.disable_databind, 0 as root, obj.page_template_id, obj.page_id, obj.control_custom_class");
            sb.Append(", pt.page_template_id, pt.template_name, pt.template_custom_class, pt.custom_class_for_generics, pt.custom_class_for_containers, pt.custom_class_for_forms");
            sb.Append(", c.content_id, c.content_name, c.site_id as container_site_id, c.virtual_type, cf.net_language_id as form_language_id");
            sb.Append(", cnt.allow_order_dynamic, cnt.order_dynamic, cnt.order_static, cnt.filter_value, cnt.select_total, cnt.select_start, cnt.schedule_dependence, cnt.rotate_content, cnt.apply_security, cnt.show_archived, cnt.duration, cnt.enable_cache_invalidation, cnt.dynamic_content_variable, cnt.start_level, cnt.end_level, cnt.use_level_filtration, cnt.return_last_modified");
            sb.Append(", cnt.content_id as container_content_id, cf.content_id as form_content_id");
            sb.Append(" from object_format objf");
            sb.Append(" inner join object obj on objf.[object_id] = obj.[object_id]");
            sb.Append(" inner join page_template pt on obj.page_template_id = pt.page_template_id");
            sb.Append(" left join container cnt on obj.[object_id] = cnt.[object_id]");
            sb.Append(" left join content_form cf on obj.[object_id] = cf.[object_id]");
            sb.Append(" left join content c on (c.content_id = cnt.content_id or c.content_id = cf.content_id)");
            BaseObjectsSql = sb.ToString();
        }
        public string BaseObjectsSql { get; private set; }

        public string SiteObjectsSql
        {
            get
            {
                var sb = new StringBuilder(BaseObjectsSql);
                sb.Append(" where pt.site_id = " + Info.SiteId);
                return sb.ToString();
            }
        }



        internal void FillTemplateRow()
        {
            Data.Rows.Add(TemplateRow);
        }

        public DataRow TemplateRow
        {
            get
            {
                var row = Data.NewRow();
                row["ASSEMBLE_IN_LIVE"] = Info.GetBoolean("ASSEMBLE_IN_LIVE");
                row["ASSEMBLE_IN_STAGE"] = Info.GetBoolean("ASSEMBLE_IN_STAGE");
                row["CODE_BEHIND"] = Info.TemplateCodeBehind;
                row["FORMAT_BODY"] = Info.TemplatePresentation;
                row["NET_OBJECT_NAME"] = Info.GetString("NET_TEMPLATE_NAME");
                row["NET_LANGUAGE_ID"] = Info.GetInt32("NET_LANGUAGE_ID");
                row["ENABLE_VIEWSTATE"] = Info.GetBoolean("ENABLE_VIEWSTATE");
                row["DISABLE_DATABIND"] = Info.GetBoolean("DISABLE_DATABIND");
                row["PAGE_TEMPLATE_ID"] = Info.GetInt32("PAGE_TEMPLATE_ID");
                row["TEMPLATE_NAME"] = Info.GetString("TEMPLATE_NAME");
                row["CONTROL_CUSTOM_CLASS"] = Info.GetString("TEMPLATE_CUSTOM_CLASS");
                row["PAGE_ID"] = DBNull.Value;
                row["NET_FORMAT_NAME"] = DBNull.Value;
                row["OBJECT_TYPE_ID"] = 1;
                row["ROOT"] = 1;
                return row;
            }
        }


        public static string GetCallFilter(ObjectCall call)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[template_name] = '{0}' and [object_name] = '{1}' ", call.TemplateName, call.ObjectName);
            if (call.TypeCode == "TOF" || call.TypeCode == "OF")
            {
                sb.AppendFormat(" and [format_name] = '{0}' ", call.FormatName);
            }
            else if (call.TypeCode == "TO" || call.TypeCode == "O")
            {
                sb.Append(" and [default_format_id] = [current_format_id] ");
            }
            else
            {
                throw new ArgumentException("Unknown call type - " + call.TypeCode);
            }
            return sb.ToString();
        }
        public static string GetAdditionalCallFilter(ObjectCall call)
        {

            if (call.TypeCode == "TOF" || call.TypeCode == "TO")
            {
                return " and [page_id] is null ";
            }
            else if (call.TypeCode == "OF" || call.TypeCode == "O")
            {
                return " and [page_id] is not null ";
            }
            else
            {
                return "";
            }
        }

        internal bool HasAlreadyLoaded(ObjectCall call)
        {
            var dvLoadedObjects = new DataView(Data);
            var dvObjects = new DataView(Info.Objects);
            var filter = GetCallFilter(call);
            dvObjects.RowFilter = filter;
            dvLoadedObjects.RowFilter = filter;
            if (dvObjects.Count == 2)
            {
                dvLoadedObjects.RowFilter = dvLoadedObjects.RowFilter + GetAdditionalCallFilter(call);
            }
            var result = dvLoadedObjects.Count;
            dvObjects.Dispose();
            dvLoadedObjects.Dispose();
            return result > 0;
        }

        internal void Load(ObjectCall call)
        {

            var sb = new StringBuilder(SiteObjectsSql);
            if ((call.TypeCode == "OF" || call.TypeCode == "O") && !String.IsNullOrEmpty(Info.PageId))
            {
                sb.Append(" and lower(obj.object_name) = N'");
                sb.Append(call.ObjectName);
                sb.Append("'");

                sb.Append(" and (obj.page_id = ");
                sb.Append(Info.PageId);
                sb.Append(" or obj.page_id is null and lower(pt.template_name) = N'");
                sb.Append(call.TemplateName);
                sb.Append("'");
                sb.Append(" and obj.object_name not in (select object_name from object where page_id = ");
                sb.Append(Info.PageId);
                sb.Append("))");
            }
            else
            {
                sb.Append(" and lower(obj.object_name) = N'");
                sb.Append(call.ObjectName);
                sb.Append("'");

                sb.Append(" and obj.page_id is null");

                sb.Append(" and lower(pt.template_name) = N'");
                sb.Append(call.TemplateName);
                sb.Append("'");
            }
            Info.Controller.Cnn.GetData(sb.ToString(), Data);
        }
    }
}

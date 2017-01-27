using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Assembling.Info
{
    public class AssembleInfo
    {
        public AssembleInfo(AssembleControllerBase controller, string sqlQuery)
        {
            Controller = controller;
            Data = controller.Cnn.GetDataTable(sqlQuery);
            FillAssembleInfo();
        }

        public AssembleInfo(AssembleControllerBase controller, DataTable data)
        {
            Controller = controller;
            Data = data;
            FillAssembleInfo();
        }

        public bool GetNumericBoolean(string fieldName)
        {
            return Convert.ToBoolean(GetInt32(fieldName));
        }

        public int GetInt32(string fieldName)
        {
            int result;
            if (FirstDataRow.Table.Columns[fieldName] == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Field {0} is not found in the page-level information table", fieldName));
            }
            if (!int.TryParse(GetString(fieldName), out result))
            {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "Cannot convert field {0} to Int32", fieldName));
            }

            return result;
        }

        public string GetString(string fieldName)
        {
            return FirstDataRow.Table.Columns[fieldName] != null ? FirstDataRow[fieldName].ToString() : "";
        }

        public bool GetBoolean(string fieldName)
        {
            if (FirstDataRow.Table.Columns[fieldName] == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Field {0} is not found in the page-level information table", fieldName));
            }

            if (FirstDataRow[fieldName] == DBNull.Value)
            {
                throw new ArgumentException("Unexpected null value: " + fieldName);
            }

            return (bool)FirstDataRow[fieldName];
        }

        private void FillAssembleInfo()
        {
            InitPermissionLevels();
            if (Data.Rows.Count > 0)
            {
                Controls = new ControlSetInfo(this, Controller.IsDbConnected);
                Paths = new PathInfo(this);
                Security = new SecurityOptions(this);
                Transformer = new CodeTransformer(this);
                if (Controller.IsDbConnected)
                {
                    FillDataTables();
                }
            }
            else
            {
                throw new DataException("Cannot load assemble data");
            }
        }

        private void InitPermissionLevels()
        {
            PermissionLevels = new Hashtable { { "4", "0" }, { "6", "1" }, { "3", "2" }, { "2", "3" }, { "1", "4" } };
        }

        public string TemplateCodeBehind
        {
            get
            {
                if (!IsAssembleFormatMode)
                {
                    return GetString("CODE_BEHIND");
                }

                return IsPreviewMode ? GetString("PREVIEW_CODE_BEHIND") : "";
            }
        }

        public string TemplatePresentation
        {
            get
            {
                if (!IsAssembleFormatMode)
                {
                    return GetString("TEMPLATE_BODY");
                }

                var result = string.Format(CultureInfo.InvariantCulture, "<qp:placeholder runat=\"server\" calls=\"{0}.{1}\" />", GetString("OBJECT_NAME"), GetString("FORMAT_NAME"));
                if (IsPreviewMode)
                {
                    if (!string.IsNullOrEmpty(GetString("PREVIEW_TEMPLATE_BODY")))
                    {
                        result = Regex.Replace(GetString("PREVIEW_TEMPLATE_BODY"), @"<qp:previewholder\s+\/>", result);
                    }
                }
                else
                {
                    if (Mode == AssembleMode.GlobalCss)
                    {
                        result = string.Format(CultureInfo.InvariantCulture, "<html><body>{0}</body></html>", result);
                    }
                }

                return result;
            }
        }

        public Hashtable TemplateNamespaces
        {
            get
            {
                var hash = new Hashtable();
                var records = Regex.Split(GetString("USING"), @"[,\s\n]+", RegexOptions.Multiline);
                foreach (var record in records)
                {
                    if (!string.IsNullOrEmpty(record) && !hash.ContainsKey(record))
                    {
                        hash.Add(record, "");
                    }
                }

                return hash;
            }
        }

        private void FillDataTables()
        {
            Statuses = Controller.Cnn.GetDataTable("select * from status_type");
            Templates = Controller.Cnn.GetDataTable("select * from page_template pt where site_id=" + SiteId);
            ContainerStatuses = Controller.Cnn.GetDataTable("select cs.* from container_statuses cs inner join object obj on cs.[object_id] = obj.[object_id] inner join page_template pt on pt.page_template_id = obj.page_template_id where pt.site_id = " + SiteId);
            ObjectValues = Controller.Cnn.GetDataTable("select ov.* from object_values ov inner join object obj on ov.[object_id] = obj.[object_id] inner join page_template pt on pt.page_template_id = obj.page_template_id where pt.site_id = " + SiteId);
            ThankYouPages = Controller.Cnn.GetDataTable("SELECT p.page_name, p.page_id, pt.template_folder, p.page_folder, p.page_filename, cf.object_id FROM content_form AS cf LEFT OUTER JOIN page AS p ON p.page_id = cf.thank_you_page_id LEFT OUTER JOIN page_template AS pt ON pt.page_template_id = p.page_template_id where pt.site_id = " + SiteId);
            FileAttributes = Controller.Cnn.GetDataTable(" select ca.*, s.use_site_library as source_use_site_library, s.content_id as source_content_id from content_attribute ca left join content_attribute s on ca.persistent_attr_id = s.attribute_id inner join content c on ca.content_id = c.content_id where ca.attribute_type_id in (7,8,12) and c.site_id = " + SiteId);
            Workflow = Controller.Cnn.GetDataTable("select cwb.content_id, st.status_type_id, st.status_type_name from workflow_rules wr inner join status_type st on wr.successor_status_id = st.status_type_id inner join content_workflow_bind cwb on cwb.workflow_id = wr.workflow_id where st.weight = (select max(st2.weight) from workflow_rules wr2 inner join status_type st2 on wr.successor_status_id = st.status_type_id where wr2.workflow_id = wr.workflow_id)");

            var pageIdForFilter = PageId;
            if (string.IsNullOrEmpty(pageIdForFilter)) { pageIdForFilter = "0"; }
            Objects = Controller.Cnn.GetDataTable("select pt.template_name, pt.net_template_name, pt.page_template_id, p.page_id, obj.[object_name], obj.[object_id], obj.net_object_name, objf.format_name, objf.net_format_name, obj.object_format_id as default_format_id, p.page_folder, objf.object_format_id as current_format_id from object as obj inner join object_format as objf on obj.object_id = objf.object_id inner join page_template pt on obj.page_template_id = pt.page_template_id left join page as p on p.page_id = obj.page_id where pt.site_id = " + SiteId + " and (obj.page_id is null or obj.page_id = " + pageIdForFilter + ")");
        }

        public AssembleControllerBase Controller { get; }

        public DataTable Data { get; set; }

        public Hashtable PermissionLevels { get; private set; }

        public ControlSetInfo Controls { get; set; }

        public PathInfo Paths { get; private set; }

        public SecurityOptions Security { get; private set; }

        public CodeTransformer Transformer { get; private set; }

        public AssembleMode Mode => Controller.CurrentAssembleMode;

        public Encoding Encoding => Encoding.GetEncoding(CharSet);

        public int CacheTimeInMinutes => Mode == AssembleMode.Page ? GetInt32("CACHE_HOURS") * 60 : 0;

        public int CachingType => Mode == AssembleMode.Page ? GetInt32("PROXY_CACHE") : 0;

        public string CharSet => GetString(!string.IsNullOrEmpty(PageId) ? "PAGE_CHARSET" : "TEMPLATE_CHARSET");

        public AssembleLocation Location => GetNumericBoolean("IS_LIVE") ? AssembleLocation.Live : AssembleLocation.Stage;

        public int SiteId => GetInt32("SITE_ID");

        public Status GetMaxStatus() => GetMaxStatus(SiteId);

        public static string PageLanguage => "vb";

        public string PageFileName => !IsAssembleFormatMode ? GetString("PAGE_FILENAME") : GetString("OBJECT_ID") + ".aspx";

        public string PageGetOutputFileName => PageFileName.Replace(".aspx", "_GetControlOutput.aspx");

        public string NetTemplateName => GetNetName(GetString("NET_TEMPLATE_NAME"), TemplateId.ToString(CultureInfo.InvariantCulture), "t");

        public static string GetNetName(string netName, string id, string typeCode)
        {
            return !string.IsNullOrEmpty(netName) ? netName : typeCode + id;
        }

        public string TemplateName => GetString("TEMPLATE_NAME");

        public int TemplateId => GetInt32("PAGE_TEMPLATE_ID");

        public string PageId => GetString("PAGE_ID");

        public string PageIdWithDefault => string.IsNullOrEmpty(PageId) ? "0" : PageId;

        public DataTable Templates { get; set; }

        public DataTable Statuses { get; set; }

        public DataTable ContainerStatuses { get; set; }

        public DataTable ObjectValues { get; set; }

        public DataTable ThankYouPages { get; set; }

        public DataTable FileAttributes { get; set; }

        public DataTable Workflow { get; set; }

        public DataTable Objects { get; set; }

        public string TemplateFileName => NetTemplateName + ".ascx";

        public string TemplateCodeFileName => TemplateFileName + "." + GetLangExtension(GetInt32("NET_LANGUAGE_ID"));

        public string PageCodeFileName => PageFileName + "." + PageLanguage;

        public string PageGetOutputCodeFileName => PageGetOutputFileName + "." + PageLanguage;

        public DataRow FirstDataRow => Data.Rows[0];

        public static bool UsePartialClasses => true;

        public bool AssembleForMobile => GetBoolean("FOR_MOBILE_DEVICES");

        public bool EnableOnScreen => string.IsNullOrEmpty(GetString("ENABLE_ONSCREEN")) || GetBoolean("ENABLE_ONSCREEN");

        public bool AssembleAllControls => Mode == AssembleMode.Page;

        public bool ForceAssemble => GetNumericBoolean("FORCE_ASSEMBLE");

        public bool ForceLive => IsAssembleFormatMode && GetBoolean("ASSEMBLE_FORMATS_IN_LIVE");

        public bool ForceTestDirectory => IsLive && !IsAssembleFormatMode && !string.IsNullOrEmpty(GetString("FORCE_TEST_DIRECTORY")) && GetBoolean("FORCE_TEST_DIRECTORY") && !string.IsNullOrEmpty(GetString("TEST_DIRECTORY"));

        private string CustomClassName => GetString(!string.IsNullOrEmpty(GetString("PAGE_CUSTOM_CLASS")) ? "PAGE_CUSTOM_CLASS" : "CUSTOM_CLASS_FOR_PAGES");

        public string SystemClassName => AssembleForMobile ? "Quantumart.QPublishing.QMobilePage" : "Quantumart.QPublishing.QPage";

        public string BaseClassName => !string.IsNullOrEmpty(CustomClassName) ? CustomClassName : SystemClassName;

        public bool IsAssembleObjectsMode => Mode == AssembleMode.AllPageObjects ||
                                             Mode == AssembleMode.AllTemplateObjects ||
                                             Mode == AssembleMode.SelectedObjects;

        public bool IsAssembleFormatMode => Mode == AssembleMode.GlobalCss ||
                                            Mode == AssembleMode.Notification ||
                                            Mode == AssembleMode.Preview ||
                                            Mode == AssembleMode.PreviewById ||
                                            Mode == AssembleMode.PreviewAll;

        public bool IsPreviewMode => Mode == AssembleMode.PreviewById ||
                                     Mode == AssembleMode.PreviewAll ||
                                     Mode == AssembleMode.Preview;

        public bool IsLive => Location == AssembleLocation.Live;

        public bool AssembleMainFormat { get; set; } = true;

        public string PageAssembleMode
        {
            get
            {
                switch (Mode)
                {
                    case AssembleMode.Notification:
                        return "Mode.Notification";
                    case AssembleMode.Preview:
                        return "Mode.PreviewObjects";
                    case AssembleMode.PreviewAll:
                    case AssembleMode.PreviewById:
                        return "Mode.PreviewArticles";
                    default:
                        return "Mode.GlobalCss";
                }
            }
        }

        public static string Configuration(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? ConfigurationManager.AppSettings[key] : "";
        }

        public Status GetMaxStatus(int siteId)
        {
            var dv = new DataView(Statuses)
            {
                RowFilter = "SITE_ID = " + siteId.ToString(CultureInfo.InvariantCulture),
                Sort = "WEIGHT DESC"
            };

            var result = new Status
            {
                Id = Convert.ToInt32((decimal)dv[0]["STATUS_TYPE_ID"]),
                Name = dv[0]["STATUS_TYPE_NAME"].ToString()
            };

            dv.Dispose();
            return result;
        }

        public static string GetLangExtension(int langCode)
        {
            var result = string.Empty;
            if (langCode == 1)
            {
                result = "cs";
            }

            if (langCode == 2)
            {
                result = "vb";
            }

            return result;
        }

        public static string GetLangName(int langCode)
        {
            var result = string.Empty;
            if (langCode == 1)
            {
                result = "c#";
            }

            if (langCode == 2)
            {
                result = "vb";
            }

            return result;
        }

        public static ControlType GetControlType(int controlTypeId)
        {
            switch (controlTypeId)
            {
                case 2: { return ControlType.PublishingContainer; }
                case 9: { return ControlType.PublishingForm; }
                default: { return ControlType.Generic; }
            }
        }

        internal bool IsTemplateName(string name)
        {
            var dv = new DataView(Templates) { RowFilter = "[template_name] = '" + name + "'" };
            var result = dv.Count > 0;
            dv.Dispose();
            return result;
        }

        public string AssembleAllowanceFieldName
        {
            get
            {
                if (IsAssembleFormatMode)
                {
                    switch (Mode)
                    {
                        case AssembleMode.Notification:
                            return IsLive || ForceLive ? "assemble_notification_in_live" : "assemble_notification_in_stage";
                        case AssembleMode.Preview:
                            return IsLive || ForceLive ? "assemble_preview_in_live" : "assemble_preview_in_stage";
                    }

                    return string.Empty;
                }

                return IsLive ? "assemble_in_live" : "assemble_in_stage";
            }
        }

        internal bool GenerateTrace => Mode == AssembleMode.Page && GetBoolean("GENERATE_TRACE");

        internal bool GenerateOnScreen => !(IsLive || IsAssembleFormatMode) && EnableOnScreen;
    }
}

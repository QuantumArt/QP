using System;
using System.Globalization;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using QA_Assembling.Info;


namespace QA_Assembling
{

    public enum AssembleMode
    {
        Page,
        AllPageObjects,
        AllTemplateObjects,
        SelectedObjects,
        Notification,
        Preview,
        PreviewById,
        PreviewAll,
        GlobalCss,
        Contents
    }

    public enum AssembleLocation
    {
        Live,
        Stage
    }




    public class AssembleControllerBase
    {

        #region Service properties and methods


        private readonly string _spacer = " ";

        public static string Renames { get; protected set; } = ", p.charset as page_charset, pt.charset as template_charset, p.enable_viewstate as page_enable_viewstate, pt.enable_viewstate as template_enable_viewstate ";

        public static string RenamesWithoutPage { get; protected set; } = ", pt.charset as template_charset, pt.enable_viewstate as template_enable_viewstate ";

        internal virtual string GetFilter()
        {
            return "";
        }

        protected AssembleControllerBase(DbConnector cnn)
        {
            Cnn = cnn;
            IsDbConnected = true;
        }

        protected AssembleControllerBase(string connectionParameter)
        {
            FillController(connectionParameter, true);
        }

        protected AssembleControllerBase(string connectionParameter, bool isCustomerCode)
        {
            FillController(connectionParameter, isCustomerCode);
        }

        private void FillController(string connectionParameter, bool isCustomerCode)
        {
            Cnn = new DbConnector(connectionParameter, isCustomerCode);
            IsDbConnected = true;
        }

        protected AssembleControllerBase()
        {
            //m_isDbConnected = false;
        }

        public AssembleMode CurrentAssembleMode { get; protected set; }

        public bool IsDbConnected { get; private set; }

        public bool IsLocalAssembling => !IsDbConnected;

        public AssembleInfo Info { get; protected set; }

        internal DbConnector Cnn { get; private set; }

        public bool UseFixedLocation { get; protected set; }

        public AssembleLocation FixedLocation { get; protected set; } = AssembleLocation.Live;

        /*internal static string BackendUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["BackendUrl"];
            }
        }
        */

        /*private AssembleLocation m_assembleLocation;
        internal AssembleLocation CurrentAssembleLocation
        {
            get { return m_assembleLocation; }
            set { m_assembleLocation = value; }
        }*/


        protected static void CreateFolder(string path)
        {
            var pathFragments = path.Split('\\');
            var sb = new StringBuilder();
            foreach (var fragment in pathFragments)
            {
                sb.Append(fragment);
                sb.Append(@"\");
                if (!fragment.EndsWith(":", StringComparison.OrdinalIgnoreCase))
                {
                    var currentPath = sb.ToString();
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
                }


            }
        }

        protected StreamWriter CreateFile(string path)
        {
            var sw = new StreamWriter(path, false, Info.Encoding);
            return sw;
        }

        protected static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }

        }

        #endregion

        #region Code Generation
        protected string EnableSessionString
        {
            get
            {
                if (Info.GetNumericBoolean("ALLOW_USER_SESSIONS") || !IsLive)
                { return "EnableSessionState=\"True\""; }
                else
                { return "EnableSessionState=\"False\""; }
            }
        }

        protected string EnableViewStateString
        {
            get
            {
                var enableViewState = Info.Mode == AssembleMode.Page ? Info.GetBoolean("PAGE_ENABLE_VIEWSTATE") : Info.GetBoolean("TEMPLATE_ENABLE_VIEWSTATE");
                if (enableViewState)
                {
                    return "EnableViewState=\"True\"";
                }
                else
                {
                    return "EnableViewState=\"False\"";
                }
            }
        }
        protected static string GetControlEnableViewStateString(ControlInfo control)
        {
            if (control.EnableViewState)
            { return "EnableViewState=\"True\""; }
            else
            { return "EnableViewState=\"False\""; }
        }
        protected string PageSourceString
        {
            get
            {
                var codeName = AssembleInfo.UsePartialClasses ? "CodeFile" : "Src";
                return string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"", codeName, Info.PageCodeFileName);
            }
        }
        protected static string GetControlSourceString(ControlInfo control)
        {
            var codeName = AssembleInfo.UsePartialClasses ? "CodeFile" : "Src";
            return string.Format(CultureInfo.InvariantCulture, "{0}=\"{1}\"", codeName, control.CommonCodeFileName);
        }

        protected string NamespaceName
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("QP7.Site").Append(Info.SiteId);
                sb.Append(".").Append("Template").Append(Info.TemplateId);

                if (!string.IsNullOrEmpty(Info.PageId))
                {
                    sb.Append(".").Append("Page").Append(Info.PageId);
                }

                return sb.ToString();
            }
        }


        protected string ClassName => string.Format(CultureInfo.InvariantCulture, "Default_{0}", Info.PageFileName.Replace(".", "_"));

        protected string FullClassName => string.Format(CultureInfo.InvariantCulture, "{0}.{1}", NamespaceName, ClassName);

        protected string GetControlFullClassName(ControlInfo control)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", NamespaceName, control.ClassName);
        }

        protected string ClassDefinition
        {
            get
            {
                var sb = new StringBuilder("Public");
                sb.Append(_spacer);
                if (AssembleInfo.UsePartialClasses)
                {
                    sb.Append("Partial");
                    sb.Append(_spacer);
                }
                sb.Append("Class");
                sb.Append(_spacer);
                sb.Append(ClassName);
                return sb.ToString();
            }

        }

        protected string PageInheritsDefinition
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("Inherits");
                sb.Append(" ");
                sb.Append(Info.BaseClassName);
                return sb.ToString();
            }
        }

        protected static string GetControlInheritsDefinition(ControlInfo control)
        {
            var sb = new StringBuilder();
            sb.Append(control.IsCSharp ? ":" : "Inherits");
            sb.Append(" ");
            sb.Append(control.BaseClassName);
            return sb.ToString();
        }

        protected string PageDirective
        {
            get
            {

                var sb = new StringBuilder();
                sb.Append("<%@ Page");
                sb.Append(_spacer);

                sb.Append("Language=\"");
                sb.Append(AssembleInfo.PageLanguage);
                sb.Append("\"");
                sb.Append(_spacer);

                sb.Append("Strict=\"True\" AutoEventWireup=\"false\"");
                sb.Append(_spacer);

                sb.Append(EnableSessionString);
                sb.Append(_spacer);
                sb.Append(EnableViewStateString);
                sb.Append(_spacer);

                sb.Append("Inherits=");
                sb.Append(FullClassName);
                sb.Append(_spacer);

                sb.Append(PageSourceString);
                sb.Append(_spacer);
                sb.Append("%>");

                return sb.ToString();
            }

        }

        protected string GetControlDirective(ControlInfo control)
        {
            var sb = new StringBuilder();
            sb.Append("<%@ Control");
            sb.Append(_spacer);

            sb.Append("Language=\"");
            sb.Append(control.Language);
            sb.Append("\"");
            sb.Append(_spacer);

            sb.Append("Inherits=\"");
            sb.Append(GetControlFullClassName(control));
            sb.Append("\"");
            sb.Append(_spacer);

            sb.Append("ClassName=\"");
            sb.Append(control.ClassName);
            sb.Append("_Presentation");
            sb.Append("\"");
            sb.Append(_spacer);

            sb.Append(GetControlEnableViewStateString(control));
            sb.Append(_spacer);

            if (AssembleInfo.UsePartialClasses)
            {
                sb.Append(GetControlCompilerOptions(control));
                sb.Append(_spacer);
            }

            sb.Append(GetControlSourceString(control));
            sb.Append(_spacer);
            sb.Append("%>");

            return sb.ToString();

        }

        private static string GetControlCompilerOptions(ControlInfo control)
        {
            var sb = new StringBuilder();
            sb.Append("CompilerOptions=");
            sb.Append("'\"");
            sb.Append(control.TargetFolder);
            sb.Append(control.CommonSystemCodeFileName);
            sb.Append("\"'");
            return sb.ToString();
        }

        protected void AppendStandardDirectives(StringBuilder sb)
        {
            sb.AppendLine("<%@ import NameSpace=\"Quantumart.QPublishing\" %>");
            sb.AppendLine("<%@ import NameSpace=\"System.IO\" %>");
            sb.AppendLine("<%@ import NameSpace=\"System.Data\" %>");
            sb.Append("<%@ Register TagPrefix=\"qp\" NameSpace=\"Quantumart.QPublishing\" Assembly=\"Quantumart\" %>");
            if (IsLocalAssembling) sb.AppendLine("");
        }

        protected void GeneratePageInit(StringBuilder sb, int pageLevel)
        {
            sb.Append(Padding(pageLevel)).AppendLine("protected Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init");

            WriteCommonInitialization(sb, checked(pageLevel + 1));
            WritePageSpecificInitialization(sb, checked(pageLevel + 1));

            sb.Append(Padding(pageLevel)).AppendLine("End Sub");
        }


        public bool IsLive
        {
            get
            {
                if (UseFixedLocation)
                {
                    return FixedLocation == AssembleLocation.Live;
                }
                else
                {
                    return Info.IsLive;
                }
            }
        }

        protected void WriteCommonInitialization(StringBuilder sb, int pageLevel)
        {
            sb.Append(Padding(pageLevel)).AppendLine("Me.site_id = " + Info.SiteId);
            sb.Append(Padding(pageLevel)).AppendLine("Me.page_template_id = " + Info.TemplateId);
            sb.Append(Padding(pageLevel)).AppendLine(string.Format(CultureInfo.InvariantCulture, "Me.IsPreview = {0}", Info.IsAssembleFormatMode));
            if (Info.IsAssembleFormatMode)
            {
                sb.Append(Padding(pageLevel)).AppendLine(string.Format(CultureInfo.InvariantCulture, "Me.PageAssembleMode = {0}", Info.PageAssembleMode));
            }
            if (Info.ForceTestDirectory) { sb.Append(Padding(pageLevel)).AppendLine("Me.IsTest = True"); }
            sb.Append(Padding(pageLevel)).AppendLine(string.Format(CultureInfo.InvariantCulture, "Me.IsStage = {0}", !IsLive && !Info.ForceLive));
            sb.Append(Padding(pageLevel)).AppendLine("Me.LastModified = New DateTime(" + DateTime.Now.Ticks + ")");
        }
        protected void WritePageSpecificInitialization(StringBuilder sb, int pageLevel)
        {
            sb.Append(Padding(pageLevel)).AppendLine("Me.page_id = " + Info.PageIdWithDefault);
            sb.Append(Padding(pageLevel)).AppendLine("Me.GenerateTrace = " + BoolStr(false, Info.GenerateTrace));
            sb.Append(Padding(pageLevel)).AppendLine("Me.Controls_Folder = \"" + Info.Paths.PageControlsFolderName + "/\"");
            sb.Append(Padding(pageLevel)).AppendLine("Me.PageFolder = \"" + Info.Paths.PageFolder + "\"");

            if (IsLocalAssembling)
            {
                sb.Append(Padding(pageLevel)).AppendLine("Me.IsLocalAssembling = \"True\"");
                sb.Append(Padding(pageLevel)).AppendLine("Me.site_url = \"" + Info.Paths.SiteUrl + "\"");
                sb.Append(Padding(pageLevel)).AppendLine("Me.upload_url = \"" + Info.Paths.UploadUrl + "\"");
                if (Info.GetNumericBoolean("USE_ABSOLUTE_UPLOAD_URL"))
                {
                    sb.Append(Padding(pageLevel)).AppendLine("Me.UploadURLPrefix = \"" + Info.Paths.UploadUrlPrefix + "\"");
                }
            }
            sb.Append(Padding(pageLevel)).AppendLine("Initialize()");
        }

        protected string GetObjectValuesCode(ControlInfo control, int padLevel)
        {
            var values = new Hashtable();
            var sb = new StringBuilder();
            GetDefaultObjectValues(control, values);
            foreach (string key in values.Keys)
            {
                sb.Append(Padding(padLevel)).AppendFormat("    AddObjectValue ({0}, {1}){2}", key, values[key], LineEnd(control.IsCSharp));
            }
            return sb.ToString();
        }

        internal static string GetOnScreenHeadHtml(ControlInfo control)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<script language='JavaScript' src='/rs/jquery.min.js'></script>");
            sb.AppendLine("<script language='JavaScript' src='/rs/rs.js'></script>");
            sb.AppendLine("<script language='JavaScript' src='/rs/moz_common.js'></script>");
            sb.AppendLine("<script language='JavaScript' src='/rs/onscreen_common.js'></script>");
            sb.AppendLine("<script language='JavaScript' src='/rs/onscreen_main.js'></script>");
            sb.AppendLine("<script language='JavaScript' src='/rs/onscreen_handlers.js'></script>");
            sb.AppendLine("<script language='JavaScript' src='/rs/wch.js'></script>");

            sb.AppendLine("<script language='JavaScript'>RSEnableRemoteScripting('/rs');</script>");

            sb.AppendLine("<script>");
            sb.AppendLine("<!--");
            sb.AppendLine("	var g_siteUrl = '<%# Server.UrlEncode(site_url)%>';");
            sb.AppendLine("	var g_uploadUrl = '<%# Server.UrlEncode(upload_url)%>';");
            sb.AppendLine("	var g_returnStageUrl = location.href;");
            sb.AppendLine("	if (g_returnStageUrl.indexOf('?') <= 0) g_returnStageUrl += '?';");
            sb.AppendLine("	g_returnStageUrl = encodeURIComponent(g_returnStageUrl);");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "	var g_backendUrl = '<%# Session{0}\"qp_backend_url\"{1}%>';", BeginItemChar(control.IsCSharp), EndItemChar(control.IsCSharp)));
            sb.AppendLine("-->");
            sb.AppendLine("</script>");
            sb.AppendLine("<link rel='stylesheet' type='text/css' href='/rs/onscreen.css' />");
            return sb.ToString();
        }

        protected static string GetInitCode(ControlInfo control, int padLevel)
        {
            var sb = new StringBuilder();
            sb.Append(Padding(padLevel)).AppendFormat("FormatId = {0}{1}", control.GetInt32("CURRENT_FORMAT_ID"), LineEnd(control.IsCSharp)).AppendLine("");
            if (control.DisableDataBind)
            {
                sb.Append(Padding(padLevel)).AppendFormat("DisableDataBind = {0}{1}", BoolStr(control.IsCSharp, control.DisableDataBind), LineEnd(control.IsCSharp)).AppendLine("");
            }
            return sb.ToString();
        }

        protected string GetTraceCode(ControlInfo control, int padLevel)
        {
            var sb = new StringBuilder();
            var defValues = new Hashtable();
            var undefValues = new Hashtable();

            FindFormatValues(control, defValues, undefValues);

            if (defValues.Keys.Count > 0 || undefValues.Keys.Count > 0)
            {

                sb.Append(Padding(padLevel)).AppendFormat("{0}", GetIf(control.IsCSharp, "Not QPTrace Is Nothing", "QPTrace != null")).AppendLine("");
                foreach (string key in defValues.Keys)
                {
                    sb.Append(Padding(checked(padLevel + 1))).AppendFormat("AddQpDefValue({0}, {1}){2}", key, defValues[key], LineEnd(control.IsCSharp)).AppendLine("");
                }
                if (undefValues.Keys.Count > 0)
                {
                    sb.Append(Padding(checked(padLevel + 1))).AppendFormat("{0}", GetObjectVariableDeclaration(control.IsCSharp, "traceBuilder", "StringBuilder")).AppendLine("");
                    foreach (string key in undefValues.Keys)
                    {
                        var undefValue = undefValues[key].ToString();
                        undefValue = control.IsCSharp ? undefValue.Replace("\"", "\\\"") : undefValue.Replace("\"", "\"\"");
                        sb.Append(Padding(checked(padLevel + 1))).AppendFormat("traceBuilder.Append(\"Value({0});\"){1}", undefValue, LineEnd(control.IsCSharp)).AppendLine("");
                    }
                    sb.Append(Padding(checked(padLevel + 1))).AppendFormat("{0}.UndefTraceString = traceBuilder.ToString(){1}", Self(control.IsCSharp), LineEnd(control.IsCSharp)).AppendLine("");
                }
                sb.Append(Padding(padLevel)).AppendFormat("{0}", GetEndIf(control.IsCSharp)).AppendLine("");
            }
            return sb.ToString();
        }

        protected void FindFormatValues(ControlInfo control, Hashtable defined, Hashtable undefined)
        {
            var arr = GetSourcesToFind(control);
            FillValues(defined, undefined, arr);
            GetDefaultObjectValues(control, defined);
        }

        protected void GetDefaultObjectValues(ControlInfo control, Hashtable defined)
        {
            var dv = new DataView(Info.ObjectValues) {RowFilter = "OBJECT_ID = " + control.GetInt32("OBJECT_ID")};
            for (var i = 0; i < dv.Count; i++)
            {
                var variableName = "\"" + dv[i]["VARIABLE_NAME"] + "\"";
                var variableValue = "\"" + dv[i]["VARIABLE_VALUE"] + "\"";
                defined[variableName] = variableValue;
            }
            dv.Dispose();
        }

        protected static void FillValues(Hashtable defined, Hashtable undefined, string[] array)
        {
            var valueRegex = new Regex(@"(?!add)value[s]?\((?<key>[^),]+)\)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            foreach (var elem in array)
            {
                if (!string.IsNullOrEmpty(elem))
                {
                    var matches = valueRegex.Matches(elem);
                    foreach(Match m in matches)
                    {
                        var value = m.Groups["key"].Value;
                        var trimValue = value.Length > 2 ? value.Substring(1, value.Length - 2) : value;
                        var key = value.ToLowerInvariant();
                        if (value.IndexOf('"') != 0 || value.LastIndexOf('"') != value.Length - 1 || trimValue.IndexOf('"') > 0)
                        {
                            if (!undefined.Contains(key))
                            {
                                undefined.Add(key, value);
                            }
                        }
                        else
                        {
                            if (!defined.Contains(key))
                            {
                                defined.Add(key, "\"\"");
                            }
                        }
                     }
                }
            }
        }

        protected static string[] GetSourcesToFind(ControlInfo control)
        {
            var arr = new string[5];
            arr[0] = control.Presentation;
            arr[1] = control.CodeBehind;
            if (control.CurrentType == ControlType.PublishingContainer)
            {
                arr[2] = control.Row["FILTER_VALUE"].ToString();
                arr[3] = control.Row["SELECT_TOTAL"].ToString();
                var allowDynamicOrder = control.GetObject("ALLOW_ORDER_DYNAMIC") != DBNull.Value && control.GetNumericBoolean("ALLOW_ORDER_DYNAMIC");
                arr[4] = allowDynamicOrder ? control.Row["ORDER_DYNAMIC"].ToString() : "";
            }
            return arr;
        }

        #endregion

        public virtual void Assemble()
        {

        }


        public Code GetControlCode(DataRow dr)
        {

            var control = new ControlInfo(dr, Info);
            return new Code(GetControlPresentationCodeFile(control), GetControlCodeBehindCodeFile(control, false), GetControlCodeBehindCodeFile(control, true), Info.Paths.BaseAssemblePath, control.TargetFolder);
        }

        public Code PageCode => new Code(PagePresentationCodeFile, PageCodeBehindCodeFile, null, Info.Paths.BaseAssemblePath, Info.Paths.PageFolderPath);

        internal CodeFile GetControlPresentationCodeFile(ControlInfo control)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetControlDirective(control));
            AppendStandardDirectives(sb);
            sb.Append(CodeTransformer.GetProcessedPresentation(control));
            sb.Append(Environment.NewLine);
            return new CodeFile(sb.ToString(), control.CommonFileName);
        }
        internal string GetControlPresentation(ControlInfo control)
        {
            return GetControlPresentationCodeFile(control).Code;
        }

        internal CodeFile GetControlCodeBehindCodeFile(ControlInfo control, bool isSystem)
        {
            var sb = new StringBuilder();
            sb.Append(GetUsingNamespaces(control));
            sb.Append(Environment.NewLine);
            sb.AppendLine(GetStartNamespace(control.IsCSharp, NamespaceName));
            sb.AppendLine(GetControlClassDefinition(control, 1));
            if (isSystem || !AssembleInfo.UsePartialClasses)
            {
                sb.Append(GetTypeSpecificCode(control, 2));
            }
            if (!isSystem)
            {
                sb.Append(CodeTransformer.GetProcessedCodeBehind(control));
            }
            sb.AppendLine("");
            sb.Append(Padding(1)).AppendLine(GetEndClass(control));
            sb.AppendLine(GetEndNamespace(control.IsCSharp));
            return new CodeFile(sb.ToString(), isSystem ? control.CommonSystemCodeFileName : control.CommonCodeFileName);
        }

        internal string GetControlCodeBehind(ControlInfo control, bool isSystem)
        {
            return GetControlCodeBehindCodeFile(control, isSystem).Code;
        }

        public CodeFile PagePresentationCodeFile
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(PageDirective);
                AppendStandardDirectives(sb);
                return new CodeFile(sb.ToString(), Info.PageFileName);
            }
        }

        public string PagePresentation => PagePresentationCodeFile.Code;

        public CodeFile PageCodeBehindCodeFile
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("Imports Quantumart.QPublishing");
                sb.AppendLine("Namespace " + NamespaceName);
                sb.AppendLine(ClassDefinition);
                sb.AppendLine(PageInheritsDefinition);
                GeneratePageInit(sb, 1);
                GeneratePagePreRender(sb, 1);
                GeneratePageUnload(sb, 1);
                sb.AppendLine("End Class");
                sb.AppendLine("End Namespace");
                return new CodeFile(sb.ToString(), Info.PageCodeFileName);
            }
        }
        public string PageCodeBehind => PageCodeBehindCodeFile.Code;

        protected void AssemblePageFiles()
        {
            if (NeedToAssemblePage())
            {
                CreateFolder(Info.Paths.PageFolderPath);

                DeleteOldPageFiles();

                using (var sw = CreateFile(Info.Paths.FullPagePath))
                {
                    sw.Write(PagePresentation);
                    sw.Write(GetPresentationTimeStamp());
                    sw.Close();
                }

                using (var sw = CreateFile(Info.Paths.FullPageCodePath))
                {
                    sw.Write(PageCodeBehind);
                    sw.WriteLine("");
                    sw.Write(GetPageCodeTimeStamp());
                    sw.Close();
                }

                if (!Info.IsLive && !Info.IsAssembleFormatMode)
                {
                    CreateMyFlyFile();
                }

                //if (Info.IsLive && Info.Mode == AssembleMode.Page)
                //GenerateControlOutputPage();

                MarkPageAssembledInDatabase();
            }

        }

        private void CreateMyFlyFile()
        {
            var myFlyCodeFile = MyFlyPresentationCodeFile;
            var myFlyPath = Info.Paths.StagePath + "\\" + myFlyCodeFile.FileName;
            if (!File.Exists(myFlyPath))
            {
                using (var sw = CreateFile(myFlyPath))
                {
                    sw.Write(myFlyCodeFile.Code);
                    sw.Close();
                }
            }
        }

        public Code MyFlyCode => new Code(MyFlyPresentationCodeFile, null, null, Info.Paths.BaseAssemblePath, Info.Paths.BaseAssemblePath);

        private static CodeFile MyFlyPresentationCodeFile => new CodeFile("<%@ Page language=\"vb\" AutoEventWireup=\"false\" Inherits=\"Quantumart.QPublishing.OnFlyPage\" %>", "MyFly.aspx");


        protected static void GeneratePageUnload(StringBuilder sb, int pageLevel)
        {
            sb.Append(Padding(pageLevel)).AppendLine("protected Sub Page_Unload(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Unload");
            sb.Append(Padding(checked(pageLevel + 1))).AppendLine("HandleRender()");
            sb.Append(Padding(pageLevel)).AppendLine("End Sub");
        }

        protected static void GeneratePagePreRender(StringBuilder sb, int pageLevel)
        {
            sb.Append(Padding(pageLevel)).AppendLine("protected Sub Page_PreRender(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.PreRender");
            sb.Append(Padding(checked(pageLevel + 1))).AppendLine("HandlePreRender()");
            sb.Append(Padding(pageLevel)).AppendLine("End Sub");
        }

        protected void AssembleControlFiles(ControlInfo control)
        {
            CreateFolder(control.TargetFolder);

            DeleteOldControlFiles(control);

            using (var sw = CreateFile(control.TargetFolder + control.CommonFileName))
            {
                sw.Write(GetControlPresentation(control));
                sw.Write(GetPresentationTimeStamp());
                sw.Close();
            }

            if (AssembleInfo.UsePartialClasses)
            {
                using (var sw = CreateFile(control.TargetFolder + control.CommonSystemCodeFileName))
                {
                    sw.Write(GetControlCodeBehind(control, true));
                    sw.WriteLine("");
                    sw.Write(GetCodeTimeStamp(control));
                    sw.Close();
                }
            }
            using (var sw = CreateFile(control.TargetFolder + control.CommonCodeFileName))
            {
                sw.Write(GetControlCodeBehind(control, false));
                sw.WriteLine("");
                sw.Write(GetCodeTimeStamp(control));
                sw.Close();
            }

        }

        private string GetPresentationTimeStamp()
        {
            return $"{"<%--"}Generated at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}{"--%>"}";
        }

        private string GetCodeTimeStamp(ControlInfo control)
        {
            return
                $"{GetSingleLineComment(control)}Generated at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}"; 
        }

        private string GetPageCodeTimeStamp()
        {
            return $"'Generated at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}";
        }

        private void DeleteOldControlFiles(ControlInfo control)
        {
            DeleteFile(control.TargetFolder + control.SimpleCodeFileName);
            DeleteFile(control.TargetFolder + control.SimpleFileName);
            if (control.IsRoot && !Info.IsAssembleFormatMode && !string.IsNullOrEmpty(Info.PageId))
            {
                DeleteFile(Info.Paths.PageControlsPath + control.CommonFileName);
                DeleteFile(Info.Paths.PageControlsPath + control.CommonCodeFileName);
            }
        }

        private void DeleteOldPageFiles()
        {
            DeleteFile(Info.Paths.PageFolderPath + Info.PageGetOutputFileName);
            DeleteFile(Info.Paths.PageFolderPath + Info.PageGetOutputCodeFileName);
        }

        protected string GetControlClassDefinition(ControlInfo control, int padLevel)
        {
            var sb = new StringBuilder(Padding(padLevel));
            sb.Append(control.IsCSharp ? "public" : "Public");
            sb.Append(_spacer);
            if (AssembleInfo.UsePartialClasses)
            {
                sb.Append(control.IsCSharp ? "partial" : "Partial");
                sb.Append(_spacer);
            }
            sb.Append(control.IsCSharp ? "class" : "Class");
            sb.Append(_spacer);
            sb.Append(control.ClassName);
            if (control.IsCSharp)
            {
                sb.Append(" ");
                sb.AppendLine(GetControlInheritsDefinition(control));
                sb.Append(Padding(padLevel)).Append("{");
            }
            else
            {
                sb.AppendLine("");
                sb.Append(GetControlInheritsDefinition(control));
            }
            return sb.ToString();
        }

        protected static string GetStartNamespace(bool isCSharp, string name)
        {
            if (isCSharp)
            {
                return "namespace " + name + Environment.NewLine + "{";
            }
            else
            {
                return "Namespace " + name;
            }
        }

        protected static string GetEndNamespace(bool isCSharp)
        {
            if (isCSharp)
            {
                return "}";
            }
            else
            {
                return "End Namespace";
            }
        }

        protected static string GetEndClass(ControlInfo control)
        {
            if (control.IsCSharp)
            {
                return "}";
            }
            else
            {
                return "End Class";
            }
        }

        protected static string GetSingleLineComment(ControlInfo control)
        {
            if (control.IsCSharp)
            {
                return "//";
            }
            else
            {
                return "'";
            }
        }

        protected static string GetObjectVariableDeclaration(bool isCSharp, string name, string type, string value)
        {
            return GetVariableDeclarationLeft(isCSharp, name, type, type) + " = new " + type + "(" + value + ")" + LineEnd(isCSharp);
        }

        protected static string GetObjectVariableDeclaration(bool isCSharp, string name, string type)
        {
            return GetObjectVariableDeclaration(isCSharp, name, type, "");
        }

        protected static string GetVariableDeclaration(bool isCSharp, string name, string vbType, string csType, string value)
        {
            return GetVariableDeclarationLeft(isCSharp, name, vbType, csType) + " = " + value + LineEnd(isCSharp);
        }

        protected static string GetArrayDeclarationLeft(bool isCSharp, string name, string vbType, string csType)
        {
            return GetVariableDeclarationLeft(isCSharp, name + BeginItemChar(isCSharp) + EndItemChar(isCSharp), vbType, csType);
        }

        protected static string GetVariableDeclaration(bool isCSharp, string name, string vbType, string csType)
        {
            return GetVariableDeclarationLeft(isCSharp, name, vbType, csType) + LineEnd(isCSharp);
        }

        protected static string GetVariableDeclarationLeft(bool isCSharp, string name, string vbType, string csType)
        {
            if (isCSharp)
            {
                return csType + " " + name;
            }
            else
            {
                return "Dim " + name + " As " + vbType;
            }
        }

        protected static string GetForEach(bool isCSharp, string key, string array)
        {
            if (isCSharp)
            {
                return string.Format(CultureInfo.InvariantCulture, "foreach (string {0} in {1}) {{", key, array);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "For Each {0} In {1}", key, array);
            }
        }

        protected static string GetIf(bool isCSharp, string vbCondition, string csCondition)
        {
            if (isCSharp)
            {
                return "if (" + csCondition + ") {";
            }
            else
            {
                return "If " + vbCondition + " Then";
            }
        }

        protected static string GetIf(bool isCSharp, string condition)
        {
            return GetIf(isCSharp, condition, condition);
        }

        protected static string GetElse(bool isCSharp)
        {
            return isCSharp ? "} else {" : "Else";
        }

        protected static string GetEndIf(bool isCSharp)
        {
            return isCSharp ? "}" : "End If";
        }

        protected static string GetEndSub(bool isCSharp)
        {
            return isCSharp ? "}" : "End Sub";
        }

        protected static string GetNext(bool isCSharp)
        {
            return isCSharp ? "}" : "Next";
        }


        protected static string Padding(int size)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                sb.Append("\t");
            }
            return sb.ToString();
        }

        internal static string LineEnd(bool isCSharp)
        {
            return isCSharp ? ";" : "";
        }

        internal static string Cast(bool isCSharp, string expr, string type)
        {
            return isCSharp ? $"({type})({expr})" : $"DirectCast({expr}, {type})";
        }

        internal static string Self(bool isCSharp)
        {
            return isCSharp ? "this" : "Me";
        }

        internal static string BoolStr(bool isCSharp, bool value)
        {
            if (value)
            {
                return isCSharp ? "true" : "True";
            }
            else
            {
                return isCSharp ? "false" : "False";
            }
        }


        internal static string ConcatChar(bool isCSharp)
        {
            return isCSharp ? "+" : "&";
        }

        internal static string BeginItemChar(bool isCSharp)
        {
            return isCSharp ? "[" : "(";
        }

        internal static string EndItemChar(bool isCSharp)
        {
            return isCSharp ? "]" : ")";
        }

        internal static string Ref(bool isCSharp)
        {
            return isCSharp ? "ref" : "";
        }

        /*internal static string Equal(bool isCSharp)
        {
            return (isCSharp) ? "==" : "="; 
        }*/

        internal static string InEqual(bool isCSharp)
        {
            return isCSharp ? "!=" : "<>"; 
        }

        /*internal string And(bool isCSharp)
        {
            return (isCSharp) ? "&&" : "And"; 
        }

        internal static string Or(bool isCSharp)
        {
            return (isCSharp) ? "||" : "Or"; 
        }

        internal static string Not(bool isCSharp)
        {
            return (isCSharp) ? "!" : "Not";
        }
        */

        internal static string Imports(bool isCSharp)
        {
            return isCSharp ? "using" : "Imports";
        }



        protected string GetUsingNamespaces(ControlInfo control)
        {
            var sb = new StringBuilder();
            sb.Append(GetUsingNamespaces(control.IsCSharp, CodeTransformer.SystemNamespaces));
            sb.Append(GetUsingNamespaces(control.IsCSharp, Info.TemplateNamespaces));
            sb.Append(GetUsingNamespaces(control.IsCSharp, control.UserNamespaces));
            return sb.ToString();
        }

        internal static string GetUsingNamespaces(bool isCSharp, Hashtable hash)
        {
            var sb = new StringBuilder();
            foreach (string key in hash.Keys)
            {
                sb.Append(Imports(isCSharp));
                sb.Append(" ");
                sb.Append(key);
                sb.Append(LineEnd(isCSharp));
                sb.AppendLine("");
            }
            return sb.ToString();

        }



        protected void AssembleControlSet()
        {

            while (Info.Controls.RowIndex < Info.Controls.Count)
            {
                Info.Controls.FillRowSpecificInfo();
                var control = Info.Controls.Current;
                if (IsDbConnected && !Info.IsAssembleObjectsMode)
                {
                    ParseFormat(control);
                    if (Info.IsAssembleFormatMode && Info.Controls.RowIndex == 0)
                    {
                        var checkFormat =
                            new ControlInfo(Info.Controls.Data.Rows[Info.Controls.RowIndex + 1], Info);
                        Info.AssembleMainFormat = NeedToAssembleControl(checkFormat);
                    }
                }
                if (NeedToAssembleControl(control))
                {
                    MarkControlAssembledInDatabase(control);
                    AssembleControlFiles(control);
                }
                Info.Controls.RowIndex++;
            }

        }


        private string GetTypeSpecificCode(ControlInfo control, int padLevel)
        {
            if (control.IsRoot)
            {
                return "";
            }
            else
            {

                var sb = new StringBuilder();
                BeginLoadControlDataSub(control, sb, padLevel);
                if (control.CurrentType == ControlType.Generic || control.ContentSelected)
                {
                    WriteControlInitCode(control, sb, checked(padLevel + 1));
                    WriteControlGetDataCode(control, sb, checked(padLevel + 1));
                }
                else
                {
                    sb.Append(Padding(checked(padLevel + 1))).AppendLine(string.Format(CultureInfo.InvariantCulture, "throw new Exception(\"{0}\"){1}", control.MissedContentExceptionString, LineEnd(control.IsCSharp)));
                }

                sb.Append(Padding(padLevel)).AppendLine(GetEndSub(control.IsCSharp));

                if (control.CurrentType == ControlType.PublishingContainer && !control.Container.IsNewAssembling)
                {
                    WriteTotalRecordsSub(control, sb, padLevel);
                }
                return sb.ToString();
            }
        }

        protected static void WriteTotalRecordsSub(ControlInfo control, StringBuilder sb, int padLevel)
        {
            if (!control.IsCSharp)
            {
                sb.Append(Padding(padLevel)).AppendLine("protected Sub GetTotalRecords(o as Object, e as SqlDataSourceStatusEventArgs)");
                sb.Append(Padding(checked(padLevel + 1))).AppendLine("Me.AbsoluteTotalRecords = Convert.ToInt32((CType(e.Command.Parameters(\"@TotalRecords\"), SqlParameter)).Value)");
                sb.Append(Padding(checked(padLevel + 1))).AppendLine("SaveAbsoluteTotalRecordsInCache(Me.AbsoluteTotalRecords, " + control.Container.Duration + ")");
                sb.Append(Padding(padLevel)).AppendLine("End Sub");
            }
            else
            {
                sb.Append(Padding(padLevel)).AppendLine("protected void GetTotalRecords(Object o, SqlDataSourceStatusEventArgs e)");
                sb.Append(Padding(padLevel)).AppendLine("{");
                sb.Append(Padding(checked(padLevel + 1))).AppendLine("this.AbsoluteTotalRecords = Convert.ToInt32(((SqlParameter)e.Command.Parameters[\"@TotalRecords\"]).Value);");
                sb.Append(Padding(checked(padLevel + 1))).AppendLine("SaveAbsoluteTotalRecordsInCache(this.AbsoluteTotalRecords, " + control.Container.Duration + ");");
                sb.Append(Padding(padLevel)).AppendLine("}");
            }
        }

        protected void WriteControlGetDataCode(ControlInfo control, StringBuilder sb, int padLevel)
        {
            if (control.CurrentType == ControlType.PublishingForm || (control.CurrentType == ControlType.PublishingContainer && !control.Container.IsNewAssembling))
            {
                sb.Append(Padding(padLevel)).AppendLine("ContentName = \"" + control.GetString("CONTENT_NAME") + "\"" + LineEnd(control.IsCSharp));
                sb.Append(Padding(padLevel)).AppendLine(string.Format(CultureInfo.InvariantCulture, "ContentUploadURL = \"{0}\"{1}", Info.Paths.GetContentUploadUrl(control.GetInt32("CONTENT_ID")), LineEnd(control.IsCSharp)));
            }
            if (control.CurrentType == ControlType.PublishingForm)
            {
                sb.Append(Padding(padLevel)).AppendLine(GetVariableDeclaration(control.IsCSharp, "itemId", "Int32", "int", "0"));
                sb.Append(Padding(padLevel)).AppendLine(GetIf(control.IsCSharp, "String.IsNullOrEmpty(Value(\"get_content_item_id\")) And IsNumeric(Value(\"get_content_item_id\"))", "!String.IsNullOrEmpty(Value(\"get_content_item_id\")) && Int32.TryParse(Value(\"get_content_item_id\"), out itemId)"));
                if (!control.IsCSharp)
                {
                    sb.Append(Padding(checked(padLevel + 1))).AppendLine("itemId = CType(Value(\"get_content_item_id\"), Int32)" + LineEnd(control.IsCSharp));
                }
                sb.Append(Padding(padLevel)).AppendLine(GetEndIf(control.IsCSharp));
                sb.Append(Padding(padLevel)).AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}.ThankYouPage = site_url {3} \"{1}\"{2}", Self(control.IsCSharp), control.GetThankYouPage(), LineEnd(control.IsCSharp), ConcatChar(control.IsCSharp)));
                sb.Append(Padding(padLevel)).AppendLine("DataTable dt = Cnn.GetData(\"SELECT * FROM content_" + control.GetString("CONTENT_ID") + " WHERE content_item_id = \" + itemId.ToString())" + LineEnd(control.IsCSharp));
            }
            else if (control.CurrentType == ControlType.PublishingContainer)
            {

                if (control.Container.IsNewAssembling)
                {
                    sb.Append(Padding(padLevel)).AppendLine(
                        $"ContentID = {control.GetString("CONTENT_ID")}{LineEnd(control.IsCSharp)}");
                    if(!string.IsNullOrEmpty(control.GetString("DYNAMIC_CONTENT_VARIABLE")))
                        sb.Append(Padding(padLevel)).AppendLine(
                            $"DynamicVariable = \"{control.GetString("DYNAMIC_CONTENT_VARIABLE")}\"{LineEnd(control.IsCSharp)}");
                    if (control.Container.Duration != 0) sb.Append(Padding(padLevel)).AppendLine("Duration = " + control.Container.Duration + LineEnd(control.IsCSharp));
                    if (control.Container.EnableCacheInvalidation) sb.Append(Padding(padLevel)).AppendLine("EnableCacheInvalidation = " + control.GetBoolean("ENABLE_CACHE_INVALIDATION").ToString().ToLower() + LineEnd(control.IsCSharp));
                    if (control.Container.IsRandom) sb.Append(Padding(padLevel)).AppendLine("RotateContent = true" + LineEnd(control.IsCSharp));
                    if (control.Container.ForceUnited) sb.Append(Padding(padLevel)).AppendLine("ForceUnited = true" + LineEnd(control.IsCSharp));
                    if (!control.Container.UseSchedule) sb.Append(Padding(padLevel)).AppendLine("UseSchedule = false" + LineEnd(control.IsCSharp));
                    if (control.Container.ShowArchive) sb.Append(Padding(padLevel)).AppendLine("ShowArchive = true" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine(
                        $"Statuses = \"{control.Container.StatusString}\"{LineEnd(control.IsCSharp)}");
                    if (!string.IsNullOrEmpty(control.Container.CustomFilter)) sb.Append(Padding(padLevel)).AppendLine(
                        $"CustomFilter = {control.Container.CustomFilter}{LineEnd(control.IsCSharp)}");
                    if (!string.IsNullOrEmpty(control.Container.StaticOrder)) sb.Append(Padding(padLevel)).AppendLine(
                        $"StaticOrder = \"{control.Container.StaticOrder}\"{LineEnd(control.IsCSharp)}");
                    if (control.Container.DynamicOrder != "\"\"") sb.Append(Padding(padLevel)).AppendLine(
                        $"DynamicOrder = {control.Container.DynamicOrder}{LineEnd(control.IsCSharp)}");
                    if (!string.IsNullOrEmpty(control.GetString("SELECT_START"))) sb.Append(Padding(padLevel)).AppendLine(
                        $"StartRow = ({control.GetString("SELECT_START")}).ToString(){LineEnd(control.IsCSharp)}");
                    if (!string.IsNullOrEmpty(control.GetString("SELECT_TOTAL"))) sb.Append(Padding(padLevel)).AppendLine(
                        $"PageSize = ({control.GetString("SELECT_TOTAL")}).ToString(){LineEnd(control.IsCSharp)}");
                    if (control.Container.ApplySecurity)
                    {
                        sb.Append(Padding(padLevel)).AppendLine("UseSecurity = true" + LineEnd(control.IsCSharp));
                        if (control.Container.UseLevelFiltration)
                        {
                            sb.Append(Padding(padLevel)).AppendLine("UseLevelFiltration = true" + LineEnd(control.IsCSharp));
                            sb.Append(Padding(padLevel)).AppendLine(
                                $"StartLevel = \"{control.Container.StartLevel}\"{LineEnd(control.IsCSharp)}");
                            sb.Append(Padding(padLevel)).AppendLine(
                                $"EndLevel = \"{control.Container.EndLevel}\"{LineEnd(control.IsCSharp)}");
                        }

                    }
                    if (Info.IsAssembleFormatMode && control.Container.AssembleRootObject) 
                        sb.Append(Padding(padLevel)).AppendLine("IsRoot = true" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("FillData()" + LineEnd(control.IsCSharp));
                }


                else
                {
                    if (string.IsNullOrEmpty(control.GetString("DYNAMIC_CONTENT_VARIABLE")))
                    {
                        sb.Append(Padding(padLevel)).AppendLine(
                            $"ContentID = {control.GetString("CONTENT_ID")}{LineEnd(control.IsCSharp)}");
                    }
                    else
                    {
                            sb.Append(Padding(padLevel)).AppendLine(GetVariableDeclaration(control.IsCSharp, "extSiteId", "Integer", "int", "0"));
                            sb.Append(Padding(padLevel)).AppendLine(
                                $"ContentID = Cnn.GetDynamicContentId(InternalStrValue(\"{control.GetString("DYNAMIC_CONTENT_VARIABLE")}\") , {control.GetString("CONTENT_ID")}, {Info.SiteId}, {Ref(control.IsCSharp)} extSiteId){LineEnd(control.IsCSharp)}");
                            sb.Append(Padding(padLevel)).AppendLine(GetIf(control.IsCSharp,
                                $"ContentID {InEqual(control.IsCSharp)} {control.GetString("CONTENT_ID")}"));
                            sb.Append(Padding(padLevel + 1)).AppendLine(
                                $"ContentUploadURL = Cnn.GetContentUploadUrlByID(extSiteId, ContentID){LineEnd(control.IsCSharp)}");
                            sb.Append(Padding(padLevel + 1)).AppendLine(
                                $"ContentName = Cnn.GetContentName({Cast(control.IsCSharp, "ContentID", "Int32")}){LineEnd(control.IsCSharp)}");
                            sb.Append(Padding(padLevel)).AppendLine(GetEndIf(control.IsCSharp));

                    }
                    
                    var united = control.Container.FromClause.IndexOf("UNITED", StringComparison.OrdinalIgnoreCase) > 0 ? "\"_UNITED\"" : "String.Empty";
                    sb.Append(Padding(padLevel)).AppendLine(GetVariableDeclaration(control.IsCSharp, "united", "String", "string", united));
                    sb.Append(Padding(padLevel)).AppendLine(GetVariableDeclaration(control.IsCSharp, "table", "String", "string", "String.Format(\"content_{0}{1}\", ContentID, united)"));
                    sb.Append(Padding(padLevel)).AppendLine(GetVariableDeclaration(control.IsCSharp, "from", "String", "string", "String.Format(\" {0} AS c WITH (NOLOCK)\", table)"));
                    sb.Append(Padding(padLevel)).AppendLine(GetVariableDeclaration(control.IsCSharp, "where", "String", "string", control.Container.WhereClause(control.IsCSharp)));
                    sb.Append(Padding(padLevel)).AppendLine(GetVariableDeclaration(control.IsCSharp, "orderBy", "String", "string", control.Container.OrderClause));



                    sb.Append(Padding(padLevel)).AppendLine(GetObjectVariableDeclaration(control.IsCSharp, "src", "SqlDataSource"));
                    sb.Append(Padding(padLevel)).AppendLine("src.ConnectionString = ConfigurationManager.ConnectionStrings" + BeginItemChar(control.IsCSharp) + "\"qp_database\"" + EndItemChar(control.IsCSharp) + ".ConnectionString" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("src.SelectCommand = \"qp_GetContentPage\"" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("src.SelectCommandType = SqlDataSourceCommandType.StoredProcedure" + LineEnd(control.IsCSharp));
                    if (control.Container.Duration != 0)
                    {
                        sb.Append(Padding(padLevel)).AppendLine("src.EnableCaching = true" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.CacheExpirationPolicy = DataSourceCacheExpiry.Absolute" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.CacheDuration = " + control.Container.Duration + LineEnd(control.IsCSharp));
                    }

                    if (control.GetBoolean("ENABLE_CACHE_INVALIDATION"))
                    {
                        sb.Append(Padding(padLevel)).AppendLine("SqlCacheDependencyAdmin.EnableNotifications(ConfigurationManager.ConnectionStrings" + BeginItemChar(control.IsCSharp) + "\"qp_database\"" + EndItemChar(control.IsCSharp) + ".ConnectionString)" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("SqlCacheDependencyAdmin.EnableTableForNotifications(ConfigurationManager.ConnectionStrings" + BeginItemChar(control.IsCSharp) + "\"qp_database\"" + EndItemChar(control.IsCSharp) + ".ConnectionString, \"CONTENT_\" " + ConcatChar(control.IsCSharp) + " ContentID " + ")" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SqlCacheDependency = \"qp_database:CONTENT_\" " + ConcatChar(control.IsCSharp) + " ContentID " + LineEnd(control.IsCSharp));
                    }

                    if (!control.Container.ApplySecurity)
                    {
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"Select\", TypeCode.String, " + ContainerInfo.SelectClause + ")" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"From\", TypeCode.String, from )" + LineEnd(control.IsCSharp));
                    }
                    else
                    {

                        if (control.Container.UseLevelFiltration)
                        {
                            sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"Select\", TypeCode.String, " + ContainerInfo.SelectClause + ")" + LineEnd(control.IsCSharp));
                            sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"From\", TypeCode.String, from " + ConcatChar(control.IsCSharp) + " \" inner join (" + ContainerInfo.SecurityMagicString + ") as pi on c.content_item_id = pi.content_item_id\")" + LineEnd(control.IsCSharp));
                        }
                        else
                        {
                            sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"Select\", TypeCode.String, " + ContainerInfo.SelectClause + " " + ConcatChar(control.IsCSharp) + " \", IsNull(pi.permission_level,0) as current_permission_level\") " + LineEnd(control.IsCSharp));
                            sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"From\", TypeCode.String, from " + ConcatChar(control.IsCSharp) + " \" left outer join (" + ContainerInfo.SecurityMagicString + ") as pi on c.content_item_id = pi.content_item_id\")" + LineEnd(control.IsCSharp));
                        }

                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"use_security\", TypeCode.Int32, \"1\")" + LineEnd(control.IsCSharp));

                        if (!control.IsCSharp)
                        {
                            sb.Append(Padding(padLevel)).AppendLine("Dim qpUid, qpGid as String");
                            sb.Append(Padding(padLevel)).AppendLine("if System.Web.HttpContext.Current.Session Is Nothing then");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("qpUid = \"0\"");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("qpGid = \"0\"");
                            sb.Append(Padding(padLevel)).AppendLine("else");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("if " + Info.Security.UserIdVbName + " Is Nothing then");
                            sb.Append(Padding(checked(padLevel + 2))).AppendLine("qpUid = \"0\"");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("else");
                            sb.Append(Padding(checked(padLevel + 2))).AppendLine("qpUid = " + Info.Security.UserIdVbName + ".ToString()");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("end if");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("if " + Info.Security.GroupIdVbName + " Is Nothing then");
                            sb.Append(Padding(checked(padLevel + 2))).AppendLine("qpGid = \"0\"");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("else");
                            sb.Append(Padding(checked(padLevel + 2))).AppendLine("qpGid = " + Info.Security.GroupIdVbName + ".ToString()");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("end if");
                            sb.Append(Padding(padLevel)).AppendLine("end if");
                        }
                        else
                        {
                            sb.Append(Padding(padLevel)).AppendLine("string qpUid, qpGid;");
                            sb.Append(Padding(padLevel)).AppendLine("if (System.Web.HttpContext.Current.Session == null) {");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("qpUid = \"0\";");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("qpGid = \"0\";");
                            sb.Append(Padding(padLevel)).AppendLine("} else {");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("qpUid = (" + Info.Security.UserIdCSharpName + " == null) ? \"0\" : " + Info.Security.UserIdCSharpName + ".ToString();");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("qpGid = (" + Info.Security.GroupIdCSharpName + " == null) ? \"0\" : " + Info.Security.GroupIdCSharpName + ".ToString();");
                            sb.Append(Padding(padLevel)).AppendLine("}");
                        }

                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"user_id\", TypeCode.Decimal, qpUid)" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"group_id\", TypeCode.Decimal, qpGid)" + LineEnd(control.IsCSharp));

                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"start_level\", TypeCode.Int32, (" + control.Container.StartLevel + ").ToString())" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"end_level\", TypeCode.Int32, (" + control.Container.EndLevel + ").ToString())" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"entity_name\", TypeCode.String, \"content_item\")" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"parent_entity_name\", TypeCode.String, \"content\")" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"parent_entity_id\", TypeCode.Decimal, ContentID.ToString())" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"insert_key\", TypeCode.String, \"" + ContainerInfo.SecurityMagicString + "\")" + LineEnd(control.IsCSharp));
                    }

                    sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"Where\", TypeCode.String, where)" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"OrderBy\", TypeCode.String, " + control.Container.OrderClause + " " + ConcatChar(control.IsCSharp) + " \" \")" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"StartRow\", TypeCode.Int32, (" + control.Container.StartRow + ").ToString())" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"PageSize\", TypeCode.Int32, (" + control.Container.PageSize + ").ToString())" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(\"GetCount\", TypeCode.Int32, \"" + control.Container.GetCountWithoutPaging + "\")" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine(GetObjectVariableDeclaration(control.IsCSharp, "totalRecords", "Parameter", "\"TotalRecords\""));


                    sb.Append(Padding(padLevel)).AppendLine("totalRecords.Type = TypeCode.Int32" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("totalRecords.Direction = ParameterDirection.Output" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("src.SelectParameters.Add(totalRecords)" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine(Self(control.IsCSharp) + ".CacheKey = String.Format(\"{0}_{1}_{2}\", " + Self(control.IsCSharp) + ".FormatId, from, where.Replace(\" \", \"_\"))" + LineEnd(control.IsCSharp));

                    if (control.Container.GetCountWithoutPaging != "0")
                    {
                        if (!control.IsCSharp)
                        {
                            sb.Append(Padding(padLevel)).AppendLine("AddHandler src.Selected, AddressOf GetTotalRecords" + LineEnd(control.IsCSharp));
                        }
                        else
                        {
                            sb.Append(Padding(padLevel)).AppendLine("src.Selected += new SqlDataSourceStatusEventHandler(GetTotalRecords)" + LineEnd(control.IsCSharp));
                        }
                    }

                    if (!control.IsCSharp)
                    {
                        sb.Append(Padding(padLevel)).AppendLine("Dim dt as DataTable = New DataTable()" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("Dim dw as DataView = CType(src.Select(new DataSourceSelectArguments()), DataView)" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("if not IsNothing(dw) ");
                        sb.Append(Padding(checked(padLevel + 1))).AppendLine("dt = dw.ToTable()");
                        sb.Append(Padding(padLevel)).AppendLine("end if");
                    }
                    else
                    {
                        sb.Append(Padding(padLevel)).AppendLine("DataTable dt = new DataTable()" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("DataView dw = (DataView)src.Select(new DataSourceSelectArguments())" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("if (dw != null) {");
                        sb.Append(Padding(checked(padLevel + 1))).AppendLine("dt = dw.ToTable()" + LineEnd(control.IsCSharp));
                        sb.Append(Padding(padLevel)).AppendLine("}");
                        if (control.Container.Duration != 0)
                        {
                            sb.Append(Padding(padLevel)).AppendLine("if (!dt.Columns.Contains(\"content_item_id\")) {");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("src.EnableCaching = false" + LineEnd(control.IsCSharp));
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("dw = (DataView)src.Select(new DataSourceSelectArguments())" + LineEnd(control.IsCSharp));
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("dt = dw.ToTable()" + LineEnd(control.IsCSharp));
                            sb.Append(Padding(padLevel)).AppendLine("}");
                            sb.Append(Padding(padLevel)).AppendLine("if (!dt.Columns.Contains(\"content_item_id\")) {");
                            sb.Append(Padding(checked(padLevel + 1))).AppendLine("dt = Cnn.GetCachedData(String.Format(\"select * from {0} where 1 = 0\", from))" + LineEnd(control.IsCSharp));
                            sb.Append(Padding(padLevel)).AppendLine("}");
                        }
                    }

                    sb.Append(Padding(padLevel)).AppendLine("Data = dt" + LineEnd(control.IsCSharp));
                    sb.Append(Padding(padLevel)).AppendLine("TotalRecords = Data.Rows.Count" + LineEnd(control.IsCSharp));
                    if (control.Container.GetCountWithoutPaging == "0")
                    {
                        sb.Append(Padding(padLevel)).AppendLine("AbsoluteTotalRecords = Data.Rows.Count" + LineEnd(control.IsCSharp));
                    }
                    sb.Append(Padding(padLevel)).AppendLine("RecordsPerPage = " + control.Container.PageSize + LineEnd(control.IsCSharp));

                }
                
                if (control.GetBoolean("RETURN_LAST_MODIFIED"))
                {
                    sb.Append(Padding(padLevel)).AppendLine("QPage.SetLastModified(Data)" + LineEnd(control.IsCSharp));
                }

            }
            if (control.CurrentType == ControlType.PublishingForm)
            {
                sb.Append(Padding(padLevel)).AppendLine(Self(control.IsCSharp) + ".Data = dt" + LineEnd(control.IsCSharp));
                sb.Append(Padding(padLevel)).AppendLine(Self(control.IsCSharp) + ".TotalRecords = dt.Rows.Count" + LineEnd(control.IsCSharp));
                sb.Append(Padding(padLevel)).AppendLine(Self(control.IsCSharp) + ".AbsoluteTotalRecords = dt.Rows.Count" + LineEnd(control.IsCSharp));
                sb.Append(Padding(padLevel)).AppendLine(Self(control.IsCSharp) + ".PublishedStatusName = \"" + Info.GetMaxStatus().Name + "\"" + LineEnd(control.IsCSharp));
            }
        }

        protected void WriteControlInitCode(ControlInfo control, StringBuilder sb, int padLevel)
        {
            sb.Append(GetInitCode(control, padLevel));
            sb.Append(GetTraceCode(control, padLevel));

            if (control.CurrentType == ControlType.PublishingForm || control.CurrentType == ControlType.PublishingContainer)
            {
                if (string.IsNullOrEmpty(control.GetString("CONTENT_ID")))
                {
                    throw new ArgumentException("Please, configure the object parameters: Template: " + control.GetString("PAGE_TEMPLATE_NAME") + ", Page: " + Info.GetString("PAGE_NAME") + ", Object: " + control.GetString("OBJECT_NAME") + ".");
                }

                sb.Append(GetObjectValuesCode(control, padLevel));
                sb.AppendLine("");

            }
        }

        protected static void BeginLoadControlDataSub(ControlInfo control, StringBuilder sb, int padLevel)
        {
            if (!control.IsCSharp)
            {
                sb.Append(Padding(padLevel)).AppendLine("Overrides Public Sub LoadControlData(sender as System.Object, e as System.EventArgs)");
            }
            else
            {
                sb.Append(Padding(padLevel)).AppendLine("override public void LoadControlData(System.Object sender, System.EventArgs e)");
                sb.Append(Padding(padLevel)).AppendLine("{");
            }
        }

        protected static void WrapTemplateCodeBehind(string codeBehind, ControlInfo control, StringBuilder sb)
        {
            if (!control.IsCSharp)
            {
                sb.AppendLine("Overrides Public Sub InitUserHandlers(e as System.EventArgs)");
            }
            else
            {
                sb.AppendLine("override public void InitUserHandlers(System.EventArgs e)");
                sb.AppendLine("{");
            }
            sb.Append(codeBehind);
            sb.AppendLine("");
            sb.AppendLine(GetEndSub(control.IsCSharp));
        }

        protected void InvalidateStructureCache()
        {
            CreateFolder(Info.Paths.CacheFilePath);
            InvalidateCache(Info.Paths.StructureCacheFile);
            InvalidateCache(Info.Paths.AllStructureCacheFile);
        }

        protected void InvalidatePageCache()
        {
            CreateFolder(Info.Paths.CacheFilePath);
            InvalidateCache(Info.Paths.PageObjectCacheFile);
            InvalidateCache(Info.Paths.AllPageObjectsCacheFile);
        }

        protected void InvalidateTemplateCache()
        {
            CreateFolder(Info.Paths.CacheFilePath);
            InvalidateCache(Info.Paths.TemplateObjectCacheFile);
            InvalidateCache(Info.Paths.AllTemplateObjectsCacheFile);
        }

        private void InvalidateCache(string path)
        {
            using (var sw = CreateFile(path))
            {
                sw.Write(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
                sw.Close();
            }
        }

        protected bool NeedToAssembleControl(ControlInfo control)
        {
            var fileNotExists = !File.Exists(control.TargetFolder + "\\" + control.CommonFileName);
            var codeFileNotExists = !File.Exists(control.TargetFolder + "\\" + control.CommonCodeFileName);
            var assembleBecauseOfProperties = GetAssembleBecauseOfProperties(control);
            return Info.ForceAssemble || Info.ForceTestDirectory || fileNotExists || codeFileNotExists || assembleBecauseOfProperties;
        }

        private bool GetAssembleBecauseOfProperties(ControlInfo control)
        {
            if (Info.IsAssembleFormatMode)
            {
                if (control.IsRoot || string.IsNullOrEmpty(Info.AssembleAllowanceFieldName))
                {
                    return Info.AssembleMainFormat;
                }
                else
                {
                    return control.GetBoolean(Info.AssembleAllowanceFieldName);
                }
            }
            else
            {
                return control.GetBoolean(Info.AssembleAllowanceFieldName);
            }
        }

        protected bool AssemblePageBecauseOfProperties
        {
            get
            {
                if (Info.Mode == AssembleMode.Page)
                {
                    return Info.GetBoolean(Info.AssembleAllowanceFieldName);
                }
                else if (Info.IsAssembleFormatMode)
                {
                    return Info.AssembleMainFormat;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool NeedToAssemblePage()
        {
            var fileNotExists = !File.Exists(Info.Paths.FullPagePath);
            var codeFileNotExists = !File.Exists(Info.Paths.FullPageCodePath);
            return Info.ForceAssemble || Info.ForceTestDirectory || fileNotExists || codeFileNotExists || AssemblePageBecauseOfProperties;
        }

        protected void MarkPageAssembledInDatabase()
        {
            if (IsDbConnected && !Info.ForceTestDirectory)
            {
                if (!Info.IsAssembleFormatMode)
                {
                    Cnn.ExecuteSql(string.Format(CultureInfo.InvariantCulture, "update page set {0} = 0 where page_id = {1}", Info.AssembleAllowanceFieldName, Info.PageId));
                    Info.FirstDataRow[Info.AssembleAllowanceFieldName] = 0;
                }
            }
        }

        protected void MarkControlAssembledInDatabase(ControlInfo control)
        {
            if (IsDbConnected && !Info.ForceTestDirectory)
            {
                if (Info.IsAssembleFormatMode)
                {
                    if (!control.IsRoot && !string.IsNullOrEmpty(Info.AssembleAllowanceFieldName))
                    {
                        Cnn.ExecuteSql(string.Format(CultureInfo.InvariantCulture, "update object_format set {0} = 0 where object_format_id = {1}", Info.AssembleAllowanceFieldName, control.GetString("CURRENT_FORMAT_ID")));
                        control.Row[Info.AssembleAllowanceFieldName] = 0;
                    }

                }
                else
                {
                    Cnn.ExecuteSql(control.IsRoot
                        ? string.Format(CultureInfo.InvariantCulture,
                            "update page_template set {0} = 0 where page_template_id = {1}",
                            Info.AssembleAllowanceFieldName, Info.GetString("PAGE_TEMPLATE_ID"))
                        : string.Format(CultureInfo.InvariantCulture,
                            "update object_format set {0} = 0 where object_format_id = {1}",
                            Info.AssembleAllowanceFieldName, control.GetString("CURRENT_FORMAT_ID")));
                    control.Row[Info.AssembleAllowanceFieldName] = 0;
                }
            }


        }

        protected void ParseFormat(ControlInfo control)
        {
            var textToSearch = control.CodeBehind + control.Presentation;
            ProcessObjectCalls(Regex.Match(textToSearch, "(?:[:=\\s\\t\\(%#])(?:Object|ObjectNS|ShowObject|ShowObjectNS)(?:[ \\t]*\\([ \\t]*\")([^\\\"\\)\\n]*)(?=\"[\\s]*[,]?[^\\)]*\\))"));
            ProcessObjectCalls(Regex.Match(textToSearch, "(?:<\\s*qp\\:placeholder)(?:[^>]*)(?:calls\\s*=\\s*)(?:[\"']{0,1})([^\\\"'\\/>\\=]+)(?=[\"']{1}|[\\s\\/]+)"));
        }

        protected void ProcessObjectCalls(Match match)
        {
            while (match.Success)
            {
                var callString = match.Groups[1].Value;
                var call = new ObjectCall(callString, Info);
                if (!Info.Controls.HasAlreadyLoaded(call))
                {
                    Info.Controls.Load(call);
                }
                match = match.NextMatch();
            }


        }

        protected static DataTable ConvertToDataTable(DataRow row)
        {
            if (row?.Table == null)
            {
                throw new ArgumentException("row or row.Table mustn't be null");
            }
            var dt = row.Table.Clone();
            dt.ImportRow(row);
            return dt;
        }

        private static string GetInitialPresentation(string presentation)
        {
            return CodeTransformer.GetInitialPresentation(presentation);
        }

        private static string GetInitialCodeBehind(string codeBehind)
        {
            return CodeTransformer.GetInitialCodeBehind(codeBehind);
        }

        public static InitialCode GetInitialCode(Code controlCode)
        {
            return new InitialCode(GetInitialPresentation(controlCode.Presentation.Code), GetInitialCodeBehind(controlCode.CodeBehind.Code));
        }
    }
}

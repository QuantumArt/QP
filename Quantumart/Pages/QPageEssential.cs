using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.Controls;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.Info;
using Quantumart.QPublishing.OnScreen;

namespace Quantumart.QPublishing.Pages
{
    public enum Mode
    {
        Normal,
        PreviewObjects,
        PreviewArticles,
        Notification,
        GlobalCss
    }

    public class QPageEssential
    {
        private bool _useMultiSiteLogic;

        public int SiteId;
        public int PageId;
        public int PageTemplateId;

        private Hashtable _valuesCollection;
        private Hashtable _objectValuesCollection;
        private int _expires;

        private HttpCacheability _httpCacheability;
        private HttpCacheRevalidation _httpCacheRevalidation;
        private TimeSpan _proxyMaxAge;
        private string _charSet;
        private Encoding _contentEncoding;

        private string _templateNetName;
        private string _templateName;
        private static readonly Regex CleanRegex = new Regex("(create |delete |update |grant |revoke |drop |alter |create |backup |restore |sp_|truncate |set |exec |execute |insert |dbcc |deny |union )", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public QPageEssential(Page newPage)
        {
            Page = newPage;
        }

        public bool UseMultiSiteLogic
        {
            get { return _useMultiSiteLogic || DBConnector.AppSettings["UseMultiSiteConfiguration"] == "1"; }
            set { _useMultiSiteLogic = value; }
        }

        public bool IsLocalAssembling { get; set; } = false;

        public Page Page { get; }

        public string PageFolder { get; set; }

        public string UrlToSave { get; set; } = HttpContext.Current.Request.ServerVariables["URL"] + "?" + HttpContext.Current.Request.ServerVariables["QUERY_STRING"];

        public QScreen QScreen { get; set; }

        public QpTrace QpTrace { get; set; }

        public string PageControlsFolder { get; set; }

        public string PageControlsFolderPrefix => "page_controls__";

        public string TemplateControlsFolderPrefix => "template_controls__";

        public string PreviewFolder => "temp/preview/objects";

        public string PreviewArticlesFolder => "temp/preview/articles";

        public string CssFolder => "temp/css";

        public string NotificationFolder => "qp_notifications";

        public string AdditionalFolder
        {
            get
            {
                if (PageAssembleMode == Mode.Notification)
                {
                    return NotificationFolder;
                }
                if (PageAssembleMode == Mode.PreviewArticles)
                {
                    return PreviewArticlesFolder;
                }
                if (PageAssembleMode == Mode.PreviewObjects)
                {
                    return PreviewFolder;
                }
                if (PageAssembleMode == Mode.GlobalCss)
                {
                    return CssFolder;
                }
                return "";
            }
        }

        public Mode PageAssembleMode { get; set; } = Mode.Normal;

        public string TemplateNetName
        {
            get { return _templateNetName ?? (_templateNetName = GetTemplateField("NET_TEMPLATE_NAME")); }
            set { _templateNetName = value; }
        }

        public int ExpiresInMinutes => GetIntPageField("CACHE_HOURS") * 60;

        public int CachingType => GetIntPageField("PROXY_CACHE") * 60;

        public bool SetLastModifiedHeader
        {
            get
            {
                var flag = GetBooleanPageField("SET_LAST_MODIFIED_HEADER");
                return !IsStage && flag.HasValue && flag.Value;
            }
        }

        public bool SendNoCacheHeaders
        {
            get
            {
                var result = true;
                if (PageId != 0 && !IsStage)
                {
                    var pageSendFlag = GetBooleanPageField("SEND_NOCACHE_HEADERS");
                    var templateSendFlag = GetBooleanTemplateField("SEND_NOCACHE_HEADERS");
                    if (pageSendFlag.HasValue && !pageSendFlag.Value || !pageSendFlag.HasValue && templateSendFlag.HasValue && !templateSendFlag.Value)
                    {
                        result = false;
                    }
                }
                return result;
            }
        }

        public string TemplateName
        {
            get { return _templateName ?? (_templateName = GetTemplateField("TEMPLATE_NAME")); }
            set { _templateName = value; }
        }

        private string _templateFolder;

        public string TemplateFolder => _templateFolder ?? (_templateFolder = GetTemplateField("TEMPLATE_FOLDER"));

        private DataView GetTemplateView(int pageTemplateId)
        {
            if (UseMultiSiteLogic)
            {
                return Cnn.GetAllTemplates($"PAGE_TEMPLATE_ID = {pageTemplateId}");
            }
            return Cnn.GetTemplates($"PAGE_TEMPLATE_ID = {pageTemplateId}");
        }

        private DataView GetPageView(int pageId)
        {
            if (UseMultiSiteLogic)
            {
                return Cnn.GetAllPages($"PAGE_ID = {pageId}");
            }
            return Cnn.GetPages($"PAGE_ID = {pageId}");
        }

        private static string GetViewFieldValue(DataView view, string fieldName)
        {
            return view.Count > 0 ? view[0][fieldName.ToUpperInvariant()].ToString() : string.Empty;
        }

        private string GetTemplateField(string fieldName)
        {
            return PageTemplateId != 0 ? GetViewFieldValue(TemplateInfo, fieldName) : string.Empty;
        }

        private string GetPageField(string fieldName)
        {
            return PageId != 0 ? GetViewFieldValue(PageInfo, fieldName) : string.Empty;
        }

        private bool? GetBooleanTemplateField(string fieldName)
        {
            var value = GetTemplateField(fieldName);
            return string.IsNullOrEmpty(value) ? null : (bool?)bool.Parse(value);
        }

        private bool? GetBooleanPageField(string fieldName)
        {
            var value = GetPageField(fieldName);
            return string.IsNullOrEmpty(value) ? null : (bool?)bool.Parse(value);
        }

        private int GetIntPageField(string fieldName)
        {
            var value = GetPageField(fieldName);
            return string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
        }

        public bool IsPreview { get; set; } = false;

        public int Expires
        {
            get { return _expires; }
            set
            {
                _expires = value;
                HttpContext.Current.Response.Cache.SetExpires(DateTime.Now.AddMinutes(value));
            }
        }

        public HttpCacheability HttpCacheability
        {
            get { return _httpCacheability; }
            set
            {
                _httpCacheability = value;
                HttpContext.Current.Response.Cache.SetCacheability(value);
            }
        }

        public HttpCacheRevalidation HttpCacheRevalidation
        {
            get { return _httpCacheRevalidation; }
            set
            {
                _httpCacheRevalidation = value;
                HttpContext.Current.Response.Cache.SetRevalidation(value);
            }
        }

        public TimeSpan ProxyMaxAge
        {
            get { return _proxyMaxAge; }
            set
            {
                _proxyMaxAge = value;
                HttpContext.Current.Response.Cache.SetProxyMaxAge(value);
            }
        }

        public string CharSet
        {
            get { return _charSet; }

            set
            {
                _charSet = value;
                HttpContext.Current.Response.Charset = value;
            }
        }

        public Encoding ContentEncoding
        {
            get { return _contentEncoding; }
            set
            {
                _contentEncoding = value;
                HttpContext.Current.Response.ContentEncoding = value;
            }
        }

        public bool IsTest { get; set; } = false;

        public bool IsStage { get; set; }


        public bool QpIsInStageMode
        {
            get { return IsStage; }
            set { IsStage = value; }
        }


        public DateTime LastModified { get; set; }

        public bool IsLastModifiedDynamic { get; set; }

        public bool GenerateTrace { get; set; }

        public DataTable GetContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive)
        {



            return Cnn.GetContentData(siteName, contentName, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName, showSplittedArticle,
            includeArchive);
        }

        public DataTable GetContentDataWithSecurity(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, long lngUserId, long lngGroupId, int intStartLevel, int intEndLevel)
        {
            return Cnn.GetContentDataWithSecurity(siteName, contentName, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName, showSplittedArticle,
            includeArchive, lngUserId, lngGroupId, intStartLevel, intEndLevel, true);
        }

        public DataTable GetContentDataWithSecurity(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, long lngUserId, long lngGroupId, int intStartLevel, int intEndLevel, bool blnFilterRecords)
        {
            return Cnn.GetContentDataWithSecurity(siteName, contentName, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName, showSplittedArticle,
            includeArchive, lngUserId, lngGroupId, intStartLevel, intEndLevel, blnFilterRecords);
        }

        #region "Functions for loading controls"
        internal void ShowObject(string name, object sender, object[] parameters, IQPage page, bool useSimpleInitOrder)
        {
            var inputName = page.GetInternalCall(name);
            try
            {
                ShowControlWithFullPath(inputName, sender, parameters, useSimpleInitOrder);
            }
            catch (HttpException)
            {
                if (ControlExists(inputName))
                {
                    throw;
                }

                var trialInputName1 = page.GetInternalCall($"{TemplateName}.{name}");
                ShowControlWithFullPath(trialInputName1, sender, parameters, useSimpleInitOrder);
            }
        }

        internal void ShowControl(string name, object sender, object[] parameters)
        {
            var url = GetControlUrl(name);
            if (!string.IsNullOrEmpty(url))
            {
                ShowControlWithFullPath(url, sender, parameters, false);
            }
            else
            {
                throw new Exception($"Control {name} is not found");
            }
        }

        internal void ShowTemplateControl(string name, object sender)
        {
            var url = GetControlUrl(name, true);
            if (!string.IsNullOrEmpty(url))
            {
                ShowControlWithFullPath(url, sender, null, false);
            }
            else
            {
                throw new Exception($"Control {name} is not found");
            }
        }

        internal void ShowControlWithFullPath(string path, object sender, object[] parameters, bool useSimpleInitOrder)
        {
            AddToCallStack(path);
            var myType = BuildManager.GetCompiledType(path);
            var ctrl = Page.LoadControl(myType, parameters);
            InitControl(path, ctrl, sender, useSimpleInitOrder);
            RemoveFromCallStack();
        }

        private void AddToCallStack(string name)
        {
            ObjectCallStack.Add(name);
            if (ObjectCallStack.Count > 32)
            {
                CallStackOverflow();
            }
        }

        private void RemoveFromCallStack()
        {
            ObjectCallStack.RemoveAt(ObjectCallStack.Count - 1);
        }

        private void InitControl(string name, Control ctrl, object sender, bool useSimpleInitOrder)
        {

            if (ctrl is QUserControl)
            {
                InitControl<QUserControl>(name, ctrl, sender, useSimpleInitOrder);
            }
            else if (ctrl is QMobileUserControl)
            {
                InitControl<QMobileUserControl>(name, ctrl, sender, useSimpleInitOrder);
            }
            else if (ctrl is PartialCachingControl)
            {
                AppendControl(sender, ctrl);

            }
        }

        private static void AppendControl(object sender, Control ctrl)
        {
            ((Control)sender).Controls.Add(ctrl);
        }

        private void InitControl<T>(string name, Control ctrl, object sender, bool useSimpleInitOrder) where T : Control, IQUserControl
        {
            if (QpTrace != null)
            {
                InitControlTrace<T>(ctrl);
            }
            ((T)ctrl).UseSimpleInitOrder = useSimpleInitOrder;
            ((T)ctrl).OnLoadControl(sender, ref name);
            if (!useSimpleInitOrder)
            {
                ((T)ctrl).SimulateOnInit(new EventArgs());
            }

            AppendControl(sender, ctrl);

            if (QpTrace != null)
            {
                EndControlTrace<T>(ctrl);
            }
        }

        private void EndControlTrace<T>(Control ctrl) where T : Control, IQUserControl
        {
            if (((T)ctrl).FormatId != 0)
            {
                if (QpTrace.TraceString.EndsWith(QpTrace.TraceStartText + "<br>"))
                {
                    QpTrace.TraceString = QpTrace.TraceString.Substring(0, QpTrace.TraceString.Length - (QpTrace.TraceStartText + "<br>").Length);
                    QpTrace.TraceString = QpTrace.TraceString + Math.Round(DateTime.Now.Subtract(((T)ctrl).TraceStartTime).TotalMilliseconds) + "ms<br>";
                }
                else
                {
                    QpTrace.TraceString = QpTrace.TraceString + (ObjectCallStack.Count - 1) + "-" + ((T)ctrl).FormatId + "-" + Math.Round(DateTime.Now.Subtract(((T)ctrl).TraceStartTime).TotalMilliseconds) + "ms<br>";
                }
            }
        }

        private void InitControlTrace<T>(Control ctrl) where T : Control, IQUserControl
        {
            if (QpTrace != null)
            {
                ((T)ctrl).TraceStartTime = DateTime.Now;
                ((T)ctrl).TraceObjectString = ObjectCallStack.Count - 1 + "-";
            }
        }

        internal bool IsTemplateName(string name)
        {
            return Templates.Contains(name.ToLowerInvariant());
        }

        internal bool IsTemplateName(string name, int siteId)
        {
            return IsTemplateName($"{siteId},{name}");
        }

        public static string GetNetName(string netName, string id, string typeCode)
        {
            if (!string.IsNullOrEmpty(netName))
            {
                return netName;
            }
            return typeCode + id;
        }

        public string GetControlFileName(DataRowView drv)
        {
            var formatName = GetNetName(drv["NET_FORMAT_NAME"].ToString(), drv["CURRENT_FORMAT_ID"].ToString(), "f");
            var objectName = GetNetName(drv["NET_OBJECT_NAME"].ToString(), drv["OBJECT_ID"].ToString(), "o");

            var sb = new StringBuilder();
            sb.Append(objectName);
            if (formatName != "f")
            {
                // Root
                sb.Append("_");
                sb.Append(formatName);
            }
            sb.Append(".ascx");
            return sb.ToString();
        }

        internal bool ControlExists(string url)
        {
            return File.Exists(HttpContext.Current.Server.MapPath(url));
        }

        public string GetInternalCall(string userCall, SiteInfo[] sites)
        {
            var result = string.Empty;
            foreach (var site in sites)
            {
                //HttpContext.Current.Response.Write("Site Id:" && site.Id && "<br>")
                result = GetInternalCall(userCall, site.Id, site.Url);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
            return result;
        }

        public string GetInternalCall(string userCall, int siteId, string externalSiteUrl)
        {
            var isPageObject = false;
            var isTemplateObject = false;
            var templateResult = string.Empty;
            var pageResult = string.Empty;
            var pageKey = string.Empty;
            var mappingKey = string.Empty;

            var mappedPageId = 0;
            var mappedPageTemplateId = 0;
            var result = string.Empty;
            //HttpContext.Current.Response.Write("Getting usercall: " && userCall && "<br>")
            var call = new ObjectCall(userCall, this);

            if (siteId == SiteId)
            {
                //HttpContext.Current.Response.Write("Current site." && "<br>")
                mappedPageTemplateId = call.TemplateId;
                mappedPageId = PageId;
            }
            else
            {
                string key = $"{call.TemplateId},{siteId}";
                mappingKey = key;
                //HttpContext.Current.Response.Write("mappingTKey:" && key && "<br>")
                if (TemplateMapping.Contains(key))
                {
                    mappedPageTemplateId = (int)TemplateMapping[key];
                }
                //HttpContext.Current.Response.Write("found: " && TemplateMapping(key).ToString() && "<br>")

                //HttpContext.Current.Response.Write("mappingPKey:" && siteId && "<br>")
                if (PageMapping.Contains(siteId))
                {
                    mappedPageId = (int)PageMapping[siteId];
                }

                //HttpContext.Current.Response.Write("found: " && PageMapping(siteId).ToString() && "<br>")
            }



            if (call.WithoutTemplate)
            {
                var key = $"{mappedPageId},{userCall}".ToLowerInvariant();
                pageKey = key;
                //HttpContext.Current.Response.Write("Pkey:" && key && "<br>")
                if (PageObjects.Contains(key))
                {
                    result = PageObjects[key].ToString();
                    pageResult = result;
                    isPageObject = true;
                }
                //HttpContext.Current.Response.Write("url:" && result && "<br>")
                //HttpContext.Current.Response.Write("found:" && isPageObject && "<br>")

            }

            if (!isPageObject)
            {
                var key = $"{mappedPageTemplateId},{userCall}".ToLowerInvariant();
                var templateKey = key;
                //HttpContext.Current.Response.Write("Tkey:" && key && "<br>")
                if (TemplateObjects.Contains(key))
                {
                    result = TemplateObjects[key].ToString();
                    templateResult = result;
                    isTemplateObject = true;
                }
                //HttpContext.Current.Response.Write("url:" && result && "<br>")
                //HttpContext.Current.Response.Write("found:" && isTemplateObject && "<br>")


                if (!isTemplateObject)
                {
                    result = string.Empty;
                    if (siteId == SiteId)
                    {
                        throw new Exception(GetObjectNotFoundMessage(userCall, call.TemplateName, templateResult, pageResult, templateKey, pageKey, mappingKey));
                    }
                }
            }
            //HttpContext.Current.Response.Write("<br>")

            return result;
        }

        public string GetObjectNotFoundMessage(string userCall, string templateName, string url1, string url2, string key1, string key2, string key3)
        {
            Dump.DumpHashTable(PageObjects, key2);
            Dump.DumpHashTable(TemplateObjects, key1);
            return
                $"Object \"{userCall}\" is not found. Template: \"{templateName}\". Keys: \"{key1}\", \"{key2}\", \"{key3}\". Urls: \"{url1}\", \"{url2}\".";
        }

        public string GetInternalCall(string userCall)
        {
            var result = string.Empty;
            var pageResult = string.Empty;
            var templateResult = string.Empty;

            if (UseMultiSiteLogic)
            {
                //HttpContext.Current.Response.Write("Single site call.<br>")
                var site = new SiteInfo(SiteId, SiteUrl);
                SiteInfo[] sites = { site };
                result = GetInternalCall(userCall, sites);
            }

            var isPageObject = false;
            var isTemplateObject = false;
            var call = new ObjectCall(userCall, this);
            var key = userCall.ToLowerInvariant();
            var pageKey = key;
            var templateKey = key;

            if (UseMultiSiteLogic)
            {
                pageKey = $"{PageId},{key}";
                templateKey = $"{call.TemplateId},{key}";
            }

            if (call.WithoutTemplate)
            {

                if (PageObjects.Contains(pageKey))
                {
                    result = PageObjects[pageKey].ToString();
                    isPageObject = true;
                    pageResult = result;

                }
            }

            if (!isPageObject)
            {

                if (TemplateObjects.Contains(templateKey))
                {
                    result = TemplateObjects[templateKey].ToString();
                    isTemplateObject = true;
                    templateResult = result;
                }

                if (!isTemplateObject)
                {
                    throw new Exception(GetObjectNotFoundMessage(userCall, call.TemplateName, templateResult, pageResult, pageKey, templateKey, ""));
                }
            }


            return result;
        }

        public string GetControlUrl(string name)
        {
            return GetControlUrlWithException(name, true);
        }

        public string GetControlUrlWithException(string name, bool throwException)
        {
            var nameArr = name.Split(',');
            string controlFileName;
            string url;
            if (nameArr.Length > 1)
            {
                var externalTemplateName = nameArr[0].Trim();
                controlFileName = nameArr[1].Trim();
                url = GetControlUrl(controlFileName, true, GetTemplateFolder(externalTemplateName), PageFolder, externalTemplateName);
            }
            else
            {
                controlFileName = name.Trim();
                url = GetControlUrl(controlFileName, false);
                if (!ControlExists(url))
                {
                    url = GetControlUrl(controlFileName, true);
                }
                if (!ControlExists(url))
                {
                    url = string.Empty;
                    if (throwException)
                    {
                        throw new Exception(
                            $"Control is not found at paths: {GetControlUrl(controlFileName, false)}, {GetControlUrl(controlFileName, true)}");
                    }
                }
            }

            return url;
        }

        internal string GetControlUrl(string controlFileName, bool isTemplateObject)
        {
            return GetControlUrl(controlFileName, isTemplateObject, TemplateFolder, PageFolder, TemplateNetName);
        }

        internal string GetControlUrl(DataRowView drv)
        {
            return GetControlUrl(GetControlFileName(drv), IsTemplateObject(drv), GetTemplateFolder(drv), GetPageFolder(drv), GetNetTemplateName(drv));
        }

        internal string GetControlUrl(DataRowView drv, int siteId, string externalSiteUrl)
        {
            return GetControlUrl(GetControlFileName(drv), IsTemplateObject(drv), GetTemplateFolder(drv), GetPageFolder(drv), GetNetTemplateName(drv), siteId, externalSiteUrl);
        }

        internal string GetControlUrl(DataRowView drv, int siteId)
        {
            return GetControlUrl(drv, siteId, string.Empty);
        }

        internal string GetControlUrl(string controlFileName, bool isTemplateObject, string templateFolder, string pageFolder, string templateNetName)
        {
            return GetControlUrl(controlFileName, isTemplateObject, templateFolder, pageFolder, templateNetName, 0, string.Empty);
        }

        internal string GetControlUrl(string controlFileName, bool isTemplateObject, string templateFolder, string pageFolder, string templateNetName, int siteId, string externalSiteUrl)
        {
            var sb = new StringBuilder();
            sb.Append(GetControlCommonUrlPart(templateFolder, IsPreview, siteId, externalSiteUrl));
            sb.Append(isTemplateObject ? GetControlTemplateUrlPart(templateNetName) : GetControlPageUrlPart(pageFolder));
            sb.Append(controlFileName);
            return sb.ToString();
        }

        private string GetControlPageUrlPart(string pageFolder)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(pageFolder))
            {
                sb.Append(pageFolder);
                sb.Append("/");
            }
            sb.Append(PageControlsFolder);
            sb.Append("/");
            return sb.ToString();
        }

        private string GetControlTemplateUrlPart(string templateNetName)
        {
            var sb = new StringBuilder();
            sb.Append(TemplateControlsFolderPrefix);
            sb.Append(templateNetName);
            sb.Append("/");
            return sb.ToString();
        }

        private string GetControlCommonUrlPart(string templateFolder, bool isPreview, int siteId, string externalSiteUrl)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(externalSiteUrl))
            {
                sb.Append(externalSiteUrl);
            }
            else
            {
                sb.Append(siteId == 0 ? SiteUrl : Cnn.GetSiteUrlRel(siteId, !IsStage));
            }
            if (isPreview)
            {
                sb.Append(AdditionalFolder);
                sb.Append("/");
            }
            if (!string.IsNullOrEmpty(templateFolder))
            {
                sb.Append(templateFolder);
                sb.Append("/");
            }
            return sb.ToString();
        }

        private static bool IsTemplateObject(DataRowView drv)
        {
            return ReferenceEquals(drv["PAGE_ID"], DBNull.Value);
        }

        private static string GetNetTemplateName(DataRowView drv)
        {
            return GetNetName(drv["NET_TEMPLATE_NAME"].ToString(), drv["PAGE_TEMPLATE_ID"].ToString(), "t");
        }

        private Hashtable _pages;

        internal Hashtable Pages => _pages ?? (_pages = Cnn.GetPageHashTable());

        private Hashtable _templates;

        internal Hashtable Templates => _templates ?? (_templates = Cnn.GetTemplateHashTable());

        private Hashtable _pageObjects;

        internal Hashtable PageObjects => _pageObjects ?? (_pageObjects = IsPreview || IsStage ? Cnn.CacheManager.FillPageObjectsHashTable() : Cnn.GetPageObjectHashTable());

        private Hashtable _templateObjects;

        internal Hashtable TemplateObjects => _templateObjects ?? (_templateObjects = IsPreview || IsStage ? Cnn.CacheManager.FillTemplateObjectsHashTable() : Cnn.GetTemplateObjectHashTable());

        private Hashtable _pageMapping;

        internal Hashtable PageMapping => _pageMapping ?? (_pageMapping = Cnn.GetPageMappingHashTable());

        private Hashtable _templateMapping;

        internal Hashtable TemplateMapping => _templateMapping ?? (_templateMapping = Cnn.GetTemplateMappingHashTable());

        private DataTable _pageInfo;

        internal DataView PageInfo
        {
            get
            {
                if (_pageInfo == null)
                {
                    _pageInfo = GetPageView(PageId).ToTable();
                }
                return _pageInfo.DefaultView;
            }
        }

        private DataTable _templateInfo;

        internal DataView TemplateInfo
        {
            get
            {
                if (_templateInfo == null)
                {
                    _templateInfo = GetTemplateView(PageTemplateId).ToTable();
                }
                return _templateInfo.DefaultView;
            }
        }

        private string GetTemplateFolder(DataRowView controlDrv)
        {
            if (UseMultiSiteLogic)
            {
                return GetTemplateFolder(controlDrv["TEMPLATE_NAME"].ToString(), DBConnector.GetNumInt(controlDrv["SITE_ID"]));
            }
            return GetTemplateFolder(controlDrv["TEMPLATE_NAME"].ToString());
        }

        internal string GetTemplateFolder(string templateName, int siteId)
        {
            return GetTemplateFolder($"{siteId},{templateName}");
        }

        internal string GetTemplateFolder(string templateName)
        {
            var result = string.Empty;
            if (Templates.Contains(templateName.ToLowerInvariant()))
            {
                result = ((Template)Templates[templateName.ToLowerInvariant()]).Folder;
            }
            return result;
        }

        internal int GetTemplateId(string templateName, int siteId)
        {
            return GetTemplateId($"{siteId},{templateName}");
        }

        internal int GetTemplateId(string templateName)
        {
            var result = 0;
            if (Templates.Contains(templateName.ToLowerInvariant()))
            {
                result = ((Template)Templates[templateName.ToLowerInvariant()]).Id;
            }
            return result;
        }

        private string GetPageFolder(DataRowView controlDrv)
        {
            if (IsTemplateObject(controlDrv))
            {
                return string.Empty;
            }
            if (UseMultiSiteLogic)
            {
                return GetPageFolder(DBConnector.GetNumInt(controlDrv["PAGE_ID"]));
            }
            return PageFolder;
        }

        private string GetPageFolder(int pageId)
        {
            var result = string.Empty;
            if (Pages.Contains(pageId))
            {
                result = Pages[pageId].ToString();
            }
            return result;
        }
        #endregion

        public DBConnector Cnn { get; private set; }

        public DBConnector Cnn2 => Cnn;

        public ArrayList ObjectCallStack { get; private set; }

        // ReSharper disable once InconsistentNaming
        public int site_id
        {
            get { return SiteId; }
            set { SiteId = value; }
        }

        // ReSharper disable once InconsistentNaming
        public int page_id
        {
            get { return PageId; }
            set { PageId = value; }
        }

        // ReSharper disable once InconsistentNaming
        public int page_template_id
        {
            get { return PageTemplateId; }
            set { PageTemplateId = value; }
        }

        public Hashtable FieldValuesDictionary { get; set; }

        public Hashtable FieldNamesDictionary { get; set; }

        public string UploadUrl { get; set; }

        public string SiteUrl { get; set; }

        public string AbsoluteSiteUrl { get; set; }

        public bool AbsUploadUrl { get; set; }

        public string UploadUrlPrefix { get; set; }

        public int PublishedStatusTypeId { get; set; }

        public string PublishedStatusName { get; set; }

        private void InitTrace()
        {
            QpTrace = new QpTrace();
            QpTrace.TraceId = QpTrace.InitTrace(PageId);
            QpTrace.TraceStartText = "started";
            QpTrace.TraceString = "Page started <br>\\";
            QpTrace.TraceStartTime = DateTime.Now;
        }

        private void SetPageCharSet()
        {
            CharSet = PageId == 0 ? GetTemplateField("CHARSET") : GetPageField("CHARSET");

            if (string.IsNullOrEmpty(CharSet))
            {
                throw new Exception($"Neither page (ID = {PageId}) nor template (ID = {PageTemplateId}) is not found");
            }

            ContentEncoding = Encoding.GetEncoding(CharSet);
        }

        private void SetCachingParameters()
        {
            if (CachingType == 0)
            {
                if (SetLastModifiedHeader)
                {
                    HttpCacheability = HttpCacheability.Public;
                }
                else
                {
                    if (SendNoCacheHeaders)
                    {
                        Expires = -1000;
                        HttpCacheability = HttpCacheability.NoCache;
                        AddHeader("Pragma", "no-cache");
                        AddHeader("cache-control", "no-cache, no-store, must-revalidate");
                    }
                }
            }
            else if (CachingType == 1)
            {
                Expires = ExpiresInMinutes;
                HttpCacheRevalidation = HttpCacheRevalidation.ProxyCaches;
                ProxyMaxAge = new TimeSpan(0, ExpiresInMinutes, 0);
            }
            else if (CachingType == 2)
            {
                Expires = ExpiresInMinutes;
                HttpCacheability = HttpCacheability.Private;
                HttpCacheRevalidation = HttpCacheRevalidation.AllCaches;
                ProxyMaxAge = new TimeSpan(0, ExpiresInMinutes, 0);
            }
            else if (CachingType == 3)
            {
                Expires = ExpiresInMinutes;
                HttpCacheability = HttpCacheability.Public;
            }
        }

        public void HandleInit(IQPage qPage)
        {
            Cnn = new DBConnector { CacheManager = { StoreInDictionary = true } };
            //if (PageAssembleMode != Mode.Normal) dbConnector.CacheData = false;
            Cnn.CacheManager.SetWebSpecificInformation(qPage.QPageEssential);
            ObjectCallStack = new ArrayList();
            FillValues();
            PublishedStatusTypeId = Cnn.GetMaximumWeightStatusTypeId(site_id);
            PublishedStatusName = Cnn.GetMaximumWeightStatusTypeName(site_id);


            if (string.IsNullOrEmpty(SiteUrl))
            {
                SiteUrl = Cnn.GetSiteUrlRel(site_id, !IsStage);
            }
            if (string.IsNullOrEmpty(UploadUrl))
            {
                UploadUrl = Cnn.GetImagesUploadUrl(site_id, true);
            }
            if (string.IsNullOrEmpty(UploadUrlPrefix))
            {
                UploadUrlPrefix = Cnn.GetUploadUrlPrefix(site_id);
            }

            AbsoluteSiteUrl = Cnn.GetSiteUrl(site_id, !IsStage);


            AppendStandardHeaders();

            SetCachingParameters();

            SetPageCharSet();


            if (GenerateTrace)
            {
                InitTrace();
            }
            if (IsStage && !IsPreview)
            {
                EnableBackendOnScreen(qPage, site_id);
            }
            qPage.BeforeFirstCallInitialize();
            if (PageAssembleMode == Mode.Normal)
            {
                qPage.ShowTemplateControl(TemplateNetName + ".ascx");
            }
            else
            {
                qPage.ShowControl(TemplateNetName + ".ascx");
            }
        }

        public void HandlePreRender()
        {
            if (SetLastModifiedHeader)
            {
                HttpContext.Current.Response.Cache.SetLastModified(LastModified);
            }
        }

        public void HandleRender()
        {
            if (GenerateUrlSaving)
            {
                SaveBrowseServerSession();
                SaveUrl(SiteId.ToString());
            }
            if (GenerateTrace)
            {
                QpTrace.TraceString =
                    $"{QpTrace.TraceString}Page - {Math.Round(DateTime.Now.Subtract(QpTrace.TraceStartTime).TotalMilliseconds)}ms<br>";
                QpTrace.DoneTrace(DateTime.Now.Subtract(QpTrace.TraceStartTime), Cnn.GetAllowUserSessions(SiteId), Values);
                QpTrace.SaveTraceToDb(QpTrace.TraceString, QpTrace.TraceId);
            }
        }

        private void AppendStandardHeaders()
        {
            const string publisher = "Quantum Art's QP7.Framework 7.7";
            AddHeader("publisher", $"{publisher} {DateTime.Now:G}");
            AddHeader("P3P", "CP=\"CAO PSA OUR\"");
        }

        public void SetLastModified(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                var drArr = dt.Select("", "MODIFIED DESC");
                var tableLastModified = Convert.ToDateTime(drArr[0]["MODIFIED"]);
                if (!IsLastModifiedDynamic || DateTime.Compare(tableLastModified, LastModified) > 0)
                {
                    LastModified = tableLastModified;
                    IsLastModifiedDynamic = true;
                }
            }
        }

        public void EnableBackendOnScreen(IQPage qPage, int siteId)
        {
            if (QScreen.SessionEnabled())
            {
                qPage.IsStage = true;
                qPage.QScreen = new QScreen();
                qPage.QScreen.GetBackendAuthentication();
                qPage.QScreen.OnFlyObjCount = 0;
                if (qPage.site_id > 0)
                {
                    QScreen.SetSiteBorderModes(Cnn.GetSite(siteId));
                }
            }
        }

        public bool GenerateUrlSaving
        {
            get
            {
                var flag = GetBooleanPageField("DISABLE_BROWSE_SERVER");
                return IsStage && flag.HasValue && !flag.Value;
            }
        }

        public void SaveBrowseServerSession()
        {
            object value = HttpContext.Current.Request.QueryString["browse_server_session_id"];
            int intValue;
            if (value != null && int.TryParse(value.ToString(), out intValue) && intValue != -1)
            {
                HttpContext.Current.Session["BrowseServerSessionID"] = value.ToString();
            }
        }

        private void InternalInitialize(int siteId, int pageId, int pageTemplateId, string uploadUrl, string uploadUrlPrefix, string siteUrl, string pageFileName, string templateNetName, string pageFolder)
        {

            page_id = pageId;
            page_template_id = pageTemplateId;
            site_id = siteId;
            UploadUrl = uploadUrl;
            UploadUrlPrefix = uploadUrlPrefix;
            SiteUrl = siteUrl;
            PageControlsFolder = PageControlsFolderPrefix + pageFileName.Replace(".", "_") + "/";
            TemplateNetName = templateNetName;
            PageFolder = pageFolder;

            HandleInit(null);
        }

        public void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName)
        {
            Initialize(siteId, uploadUrl, siteUrl, pageFileName, templateNetName, "");
        }

        public void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName, string pageFolder)
        {
            InternalInitialize(siteId, 0, 0, uploadUrl, null, siteUrl, pageFileName, templateNetName, pageFolder);
        }

        public void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName, string pageFolder, Hashtable pageObjects, Hashtable templates)
        {
            Initialize(siteId, uploadUrl, siteUrl, pageFileName, templateNetName, pageFolder);
        }

        public void Initialize(int siteId, int pageId, int pageTemplateId, string uploadUrl, string uploadUrlPrefix, string siteUrl, string pageFileName, string pageFolder)
        {
            InternalInitialize(siteId, pageId, pageTemplateId, uploadUrl, uploadUrlPrefix, siteUrl, pageFileName, "", pageFolder);
        }
        public void Initialize(int siteId, int pageId, int pageTemplateId, string pageFileName, string pageFolder)
        {
            Initialize(siteId, pageId, pageTemplateId, "", "", "", pageFileName, pageFolder);
        }

        public void Initialize(int siteId)
        {
            Initialize(siteId, 0, 0, "", "");
        }

        public void FillValues()
        {
            _valuesCollection = new Hashtable();
            _objectValuesCollection = new Hashtable();

            var ctx = HttpContext.Current;

            foreach (string item in ctx.Request.QueryString)
            {
                AddValue(item, ctx.Request.QueryString[item]);
            }

            foreach (string item in ctx.Request.Form)
            {
                AddValue(item, ctx.Request.Form[item]);
            }

            foreach (string item in ctx.Request.Files)
            {
                var httpPostedFile = ctx.Request.Files[item];
                if (httpPostedFile != null)
                {
                    AddValue(item, Cnn.ShortFileName(httpPostedFile.FileName));
                }
            }
        }

        public void AddValue(string key, object value)
        {
            if (key != null)
            {
                key = Strings.LCase(key);
                if (key != null && _valuesCollection.ContainsKey(key))
                {
                    _valuesCollection.Remove(key);
                }
                if (key != null)
                {
                    _valuesCollection.Add(key, value);
                }
            }
        }

        public void AddObjectValue(string key, object value)
        {
            key = Strings.LCase(key);
            if (key != null && _objectValuesCollection.ContainsKey(key))
            {
                _objectValuesCollection.Remove(key);
            }
            if (key != null)
            {
                _objectValuesCollection.Add(key, value);
            }
        }

        public string DirtyValue(string key)
        {
            string functionReturnValue;
            key = Strings.LCase(key);
            if (key != null && _valuesCollection.ContainsKey(key))
            {
                functionReturnValue = _valuesCollection[key].ToString();
            }
            else
            {
                functionReturnValue = key != null && _objectValuesCollection.ContainsKey(key) ? _objectValuesCollection[key].ToString() : "";
            }
            return functionReturnValue;
        }

        public string Value(string key)
        {
            return DirtyValue(key).Replace("'", "");
        }

        public string Value(string key, string defaultValue)
        {
            var resValue = DirtyValue(key).Replace("'", "");
            if (string.IsNullOrEmpty(resValue))
            {
                resValue = defaultValue;
            }
            return resValue;
        }

        public long NumValue(string key)
        {
            long result;
            if (Information.IsNumeric(DirtyValue(key)))
            {
                var temp = double.Parse(DirtyValue(key).Replace(',', '.'), CultureInfo.InvariantCulture);
                result = Convert.ToInt64(temp);
            }
            else
            {
                result = 0;
            }
            return result;
        }

        public string StrValue(string key)
        {
            var result = DirtyValue(key);
            return result?.Replace("'", "''");
        }

        public string InternalStrValue(string valueName)
        {
            var ctx = HttpContext.Current;
            if (string.IsNullOrEmpty(ctx.Request.QueryString[valueName]) && string.IsNullOrEmpty(ctx.Request.Form[valueName]) && _valuesCollection.ContainsKey(valueName.ToLowerInvariant()))
            {
                return StrValue(valueName);
            }
            return string.Empty;
        }

        public Hashtable Values => _valuesCollection;

        public void CallStackOverflow()
        {
            var ctx = HttpContext.Current;

            ctx.Response.Write("<b>Object Call Stack Overflow<br>Call Stack: <br></b>");

            foreach (string item in ObjectCallStack)
            {
                ctx.Response.Write(item + "<br>");
            }
            ctx.Response.End();
        }

        public static string GetSimpleContainerFilterExpression(string filterSql)
        {
            if (IsExpressionEmpty(filterSql))
            {
                return string.Empty;
            }

            return $" and ({filterSql})";
        }

        public static bool IsExpressionEmpty(string expression)
        {
            return string.IsNullOrEmpty(expression) || Strings.Trim(expression).Length == 0;
        }

        public static bool IsOrderSqlValid(string orderSql)
        {
            return !IsExpressionEmpty(orderSql);
        }

        public static string GetSimpleContainerOrderExpression(string staticSql, string dynamicSql)
        {
            if (IsOrderSqlValid(dynamicSql))
            {
                return dynamicSql;
            }
            if (IsOrderSqlValid(staticSql))
            {
                return staticSql;
            }
            return " c.modified desc";
        }

        public static string CleanSql(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            int count;
            do
            {
                text = CleanRegex.Replace(text, "");
                var matches = CleanRegex.Matches(text);
                count = matches.Count;
            }
            while (count != 0);
            return text;
        }

        public string Field(string key)
        {
            return FieldValuesDictionary.ContainsKey(key.ToLowerInvariant()) ?
                FieldValuesDictionary[key.ToLowerInvariant()].ToString() : key;
        }

        public void AddHeader(string key, string value)
        {
            HttpContext.Current.Response.AddHeader(key, value);
        }

        public void SaveUrl(string siteId)
        {
            var idObj = HttpContext.Current.Session["BrowseServerSessionID"];
            if (idObj != null && Information.IsNumeric(idObj))
            {
                var url = HttpContext.Current.Request.ServerVariables["URL"];
                var queryString = HttpContext.Current.Request.ServerVariables["QUERY_STRING"];
                queryString = string.Join("&", queryString.Split('&').Where(s => !string.Equals(s, $"browse_server_session_id={idObj}")).ToArray());
                url = string.IsNullOrEmpty(queryString) ? url : $"{url}?{queryString}";
                string sql = $"if exists(select * from sysobjects where name = N'VE_URL') exec sp_executesql N'if exists(select * from ve_url where sid = @sid) update ve_url set url = @url where sid = @sid else insert into ve_url(sid, url) values(@sid, @url)', N'@url nvarchar(1024), @sid numeric', @url = '{GetSiteDns(siteId)}{url}', @sid = {idObj}";
                Cnn.ProcessData(sql);
            }
        }

        public string GetSiteDns(string siteId)
        {
            return "http://" + Cnn.GetDns(Convert.ToInt32(siteId), false);
        }

        #region "form.inc"
        public void RemoveContentItem(int contentItemId)
        {
            Cnn.SendNotification(contentItemId, NotificationEvent.Remove);
            Cnn.DeleteContentItem(contentItemId);
        }

        public void DeleteContentItem()
        {
            RemoveContentItem(int.Parse(Value("content_item_id")));
            AddValue("content_item_id", "");
        }

        public int GetContentId(string contentName)
        {
            return Cnn.GetContentId(site_id, contentName);
        }

        public int GetContentVirtualType(int contentId)
        {
            return Cnn.GetContentVirtualType(contentId);
        }

        public string FieldName(string contentName, string fieldName)
        {
            return "field_" + FieldId(contentName, fieldName);
        }

        //getting field id for field name
        public int FieldId(string contentName, string fieldName)
        {
            return Cnn.FieldID(site_id, contentName, fieldName);
        }

        public string InputName(string contentName, string fieldName)
        {
            return Cnn.InputName(site_id, contentName, fieldName);
        }

        public bool CheckMaxLength(string str, int maxlength)
        {
            return Strings.Len(Strings.Trim(str)) <= maxlength;
        }

        public string ReplaceHtml(string str)
        {
            return str.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public void SendNotification(string notificationOn, int contentItemId, string notificationEmail)
        {
            Cnn.SendNotification(site_id, notificationOn, contentItemId, notificationEmail, !IsStage);
        }

        public string GetSiteUrl()
        {
            return Cnn.GetSiteUrl(site_id, !IsStage);
        }

        public string GetActualSiteUrl()
        {
            return Cnn.GetActualSiteUrl(site_id);
        }

        public string GetContentItemLinkIDs(string linkFieldName, long itemId)
        {
            return Cnn.GetContentItemLinkIDs(linkFieldName, itemId);
        }

        public string GetContentItemLinkIDs(string linkFieldName, string itemId)
        {
            return Cnn.GetContentItemLinkIDs(linkFieldName, itemId);
        }

        public string GetContentItemLinkQuery(string linkFieldName, long itemId)
        {
            return Cnn.GetContentItemLinkQuery(linkFieldName, itemId);
        }

        public string GetContentItemLinkQuery(string linkFieldName, string itemId)
        {
            return Cnn.GetContentItemLinkQuery(linkFieldName, itemId);
        }

        public string GetLinkIDs(string linkFieldName)
        {
            return GetContentItemLinkIDs(linkFieldName, long.Parse(Field("content_item_id")));
        }

        public int GetLinkIdForItem(string linkFieldName, int itemId)
        {
            return Cnn.GetLinkIDForItem(linkFieldName, itemId);
        }

        public string GetContentFieldValue(int itemId, string fieldName)
        {
            return Cnn.GetContentFieldValue(itemId, fieldName);
        }

        public int AddFormToContentWithoutNotification(string contentName, string statusName)
        {
            return AddFormToContentWithoutNotification(contentName, statusName, 0);
        }

        public int AddFormToContentWithoutNotification(string contentName, string statusName, int contentItemId)
        {
            var files = HttpContext.Current.Request.Files;
            var newItemId = Cnn.AddFormToContent(site_id, contentName, statusName, ref _valuesCollection, ref files, contentItemId);
            AddValue("new_content_item_id", newItemId);
            return newItemId;
        }

        public int AddFormToContent(string contentName, string statusName)
        {
            return AddFormToContent(contentName, statusName, 0);
        }

        public int AddFormToContent(string contentName, string statusName, int contentItemId)
        {
            var files = HttpContext.Current.Request.Files;
            var newItemId = Cnn.AddFormToContent(site_id, contentName, statusName, ref _valuesCollection, ref files, contentItemId);
            AddValue("new_content_item_id", newItemId);
            var actualSiteId = 0;
            Cnn.GetDynamicContentId(contentName, 0, site_id, ref actualSiteId);
            Cnn.SendNotification(newItemId, NotificationEvent.Create);
            return newItemId;
        }

        public void UpdateContentItemField(string contentName, string fieldName, int contentItemId)
        {
            UpdateContentItemField(contentName, fieldName, contentItemId, false);
        }

        public void UpdateContentItemField(string contentName, string fieldName, int contentItemId, bool withNotification)
        {
            var files = HttpContext.Current.Request.Files;
            Cnn.UpdateContentItemField(site_id, contentName, fieldName, contentItemId, ref _valuesCollection, ref files);
            if (withNotification)
            {
                var actualSiteId = 0;
                Cnn.GetDynamicContentId(contentName, 0, site_id, ref actualSiteId);
                Cnn.SendNotification(contentItemId, NotificationEvent.Modify);
            }
        }

        public void UpdateContentItem(bool updateEmpty, string statusName)
        {
            UpdateContentItem(updateEmpty, statusName, true);
        }

        public void UpdateContentItemWithoutNotification(bool updateEmpty, string statusName)
        {
            UpdateContentItem(updateEmpty, statusName, false);
        }

        public void UpdateContentItem(bool updateEmpty, string statusName, bool withNotification)
        {
            var contentItemId = int.Parse(Value("content_item_id"));
            var dt = Cnn.GetRealData("select c.site_id, c.content_id, ci.status_type_id from content_item ci inner join content as c on ci.content_id = c.content_id where ci.content_item_id = " + contentItemId);
            if (dt.Rows.Count > 0)
            {
                var contentId = DBConnector.GetNumInt(dt.Rows[0]["content_id"]);
                var actualSiteId = DBConnector.GetNumInt(dt.Rows[0]["site_id"]);
                var oldStatusTypeId = DBConnector.GetNumInt(dt.Rows[0]["status_type_id"]);
                var files = HttpContext.Current.Request.Files;
                Cnn.UpdateContentItem(actualSiteId, contentId, ref _valuesCollection, ref files, contentItemId, updateEmpty, statusName);

                Cnn.GetRealData("select ci.status_type_id from content_item ci where ci.content_item_id = " + contentItemId);
                var newStatusTypeId = DBConnector.GetNumInt(dt.Rows[0]["status_type_id"]);

                if (withNotification)
                {
                    Cnn.SendNotification(contentItemId, NotificationEvent.Modify);
                    if (oldStatusTypeId != newStatusTypeId)
                    {
                        Cnn.SendNotification(contentItemId, NotificationEvent.StatusChanged);
                    }
                }

            }
        }

        public string GetContentUploadUrl(string contentName)
        {
            return Cnn.GetContentUploadUrl(site_id, contentName);
        }

        public string GetContentUploadUrlById(int contentId)
        {
            return Cnn.GetContentUploadUrlByID(Cnn.GetSiteIdByContentId(contentId), contentId);
        }

        public string GetContentName(int contentId)
        {
            return Cnn.GetContentName(contentId);
        }

        public string GetFieldUploadUrl(string fieldName, int contentId)
        {
            return Cnn.GetFieldUploadUrl(fieldName, contentId);
        }
        #endregion

        public DataTable GetUsersByItemID_And_Permission(int itemId, int permissionLevel)
        {
            return Cnn.GetCachedData("EXEC qp_GetUsersByItemID_And_Permission " + itemId + "," + permissionLevel);
        }
    }
}

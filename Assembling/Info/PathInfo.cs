using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Quantumart.QP8.Assembling.Info
{
    public class PathInfo
    {
        private readonly AssembleInfo _info;

        public string LivePath { get; }

        public string StagePath { get; }

        public string BaseAssemblePath { get; }

        public string BaseUploadPath { get; }

        public string UploadPath => BaseUploadPath + "\\images";

        public string TemplateFolder { get; }

        public string PageFolder { get; }

        public Uri PageUrl => new Uri(PageFolder.Replace("\\", "/"));

        public string TemplateFolderPath { get; private set; }

        public string PageFolderPath { get; private set; }


        public string FullPagePath { get; private set; }

        public string FullPageCodePath => FullPagePath + "." + AssembleInfo.PageLanguage;

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string UploadUrlPrefix { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string BaseUploadUrl { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string UploadUrl => BaseUploadUrl + "images";

        public string ActualDns { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string SiteUrl { get; private set; }

        public string PageControlsPath { get; private set; }

        public string TemplateControlsPath { get; private set; }

        public static string PageControlsFolderPrefix => "page_controls__";

        public static string TemplateControlsFolderPrefix => "template_controls__";

        public string PageControlsFolderName => PageControlsFolderPrefix + _info.PageFileName.Replace(".", "_");

        public string TemplateControlsFolderName => GetTemplateControlsFolderName(_info.NetTemplateName);

        private static string GetTemplateControlsFolderName(string name)
        {
            return TemplateControlsFolderPrefix + name.Replace(".", "_");
        }

        public bool IsLive
        {
            get
            {
                if (_info.Controller.UseFixedLocation)
                {
                    return _info.Controller.FixedLocation == AssembleLocation.Live;
                }

                return _info.IsLive;
            }
        }

        public PathInfo(AssembleInfo assembleInfo)
        {
            _info = assembleInfo;
            if (_info.Data.Rows.Count == 0)
            {
                throw new InvalidOperationException("Empty Assemble Info");
            }

            LivePath = _info.GetString("LIVE_DIRECTORY");
            StagePath = _info.GetString("STAGE_DIRECTORY");

            if (_info.ForceLive)
            {
                BaseAssemblePath = LivePath;
            }
            else if (!IsLive)
            {
                BaseAssemblePath = StagePath;
            }
            else if (_info.ForceTestDirectory)
            {
                BaseAssemblePath = _info.GetString("TEST_DIRECTORY");
            }
            else
            {
                BaseAssemblePath = LivePath;
            }

            if (_info.Mode == AssembleMode.Preview)
            {
                BaseAssemblePath += "\\temp\\preview\\objects\\";
            }
            else if (_info.Mode == AssembleMode.PreviewAll || _info.Mode == AssembleMode.PreviewById)
            {
                BaseAssemblePath += "\\temp\\preview\\articles\\";
            }
            else if (_info.Mode == AssembleMode.Notification)
            {
                BaseAssemblePath += "\\qp_notifications\\";
            }
            else if (_info.Mode == AssembleMode.GlobalCss)
            {
                BaseAssemblePath += "\\temp\\css\\";
            }

            BaseUploadPath = _info.FirstDataRow["UPLOAD_DIR"].ToString();
            TemplateFolder = _info.FirstDataRow["TEMPLATE_FOLDER"].ToString();
            PageFolder = _info.Data.Columns["PAGE_FOLDER"] != null ? _info.FirstDataRow["PAGE_FOLDER"].ToString() : "";

            CalculateUrls();
            CalculateFullPath();
        }

        private void CalculateFullPath()
        {
            var sb = new StringBuilder(BaseAssemblePath);
            sb.Append("\\");
            if (!string.IsNullOrEmpty(TemplateFolder))
            {
                sb.Append(TemplateFolder);
                sb.Append("\\");
            }

            TemplateFolderPath = sb.ToString();
            if (!string.IsNullOrEmpty(PageFolder))
            {
                sb.Append(PageFolder);
                sb.Append("\\");
            }

            PageFolderPath = sb.ToString();
            FullPagePath = (_info.IsAssembleFormatMode ? BaseAssemblePath : PageFolderPath) + _info.PageFileName;
            TemplateControlsPath = TemplateFolderPath + TemplateControlsFolderName + "\\";
            PageControlsPath = PageFolderPath + PageControlsFolderName + "\\";
        }

        private void CalculateUrls()
        {
            ActualDns = GetActualDns();
            SiteUrl = GetSiteUrl();

            if (_info.GetNumericBoolean("USE_ABSOLUTE_UPLOAD_URL"))
            {
                BaseUploadUrl = _info.FirstDataRow["UPLOAD_URL_PREFIX"] + _info.FirstDataRow["UPLOAD_URL"].ToString();
                UploadUrlPrefix = _info.FirstDataRow["UPLOAD_URL_PREFIX"].ToString();
            }
            else
            {
                BaseUploadUrl = _info.FirstDataRow["UPLOAD_URL"].ToString();
                UploadUrlPrefix = "";
            }

        }

        private string GetActualDns()
        {
            if (_info.IsLive)
            {
                return _info.FirstDataRow["DNS"].ToString();
            }

            return _info.FirstDataRow["STAGE_DNS"] == DBNull.Value
                ? _info.FirstDataRow["DNS"].ToString()
                : _info.FirstDataRow["STAGE_DNS"].ToString();
        }
        private string GetSiteUrl()
        {
            return IsLive
                ? _info.FirstDataRow["LIVE_VIRTUAL_ROOT"].ToString()
                : _info.FirstDataRow["STAGE_VIRTUAL_ROOT"].ToString();
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public string GetContentUploadUrl(int contentId)
        {
            return BaseUploadUrl + "contents/" + contentId;
        }

        public string CacheFilePath => string.Format(CultureInfo.InvariantCulture, "{0}\\dependencies", BaseAssemblePath);

        public string FullCacheFileName(string shortFileName)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", CacheFilePath, shortFileName);
        }

        public string StructureCacheFile => FullCacheFileName("structure.dep");

        public string AllStructureCacheFile => FullCacheFileName("all_structure.dep");

        public string TemplateObjectCacheFile => FullCacheFileName("templates.dep");
        public string AllTemplateObjectsCacheFile => FullCacheFileName("all_templates.dep");

        public string PageObjectCacheFile => FullCacheFileName(_info.PageId + ".dep");

        public string AllPageObjectsCacheFile => FullCacheFileName("all_pages.dep");

        internal string GetExternalPath(int templateId)
        {
            string templateFolder = "", controlsFolderName = "";
            var dv = new DataView(_info.Templates)
            {
                RowFilter = "PAGE_TEMPLATE_ID = " + templateId.ToString(CultureInfo.InvariantCulture)
            };

            if (dv.Count > 0)
            {
                templateFolder = dv[0]["TEMPLATE_FOLDER"].ToString();
                controlsFolderName = GetTemplateControlsFolderName(AssembleInfo.GetNetName(dv[0]["NET_TEMPLATE_NAME"].ToString(), templateId.ToString(CultureInfo.InvariantCulture), "t"));
            }

            var sb = new StringBuilder(BaseAssemblePath);
            sb.Append(templateFolder);
            sb.Append("\\");
            sb.Append(controlsFolderName);
            sb.Append("\\");
            dv.Dispose();
            return sb.ToString();
        }
    }
}

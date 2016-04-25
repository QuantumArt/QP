using System;
using System.Text;

namespace Quantumart.QPublishing.Database
{

    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {
        #region Paths and urls

        internal static string CombineWithoutDoubleSlashes(string first, string second)
        {
            // it is allowed for url to start with '//'
            // see schemaless urls

            if (String.IsNullOrEmpty(second))
                return first;
            else
            {
                var sb = new StringBuilder();

                sb.Append(first.Replace(@":/", @"://").TrimEnd('/'));
                sb.Append("/");
                sb.Append(second.Replace("//", "/").TrimStart('/'));

                return sb.ToString();
            }
        }

        #region GetImagesUploadUrl

        public string GetImagesUploadUrlRel(int siteId)
        {
            return GetUploadUrlRel(siteId) + "images";
        }

        public string GetImagesUploadUrl(int siteId)
        {
            return GetUploadUrl(siteId) + "images";
        }

        public string GetImagesUploadUrl(int siteId, bool asShortAsPossible)
        {
            return GetUploadUrl(siteId, asShortAsPossible, false) + "images";
        }

        public string GetImagesUploadUrl(int siteId, bool asShortAsPossible, bool removeSchema)
        {
            return GetUploadUrl(siteId, asShortAsPossible, removeSchema) + "images";
        }

        #endregion

        #region GetUploadDir

        public string GetUploadDir(int siteId)
        {
            var site = GetSite(siteId);
            return site == null ? String.Empty : site.UploadDir;
        }

        #endregion

        #region GetUploadUrl

        public string GetUploadUrl(int siteId)
        {
            return GetUploadUrl(siteId, false);
        }

        public string GetUploadUrlRel(int siteId)
        {
            var site = GetSite(siteId);
            return site == null ? String.Empty : site.UploadUrl;
        }

        public string GetUploadUrl(int siteId, bool asShortAsPossible)
        {
            return GetUploadUrl(siteId, asShortAsPossible, false);
        }

        public string GetUploadUrl(int siteId, bool asShortAsPossible, bool removeSchema)
        {
            var site = GetSite(siteId);
            var sb = new StringBuilder();
            if (site != null)
            {
                var prefix = GetUploadUrlPrefix(siteId);
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (removeSchema)
                    {
                        prefix = ConvertUrlToSchemaInvariant(prefix);
                    }

                    sb.Append(prefix);
                }
                else
                {
                    if (!asShortAsPossible)
                    {
                        sb.Append(!removeSchema ? "http://" : "//");

                        sb.Append(GetDns(siteId, true));
                    }
                }
                sb.Append(GetUploadUrlRel(siteId));
            }
            return sb.ToString();
        }

        private static string ConvertUrlToSchemaInvariant(string prefix)
        {
            if (prefix.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                prefix = "//" + prefix.Substring(7);
            }
            return prefix;
        }

        public string GetUploadUrlPrefix(int siteId)
        {
            var site = GetSite(siteId);
            return site != null && site.UseAbsoluteUploadUrl ? site.UploadUrlPrefix : string.Empty;
        }

        #endregion

        #region GetSiteUrl

        public string GetActualSiteUrl(int siteId)
        {
            return GetSiteUrl(siteId, IsLive(siteId));
        }

        public string GetSiteUrl(int siteId, bool isLive)
        {
            var site = GetSite(siteId);
            var sb = new StringBuilder();
            if (site != null)
            {
                sb.Append("http://");
                sb.Append(GetDns(siteId, isLive));
                sb.Append(GetSiteUrlRel(siteId, isLive));
            }
            return sb.ToString();
        }

        public string GetSiteUrlRel(int siteId, bool isLive)
        {
            var site = GetSite(siteId);
            return site == null ? String.Empty : (isLive ? site.LiveVirtualRoot : site.StageVirtualRoot);
        }

        #endregion

        #region GetDns

        public string GetActualDns(int siteId)
        {
            return GetDns(siteId, IsLive(siteId));
        }

        public string GetDns(int siteId, bool isLive)
        {
            var site = GetSite(siteId);
            return site == null ? String.Empty : (isLive || string.IsNullOrEmpty(site.StageDns) ? site.Dns : site.StageDns);
        }

        #endregion

        #region GetSiteLibraryDirectory

        public string GetSiteLibraryDirectory(int siteId)
        {
            return GetUploadDir(siteId) + "\\images";
        }

        #endregion

        #region GetSiteDirectory

        public string GetSiteDirectory(int siteId, bool isLive)
        {
            return GetSiteDirectory(siteId, isLive, false);
        }

        public string GetSiteDirectory(int siteId, bool isLive, bool isTest)
        {
            var site = GetSite(siteId);
            if (site == null) return String.Empty;

            if (isLive && isTest)
                return String.IsNullOrEmpty(site.TestDirectory) ? site.TestDirectory : String.Empty;
            else if (isLive)
                return site.LiveDirectory;
            else
                return site.StageDirectory;
        }

        public string GetSiteLiveDirectory(int siteId)
        {
            return GetSiteDirectory(siteId, true);
        }

        #endregion

        #region Content Library

        public string GetContentLibraryDirectory(int siteId, int contentId)
        {
            return GetUploadDir(siteId) + "\\contents\\" + contentId;
        }

        public string GetContentLibraryDirectory(int contentId)
        {
            return GetContentLibraryDirectory(GetSiteIdByContentId(contentId), contentId);
        }

        #endregion

        #region Content URL

        public string GetContentUploadUrl(int siteId, string contentName)
        {
            var targetSiteId = 0;
            var contentId = GetDynamicContentId(contentName, 0, siteId, ref targetSiteId);
            if (targetSiteId == 0) targetSiteId = siteId;
            return GetContentUploadUrlByID(targetSiteId, contentId);
        }

        // ReSharper disable once InconsistentNaming
        public string GetContentUploadUrlByID(int siteId, long contentId)
        {
            return GetContentUploadUrlByID(siteId, contentId, true);
        }

        // ReSharper disable once InconsistentNaming
        public string GetContentUploadUrlByID(int siteId, long contentId, bool asShortAsPossible)
        {
            return GetContentUploadUrlByID(siteId, contentId, asShortAsPossible, false);
        }

        // ReSharper disable once InconsistentNaming
        public string GetContentUploadUrlByID(int siteId, long contentId, bool asShortAsPossible, bool removeSchema)
        {
            var site = GetSite(siteId);
            var sb = new StringBuilder();
            if (site != null)
            {
                sb.Append(GetUploadUrl(siteId, asShortAsPossible, removeSchema));
                if (sb[sb.Length - 1] != '/')
                {
                    sb.Append("/");
                }
                sb.Append("contents/");
                sb.Append(contentId);
            }
            return sb.ToString();
        }

        #endregion

        #region Field Directory

        private string GetFieldSubFolder(int attrId, bool revertSlashes)
        {
            var result = GetContentAttributeObject(attrId).SubFolder;
            if (!String.IsNullOrEmpty(result))
            {
                result = @"\" + result;
                if (revertSlashes) result = result.Replace(@"\", @"/");
            }
            return result;
        }

        public string GetFieldSubFolder(int attrId)
        {
            return GetFieldSubFolder(attrId, false);
        }

        public string GetDirectoryForFileAttribute(int attrId)
        {
            var attr = GetContentAttributeObject(attrId);
            if (attr == null)
                throw new Exception("No File/Image Attribute found with attribute_id=" + attrId.ToString());
            else
            {
                var baseDir = attr.UseSiteLibrary ? GetSiteLibraryDirectory(attr.SiteId) : GetContentLibraryDirectory(attr.SiteId, attr.ContentId);
                return baseDir + GetFieldSubFolder(attrId);
            }
        }

        #endregion

        #region Field URL

        public string GetFieldSubUrl(int attrId)
        {
            return GetFieldSubFolder(attrId, true);
        }

        public string GetFieldUploadUrl(string fieldName, Int32 contentId)
        {
            return GetFieldUploadUrl(0, fieldName, contentId);
        }

        public string GetFieldUploadUrl(int siteId, string fieldName, int contentId)
        {
            var fieldId = FieldId(contentId, fieldName);
            return GetUrlForFileAttribute(fieldId);

        }

        public string GetUrlForFileAttribute(int fieldId)
        {
            return GetUrlForFileAttribute(fieldId, true);
        }

        public string GetUrlForFileAttribute(int fieldId, bool asShortAsPossible)
        {
            return GetUrlForFileAttribute(fieldId, asShortAsPossible, false);
        }

        public string GetUrlForFileAttribute(int fieldId, bool asShortAsPossible, bool removeSchema)
        {
            if (fieldId == 0)
                return String.Empty;
            else
            {
                var attr = GetContentAttributeObject(fieldId);
                if (attr == null)
                    return String.Empty;
                else
                {
                    int sourceContentId, sourceFieldId;
                    bool useSiteLibrary;
                    if (attr.SourceAttribute == null)
                    {
                        sourceContentId = attr.ContentId;
                        sourceFieldId = attr.Id;
                        useSiteLibrary = attr.UseSiteLibrary;
                    }
                    else
                    {
                        sourceContentId = attr.SourceAttribute.ContentId;
                        sourceFieldId = attr.SourceAttribute.Id;
                        useSiteLibrary = attr.SourceAttribute.UseSiteLibrary;
                    }

                    var baseUrl = useSiteLibrary ? GetImagesUploadUrl(attr.SiteId, asShortAsPossible, removeSchema) : GetContentUploadUrlByID(attr.SiteId, sourceContentId, asShortAsPossible, removeSchema);
                    return CombineWithoutDoubleSlashes(baseUrl, GetFieldSubUrl(sourceFieldId));
                }
            }
        }

        #endregion

        #endregion
    }
}

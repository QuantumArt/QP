namespace Quantumart.QP8.BLL.Helpers
{
    public class PlaceHolderHelper
    {
        private const string UploadUrlPlaceHolder = "<%=upload_url%>";
        private const string SiteUrlPlaceHolder = "<%=site_url%>";

        public static string ReplacePlaceHoldersToUrls(Site site, string value) => string.IsNullOrEmpty(value)
            ? value
            : value.Replace(UploadUrlPlaceHolder, site.ImagesLongUploadUrl).Replace(SiteUrlPlaceHolder, site.CurrentUrl);

        public static string ReplaceUrlsToPlaceHolders(Site site, string value) => string.IsNullOrEmpty(value)
            ? value
            : value.Replace(site.ImagesLongUploadUrl, UploadUrlPlaceHolder).Replace(site.StageUrl, SiteUrlPlaceHolder).Replace(site.LiveUrl, SiteUrlPlaceHolder);
    }
}

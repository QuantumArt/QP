using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesParams : IMultistepActionSettings
    {
        public int SiteId { get; }
        public int ContentId { get; }

        public ImportArticlesParams(int siteId, int contentId)
        {
            ContentId = contentId;
            SiteId = siteId;
        }

        public int StagesCount => 3;

        public string UploadPath => PathHelper.GetUploadPath();
    }
}

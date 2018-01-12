namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesParams : IMultistepActionSettings
    {
        public int SiteId;

        public int ContentId;

        // ReSharper disable once InconsistentNaming
        public int[] IDs;

        public bool IsArchive;

        public ExportArticlesParams(int siteId, int contentId, int[] ids)
        {
            SiteId = siteId;
            ContentId = contentId;
            IDs = ids;
        }

        public ExportArticlesParams(int siteId, int contentId, int[] ids, bool isArchive)
        {
            SiteId = siteId;
            ContentId = contentId;
            IDs = ids;
            IsArchive = isArchive;
        }

        public int StagesCount => 2;

        public bool AllowAction => true;
    }
}

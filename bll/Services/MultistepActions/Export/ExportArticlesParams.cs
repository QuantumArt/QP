namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesParams : IMultistepActionSettings
    {
        public int SiteId;
        public int ContentId;
        public int[] IDs;

        public ExportArticlesParams(int siteId, int contentId, int[] ids)
        {
            SiteId = siteId;
            ContentId = contentId;
            IDs = ids;
        }

        public int StagesCount => 2;

        public bool AllowAction => true;
    }
}

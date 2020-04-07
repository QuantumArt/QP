using Newtonsoft.Json;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesParams : IMultistepActionSettings
    {
        public int SiteId;

        public int ContentId;

        [JsonProperty("IDs")]
        public int[] Ids;

        public bool IsArchive;

        public ExportArticlesParams(int siteId, int contentId, int[] ids)
        {
            SiteId = siteId;
            ContentId = contentId;
            Ids = ids;
        }

        public ExportArticlesParams(int siteId, int contentId, int[] ids, bool isArchive)
        {
            SiteId = siteId;
            ContentId = contentId;
            Ids = ids;
            IsArchive = isArchive;
        }

        public int StagesCount => 2;

        public bool AllowAction => true;
    }
}

using Newtonsoft.Json;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesParams : IMultistepActionSettings
    {
        public int SiteId { get; }

        public int ContentId { get; }

        [JsonProperty("IDs")]
        public int[] Ids { get; }

        public bool IsArchive { get; }

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

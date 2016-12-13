using System.Linq;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesParams : IMultistepActionSettings
    {
        public int SiteId;
        public int ContentId;

        public ImportArticlesParams(int siteId, int contentId)
        {
            ContentId = contentId;
            SiteId = siteId;
        }

        public int StagesCount => 3;

        public string UploadPath => $"{SiteRepository.GetById(SiteId).UploadDir}\\contents\\{ContentId}\\";

        public int BlockedFieldId
        {
            get
            {
                return ContentRepository.GetById(ContentId).Fields.FirstOrDefault(s => s.IsClassifier || s.Aggregated)?.Id ?? 0;
            }
        }
    }
}

using System.IO;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;

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

        public string UploadPath => GetUploadPath(SiteRepository.GetById(SiteId), ContentId);

        public static string GetUploadPath(Site site, int contentId)
        {
            var sb = new StringBuilder();
            sb.Append(QPConfiguration.TempDirectory);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(QPContext.CurrentCustomerCode);
            sb.Append(Path.DirectorySeparatorChar);
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesParams : IMultistepActionSettings
    {
        public int SiteId;
        public int ContentId;
        public ImportArticlesParams(int siteId, int contentId)
        {
            this.ContentId = contentId;
            this.SiteId = siteId;
        }
               
        public int StagesCount
        {
            get
            {
                return 3;
            }
        }

        public string UploadPath
        {
            get
            {
                return String.Format("{0}\\contents\\{1}\\", SiteRepository.GetById(this.SiteId).UploadDir, this.ContentId);
            }
        }

        public int BlockedFieldId
        {
            get
            {
                Field field = ContentRepository.GetById(this.ContentId).Fields.Where(s => s.IsClassifier || s.Aggregated).FirstOrDefault();
                return (field == null) ? 0 : field.Id;
            }
        }
    }
}

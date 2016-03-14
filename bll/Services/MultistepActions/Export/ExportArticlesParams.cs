using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesParams : IMultistepActionSettings
    {
        public int SiteId;
        public int ContentId;
        public int[] IDs;
        public ExportArticlesParams(int siteId, int contentId, int[] ids)
        {
            this.SiteId = siteId;
            this.ContentId = contentId;
            this.IDs = ids;
        }
        public int StagesCount
        {
            get { return 2; }
        }
        public bool AllowAction
        {
            get {
				return true;
            }
        }
    }
}

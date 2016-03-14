using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteSettingsCommand : IMultistepActionStageCommand
    {
        public int SiteId { get; set; }
        public int? NewSiteId { get; set; }
        public string SiteName { get; set; }

        public CopySiteSettingsCommand(MultistepActionStageCommandState state) : this(state.Id, null) { }

        public CopySiteSettingsCommand(int siteId, string siteName)
        {
            Site site = SiteRepository.GetById(siteId);

            if (site == null)
            {
                throw new ApplicationException(String.Format(SiteStrings.SiteNotFound, siteId));
            }

            this.SiteId = siteId;
            this.SiteName = siteName;
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            this.NewSiteId = prms.DestinationSiteId;
        }
        public MultistepActionStepResult Step(int step)
        {
            MultistepActionStepResult result = new MultistepActionStepResult();
            SiteService.CopySiteSettings(SiteId, NewSiteId.Value);
            return result;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                Id = SiteId,
                ParentId = 0,
                Type = CopySiteStageCommandTypes.CopySiteSettings
            };
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = 1,
                StepCount = 1,
                Name = String.Format(SiteStrings.CopySiteSettings, (SiteName ?? ""))
            };
        }
    }
}

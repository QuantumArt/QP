using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteService : CopySiteAbstract
    {
        private CopySiteSettingsCommand copySiteSettingsCommand;
        private CopySiteContentsCommand copySiteContentsCommand;
        private CopySiteVirtualContentsCommand copySiteVirtualContentsCommand;
        private CopySiteContentLinksCommand copySiteContentLinksCommand;
        private CopySiteArticlesCommand copySiteArticlesCommand;
        private CopySiteUpdateArticleIdsCommand copySiteUpdateArticleIdsCommand;
        private CopySiteTemlatesCommand copySiteTemplatesCommand;
        private CopySiteFilesCommand copySiteFilesCommand;

        public override void SetupWithParams(int parentId, int oldSiteId, IMultistepActionParams settingsParams)
        {
            HttpContext.Current.Session[CopySiteContextSessionKey] = settingsParams;
        }

        public override MultistepActionSettings Setup(int parentId, int siteId, bool? boundToExternal)
        {

            Site site = SiteRepository.GetById(siteId);

            if (site == null)
            {
                throw new ApplicationException(String.Format(SiteStrings.SiteNotFound, siteId));
            }

            int contentCount = SiteRepository.GetSiteRealContentCount(siteId);
            int virtualContentCount = SiteRepository.GetSiteVirtualContentCount(siteId);
            int siteContentLinksCount = SiteRepository.GetSiteContentLinkCount(siteId);
            int siteArticlesCount = ContentRepository.GetArticlesCountOnSite(siteId);
            int siteTemplatesElementsCount = ObjectRepository.GetTemplatesElementsCountOnSite(siteId);

            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session[CopySiteContextSessionKey];
            if (prms.DoNotCopyArticles != null) {
                siteArticlesCount = ContentRepository.GetArticlesCountToCopy(prms.DoNotCopyArticles.Value, site.Id);
            }

            if (prms.DoNotCopyTemplates)
                siteTemplatesElementsCount = 0;

            copySiteSettingsCommand = new CopySiteSettingsCommand(siteId, site.Name);
            copySiteContentsCommand = new CopySiteContentsCommand(siteId, site.Name, contentCount);
            copySiteVirtualContentsCommand = new CopySiteVirtualContentsCommand(siteId, site.Name, virtualContentCount);

            copySiteContentLinksCommand = new CopySiteContentLinksCommand(siteId, site.Name, siteContentLinksCount);

            copySiteArticlesCommand = new CopySiteArticlesCommand(siteId, site.Name, siteArticlesCount);
            copySiteUpdateArticleIdsCommand = new CopySiteUpdateArticleIdsCommand(siteId, siteArticlesCount);
            copySiteTemplatesCommand = new CopySiteTemlatesCommand(siteId, site.Name, siteTemplatesElementsCount);
            copySiteFilesCommand = new CopySiteFilesCommand(siteId, site.Name, prms.DoNotCopyFiles);

            return base.Setup(parentId, siteId, boundToExternal);
        }


        protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
        {
            return new MultistepActionSettings
            {
                Stages = new[] 
                { 
                    copySiteSettingsCommand.GetStageSettings(),
                    copySiteContentsCommand.GetStageSettings(),
                    copySiteVirtualContentsCommand.GetStageSettings(),
                    copySiteContentLinksCommand.GetStageSettings(),
                    copySiteArticlesCommand.GetStageSettings(),
                    copySiteUpdateArticleIdsCommand.GetStageSettings(),
                    copySiteTemplatesCommand.GetStageSettings(),
                    copySiteFilesCommand.GetStageSettings()
                }
            };
        }

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
        {
            MultistepActionStageCommandState commandState = copySiteContentsCommand.GetState();
            return new MultistepActionServiceContext
            {
                CommandStates = new[] 
                { 
                    copySiteSettingsCommand.GetState(),
                    copySiteContentsCommand.GetState(),
                    copySiteVirtualContentsCommand.GetState(),
                    copySiteContentLinksCommand.GetState(),
                    copySiteArticlesCommand.GetState(),
                    copySiteUpdateArticleIdsCommand.GetState(),
                    copySiteTemplatesCommand.GetState(),
                    copySiteFilesCommand.GetState()
                }
            };
        }

        protected string CopySiteContextSessionKey => "CopySiteService.Settings";

        public override IMultistepActionSettings MultistepActionSettings(int siteId, int contentId)
        {
            IMultistepActionSettings prms = new CopySiteParams();
            return prms;
        }
        public override void TearDown()
        {
            RemoveTempFiles();
            base.TearDown();
        }
        private void RemoveTempFiles() 
        {
            CopySiteSettings prms = (CopySiteSettings)HttpContext.Current.Session[CopySiteContextSessionKey];
            if (prms != null)
            {
                File.Delete(prms.PathForFileWithLinks);
                File.Delete(prms.PathForFileWithFilesToCopy);
            }

        }
    }
}

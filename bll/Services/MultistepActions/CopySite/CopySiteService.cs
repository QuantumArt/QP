using System;
using System.IO;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteService : CopySiteAbstract
    {
        private CopySiteSettingsCommand _copySiteSettingsCommand;
        private CopySiteContentsCommand _copySiteContentsCommand;
        private CopySiteVirtualContentsCommand _copySiteVirtualContentsCommand;
        private CopySiteContentLinksCommand _copySiteContentLinksCommand;
        private CopySiteArticlesCommand _copySiteArticlesCommand;
        private CopySiteItemLinksCommand _copySiteItemLinksCommand;
        private CopySiteUpdateArticleIdsCommand _copySiteUpdateArticleIdsCommand;
        private CopySiteTemlatesCommand _copySiteTemplatesCommand;
        private CopySiteFilesCommand _copySiteFilesCommand;

        public override void SetupWithParams(int parentId, int oldSiteId, IMultistepActionParams settingsParams)
        {
            HttpContext.Current.Session[CopySiteContextSessionKey] = settingsParams;
        }

        public override MultistepActionSettings Setup(int parentId, int siteId, bool? boundToExternal)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, siteId));
            }

            var contentCount = SiteRepository.GetSiteRealContentCount(siteId);
            var virtualContentCount = SiteRepository.GetSiteVirtualContentCount(siteId);
            var siteContentLinksCount = SiteRepository.GetSiteContentLinkCount(siteId);
            var siteArticlesCount = ContentRepository.GetArticlesCountOnSite(siteId);
            var siteTemplatesElementsCount = ObjectRepository.GetTemplatesElementsCountOnSite(siteId);

            var prms = (CopySiteSettings)HttpContext.Current.Session[CopySiteContextSessionKey];
            if (prms.DoNotCopyArticles != null)
            {
                siteArticlesCount = ContentRepository.GetArticlesCountToCopy(prms.DoNotCopyArticles.Value, site.Id);
            }

            if (prms.DoNotCopyTemplates)
            {
                siteTemplatesElementsCount = 0;
            }

            _copySiteSettingsCommand = new CopySiteSettingsCommand(siteId, site.Name);
            _copySiteContentsCommand = new CopySiteContentsCommand(siteId, site.Name, contentCount);
            _copySiteVirtualContentsCommand = new CopySiteVirtualContentsCommand(siteId, site.Name, virtualContentCount);
            _copySiteContentLinksCommand = new CopySiteContentLinksCommand(siteId, site.Name, siteContentLinksCount);
            _copySiteArticlesCommand = new CopySiteArticlesCommand(siteId, site.Name, siteArticlesCount);
            _copySiteItemLinksCommand = new CopySiteItemLinksCommand(siteId, siteArticlesCount);
            _copySiteUpdateArticleIdsCommand = new CopySiteUpdateArticleIdsCommand(siteId, siteArticlesCount);
            _copySiteTemplatesCommand = new CopySiteTemlatesCommand(siteId, site.Name, siteTemplatesElementsCount);
            _copySiteFilesCommand = new CopySiteFilesCommand(siteId, site.Name, prms.DoNotCopyFiles);

            return base.Setup(parentId, siteId, boundToExternal);
        }

        protected override MultistepActionSettings CreateActionSettings(int parentId, int id) => new MultistepActionSettings
        {
            Stages = new[]
            {
                _copySiteSettingsCommand.GetStageSettings(),
                _copySiteContentsCommand.GetStageSettings(),
                _copySiteVirtualContentsCommand.GetStageSettings(),
                _copySiteContentLinksCommand.GetStageSettings(),
                _copySiteArticlesCommand.GetStageSettings(),
                _copySiteItemLinksCommand.GetStageSettings(),
                _copySiteUpdateArticleIdsCommand.GetStageSettings(),
                _copySiteTemplatesCommand.GetStageSettings(),
                _copySiteFilesCommand.GetStageSettings()
            }
        };

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal) => new MultistepActionServiceContext
        {
            CommandStates = new[]
            {
                _copySiteSettingsCommand.GetState(),
                _copySiteContentsCommand.GetState(),
                _copySiteVirtualContentsCommand.GetState(),
                _copySiteContentLinksCommand.GetState(),
                _copySiteArticlesCommand.GetState(),
                _copySiteItemLinksCommand.GetState(),
                _copySiteUpdateArticleIdsCommand.GetState(),
                _copySiteTemplatesCommand.GetState(),
                _copySiteFilesCommand.GetState()
            }
        };

        protected string CopySiteContextSessionKey => HttpContextSession.CopySiteServiceSettings;

        public override IMultistepActionSettings MultistepActionSettings(int siteId, int contentId) => new CopySiteParams();

        public override void TearDown()
        {
            RemoveTempFiles();
            base.TearDown();
        }

        private void RemoveTempFiles()
        {
            var prms = (CopySiteSettings)HttpContext.Current.Session[CopySiteContextSessionKey];
            if (prms != null)
            {
                File.Delete(prms.PathForFileWithLinks);
                File.Delete(prms.PathForFileWithFilesToCopy);
            }
        }
    }
}

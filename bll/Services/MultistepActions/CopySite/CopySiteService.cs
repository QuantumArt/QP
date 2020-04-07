using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteService : CopySiteAbstract
    {
        public override void SetupWithParams(int parentId, int oldSiteId, IMultistepActionParams settingsParams)
        {
            HttpContext.Session.SetValue(CopySiteContextSessionKey, settingsParams);
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

            var prms = HttpContext.Session.GetValue<CopySiteSettings>(CopySiteContextSessionKey);
            if (prms.DoNotCopyArticles != null)
            {
                siteArticlesCount = ContentRepository.GetArticlesCountToCopy(prms.DoNotCopyArticles.Value, site.Id);
            }

            if (prms.DoNotCopyTemplates || site.ExternalDevelopment)
            {
                siteTemplatesElementsCount = 0;
            }

            Commands.Add(new CopySiteSettingsCommand(siteId, site.Name));
            Commands.Add(new CopySiteContentsCommand(siteId, site.Name, contentCount));
            Commands.Add(new CopySiteVirtualContentsCommand(siteId, site.Name, virtualContentCount));
            Commands.Add(new CopySiteContentLinksCommand(siteId, site.Name, siteContentLinksCount));
            Commands.Add(new CopySiteArticlesCommand(siteId, site.Name, siteArticlesCount));
            Commands.Add(new CopySiteItemLinksCommand(siteId, siteArticlesCount));
            Commands.Add(new CopySiteUpdateArticleIdsCommand(siteId, siteArticlesCount));
            Commands.Add(new CopySiteTemlatesCommand(siteId, site.Name, siteTemplatesElementsCount));
            Commands.Add(new CopySiteFilesCommand(siteId, site.Name, prms.DoNotCopyFiles));

            return base.Setup(parentId, siteId, boundToExternal);
        }

        protected string CopySiteContextSessionKey => HttpContextSession.CopySiteServiceSettings;

        public override IMultistepActionSettings MultistepActionSettings(int siteId, int contentId) => new CopySiteParams();

        public override void TearDown()
        {
            RemoveTempFiles();
            base.TearDown();
        }

        private void RemoveTempFiles()
        {
            var prms = HttpContext.Session.GetValue<CopySiteSettings>(CopySiteContextSessionKey);
            if (prms != null)
            {
                File.Delete(prms.PathForFileWithLinks);
                File.Delete(prms.PathForFileWithFilesToCopy);
            }
        }
    }
}

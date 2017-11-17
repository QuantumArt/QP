using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesService : MultistepActionServiceAbstract
    {
        private ExportArticlesCommand _command;

        public void SetupWithParams(int parentId, int id, int[] ids, ExportSettings settingsParams)
        {
            var contentId = ids == null ? id : parentId;
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            settingsParams.ContentId = content.Id;
            HttpContext.Current.Session[HttpContextSession.ExportSettingsSessionKey] = settingsParams;
        }

        public override void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            SetupWithParams(parentId, id, null, settingsParams as ExportSettings);
        }

        public override void SetupWithParams(int parentId, int[] ids, IMultistepActionParams settingsParams)
        {
            SetupWithParams(parentId, 0, ids, settingsParams as ExportSettings);
        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal) => Setup(parentId, id, null, boundToExternal);

        public override MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal)
        {
            var contentId = ids == null ? id : parentId;
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }
            var isArchive = CheckArticlesIsArchive(ids);

            var articleIds = GetArticleIds(ids, content.Id, isArchive);
            var articleExtensionContents = GetArticleExtensionContents(articleIds, content.Id, isArchive);
            _command = new ExportArticlesCommand(content.SiteId, content.Id, articleIds.Length, articleIds, articleExtensionContents);

            return base.Setup(content.SiteId, content.Id, boundToExternal);
        }

        public static bool CheckArticlesIsArchive(int[] ids)
        {
            var articles = ArticleRepository.CheckArticlesIsArchive(ids);
            return articles;
        }

        protected override MultistepActionSettings CreateActionSettings(int parentId, int id) => new MultistepActionSettings
        {
            Stages = new[]
            {
                _command.GetStageSettings()
            },
            ParentId = parentId
        };

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
        {
            var commandState = _command.GetState();
            return new MultistepActionServiceContext { CommandStates = new[] { commandState } };
        }

        protected override string ContextSessionKey { get; } = HttpContextSession.ExportContextSessionKey;

        public override void TearDown()
        {
            HttpContext.Current.Session.Remove(HttpContextSession.ExportSettingsSessionKey);
            base.TearDown();
        }

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state) => new ExportArticlesCommand(state);

        public override IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids)
        {
            IMultistepActionSettings prms;
            if (ids == null)
            {
                prms = new ExportArticlesParams(parentId, id, null);
            }
            else
            {
                var content = ContentRepository.GetById(parentId);
                prms = new ExportArticlesParams(content.SiteId, content.Id, ids);
            }

            return prms;
        }

        private static int[] GetArticleIds(int[] ids, int contentId, bool isArchive = false)
        {
            var settings = HttpContext.Current.Session[HttpContextSession.ExportSettingsSessionKey] as ExportSettings;
            var orderBy = string.IsNullOrEmpty(settings.OrderByField) ? FieldName.ContentItemId : settings.OrderByField;
            return ArticleRepository.SortIdsByFieldName(ids, contentId, orderBy, isArchive);
        }

        private static IEnumerable<Content> GetArticleExtensionContents(int[] ids, int contentId, bool isArchive) => ContentRepository.GetList(
            ContentRepository.GetReferencedAggregatedContentIds(contentId, ids ?? new int[0], isArchive)
        ).ToArray();
    }
}

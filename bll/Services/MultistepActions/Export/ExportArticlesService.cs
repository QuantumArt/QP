using System;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesService : MultistepActionServiceAbstract
    {
        private ExportArticlesCommand _command;

        public void SetupWithParams(int parentId, int id, int[] ids, ExportSettings settingsParams)
        {
            var contentId = (ids == null) ? id : parentId;
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            settingsParams.ContentId = content.Id;
            HttpContext.Current.Session[ExportSettingsSessionKey] = settingsParams;
        }

        public override void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            SetupWithParams(parentId, id, null, settingsParams as ExportSettings);
        }

        public override void SetupWithParams(int parentId, int[] ids, IMultistepActionParams settingsParams)
        {
            SetupWithParams(parentId, 0, ids, settingsParams as ExportSettings);
        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal)
        {
            return Setup(parentId, id, null, boundToExternal);
        }

        public override MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal)
        {
            var contentId = (ids == null) ? id : parentId;
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            var articleIds = GetArticleIds(ids, content.Id);
            _command = new ExportArticlesCommand(content.SiteId, content.Id, articleIds.Length, articleIds);

            return base.Setup(content.SiteId, content.Id, boundToExternal);
        }

        public string ExportSettingsSessionKey => "ExportArticlesService.Settings";

        protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
        {
            return new MultistepActionSettings
            {
                Stages = new[]
                {
                    _command.GetStageSettings()
                },
                ParentId = parentId
            };
        }

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
        {
            var commandState = _command.GetState();
            return new MultistepActionServiceContext { CommandStates = new[] { commandState } };
        }

        protected override string ContextSessionKey => "ExportArticlesService.ProcessingContext";

        public override void TearDown()
        {
            HttpContext.Current.Session.Remove(ExportSettingsSessionKey);
            base.TearDown();
        }

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            return new ExportArticlesCommand(state);
        }

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

        private static int[] GetArticleIds(int[] ids, int contentId)
        {
            var setts = HttpContext.Current.Session["ExportArticlesService.Settings"] as ExportSettings;
            var orderBy = string.IsNullOrEmpty(setts.OrderByField) ? "CONTENT_ITEM_ID" : setts.OrderByField;
            return ArticleRepository.SortIdsByFieldName(ids, contentId, orderBy);
        }
    }
}

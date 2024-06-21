using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    public class ExportArticlesService : MultistepActionServiceAbstract
    {
        public void SetupWithParams(int parentId, int id, int[] ids, ExportSettings settingsParams)
        {
            var contentId = ids == null ? id : parentId;
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            settingsParams.ContentId = content.Id;
            settingsParams.SiteId = content.SiteId;
            HttpContext.Session.SetValue(HttpContextSession.ExportSettingsSessionKey, settingsParams);
        }

        public override void SetupWithParams(int parentId, int id, IMultistepActionParams settingsParams)
        {
            SetupWithParams(parentId, id, null, settingsParams as ExportSettings);
        }

        public override void SetupWithParams(int parentId, int[] ids, IMultistepActionParams settingsParams)
        {
            SetupWithParams(parentId, 0, ids, settingsParams as ExportSettings);
        }
        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal, S3Options options)
        {
            return Setup(parentId, id, null, boundToExternal, options);
        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal, bool isArchive, S3Options options)
        {
            return Setup(parentId, id, null, boundToExternal, isArchive, options);
        }

        public override MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal, S3Options options)
        {
            return Setup(parentId, id, ids, null, false, options);
        }

        public override MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal, bool isArchive, S3Options options)
        {
            var contentId = ids != null && ids.Any() ? parentId : id;
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            var articleIds = GetArticleIds(ids, content.Id, isArchive);
            var articleExtensionContentIds = GetArticleExtensionContentIds(articleIds, content.Id, isArchive);
            Commands.Add(new ExportArticlesCommand(content.SiteId, content.Id, articleIds.Length, articleIds, articleExtensionContentIds));
            return base.Setup(content.SiteId, content.Id, boundToExternal, options);
        }

        protected override string ContextSessionKey { get; } = HttpContextSession.ExportContextSessionKey;

        public override void TearDown()
        {
            HttpContext.Session.Remove(HttpContextSession.ExportSettingsSessionKey);
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

        public override IMultistepActionSettings MultistepActionSettings(int parentId, int id, int[] ids, bool isArchive)
        {
            IMultistepActionSettings prms;
            if (ids == null)
            {
                prms = new ExportArticlesParams(parentId, id, null, isArchive);
            }
            else
            {
                var content = ContentRepository.GetById(parentId);
                prms = new ExportArticlesParams(content.SiteId, content.Id, ids, isArchive);
            }

            return prms;
        }

        private static int[] GetArticleIds(int[] ids, int contentId, bool isArchive = false)
        {
            var settings = HttpContext.Session.GetValue<ExportSettings>(HttpContextSession.ExportSettingsSessionKey);
            var orderBy = string.IsNullOrEmpty(settings.OrderByField) ? FieldName.ContentItemId : settings.OrderByField;
            return ArticleRepository.SortIdsByFieldName(ids, contentId, orderBy, isArchive);
        }

        private static int[] GetArticleExtensionContentIds(int[] ids, int contentId, bool isArchive) =>
            ContentRepository.GetReferencedAggregatedContentIds(contentId, ids ?? new int[0], isArchive).ToArray();
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ArticleVersionController : AuthQpController
    {
        private readonly PathHelper _pathHelper;

        public ArticleVersionController(PathHelper pathHelper)
        {
            _pathHelper = pathHelper;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticleVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.ArticleVersions)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var model = ArticleVersionListViewModel.Create(tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ArticleVersions)]
        [BackendActionContext(ActionCode.ArticleVersions)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ArticleVersionService.List(parentId, listCommand);
            var result = Mapper.Map<List<ArticleVersion>, List<ArticleVersionListItem>>(serviceResult);
            return new TelerikResult(result, result.Count);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareArticleVersionWithCurrent)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.CompareArticleVersionWithCurrent)]
        public async Task<ActionResult> CompareWithCurrent(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var version = ArticleVersionService.GetMergedVersion(new[] { id, ArticleVersion.CurrentVersionId }, parentId);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = ArticleVersionViewType.CompareWithCurrent;
            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareArticleLiveWithCurrent)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "id")]
        [BackendActionContext(ActionCode.CompareArticleLiveWithCurrent)]
        public async Task<ActionResult> CompareLiveWithCurrent(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var version = ArticleVersionService.GetMergedVersion(new[] { ArticleVersion.LiveVersionId, ArticleVersion.CurrentVersionId }, id);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = ArticleVersionViewType.CompareWithCurrent;
            return await JsonHtml("Properties", model);
        }
        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareArticleVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.CompareArticleVersions)]
        public async Task<ActionResult> Compare(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var version = ArticleVersionService.GetMergedVersion(selModel.Ids, parentId);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = ArticleVersionViewType.CompareVersions;
            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PreviewArticleVersion)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.PreviewArticleVersion)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode, bool? boundToExternal)
        {
            var version = ArticleVersionService.Read(id, parentId);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, successfulActionCode, boundToExternal);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.PreviewArticleVersion)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RestoreArticleVersion)]
        [BackendActionContext(ActionCode.RestoreArticleVersion)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string backendActionCode, bool? boundToExternal, IFormCollection collection)
        {
            var version = ArticleVersionService.Read(id, parentId);
            var article = ArticleService.ReadForUpdate(version.ArticleId, version.Article.ContentId);
            version.Article = article;
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data.Article.PathHelper = _pathHelper;
                model.Data.Article = ArticleVersionService.Restore(model.Data, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction());
                return Redirect("Properties", new
                {
                    tabId,
                    parentId,
                    id,
                    successfulActionCode = backendActionCode,
                    boundToExternal
                });
            }

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveArticleVersion)]
        [BackendActionContext(ActionCode.RemoveArticleVersion)]
        [BackendActionLog]
        public ActionResult Remove(int id, bool? boundToExternal)
        {
            var result = ArticleVersionService.Remove(id, boundToExternal);
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveArticleVersion)]
        [BackendActionContext(ActionCode.MultipleRemoveArticleVersion)]
        [BackendActionLog]
        public ActionResult MultipleRemove([FromBody] SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var result = ArticleVersionService.MultipleRemove(selModel.Ids, boundToExternal);
            return JsonMessageResult(result);
        }
    }
}

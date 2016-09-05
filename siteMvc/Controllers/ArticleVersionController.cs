using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using AutoMapper;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class ArticleVersionController : QPController
    {
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticleVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.ArticleVersions)]
        public ActionResult Index(string tabId, int parentId)
        {
            var model = ArticleVersionListViewModel.Create(tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ArticleVersions)]
        [BackendActionContext(ActionCode.ArticleVersions)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = ArticleVersionService.List(parentId, command.GetListCommand());
            var result = Mapper.Map<List<ArticleVersion>, List<ArticleVersionListItem>>(serviceResult);
            return View(new GridModel { Data = result, Total = result.Count });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PreviewArticleVersion)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.PreviewArticleVersion)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? boundToExternal)
        {
            var version = ArticleVersionService.Read(id, parentId);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, successfulActionCode, boundToExternal);
            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareArticleVersionWithCurrent)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.CompareArticleVersionWithCurrent)]
        public ActionResult CompareWithCurrent(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var version = ArticleVersionService.GetMergedVersion(new[] { id, ArticleVersion.CurrentVersionId }, parentId);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = ArticleVersionViewType.CompareWithCurrent;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareArticleVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.CompareArticleVersions)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Compare(string tabId, int parentId, int[] IDs, bool? boundToExternal)
        {
            var version = ArticleVersionService.GetMergedVersion(IDs, parentId);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = ArticleVersionViewType.CompareVersions;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RestoreArticleVersion)]
        [BackendActionContext(ActionCode.RestoreArticleVersion)]
        [BackendActionLog]
        [Record(ActionCode.PreviewArticleVersion)]
        public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, bool? boundToExternal, FormCollection collection)
        {
            var version = ArticleVersionService.Read(id);
            var model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            TryUpdateModel(model);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                model.Data.Article = ArticleVersionService.Restore(model.Data, boundToExternal, IsReplayAction());
                return Redirect("Properties", new
                {
                    tabId,
                    parentId,
                    id,
                    successfulActionCode = backendActionCode,
                    boundToExternal
                });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveArticleVersion)]
        [BackendActionContext(ActionCode.RemoveArticleVersion)]
        [BackendActionLog]
        [Record]
        public ActionResult Remove(int id, bool? boundToExternal)
        {
            var result = ArticleVersionService.Remove(id, boundToExternal);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveArticleVersion)]
        [BackendActionContext(ActionCode.MultipleRemoveArticleVersion)]
        [BackendActionLog]
        [Record]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemove(int[] IDs, bool? boundToExternal)
        {
            var result = ArticleVersionService.MultipleRemove(IDs, boundToExternal);
            return JsonMessageResult(result);
        }
    }
}

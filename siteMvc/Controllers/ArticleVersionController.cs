using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using Telerik.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class ArticleVersionController : QPController
    {
        #region list actions

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticleVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.ArticleVersions)]
        public ActionResult Index(string tabId, int parentId)
        {
            ArticleVersionListViewModel model = ArticleVersionListViewModel.Create(tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ArticleVersions)]
        [BackendActionContext(ActionCode.ArticleVersions)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            List<ArticleVersion> serviceResult = ArticleVersionService.List(parentId, command.GetListCommand());
            List<ArticleVersionListItem> result = Mapper.Map<List<ArticleVersion>, List<ArticleVersionListItem>>(serviceResult);
            return View(new GridModel() { Data = result, Total = result.Count });
        }

        #endregion

        #region forms actions

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PreviewArticleVersion)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.PreviewArticleVersion)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? boundToExternal)
        {
            ArticleVersion version = ArticleVersionService.Read(id, parentId);
            ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, successfulActionCode, boundToExternal);
            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareArticleVersionWithCurrent)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.CompareArticleVersionWithCurrent)]
        public ActionResult CompareWithCurrent(string tabId, int parentId, int id, bool? boundToExternal)
        {
            ArticleVersion version = ArticleVersionService.GetMergedVersion(new[] { id, ArticleVersion.CurrentVersionId }, parentId);
            ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = ArticleVersionViewType.CompareWithCurrent;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CompareArticleVersions)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "parentId")]
        [BackendActionContext(ActionCode.CompareArticleVersions)]
        public ActionResult Compare(string tabId, int parentId, int[] ids, bool? boundToExternal)
        {
            ArticleVersion version = ArticleVersionService.GetMergedVersion(ids, parentId);
            ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            model.ViewType = ArticleVersionViewType.CompareVersions;
            return JsonHtml("Properties", model);
        }

        #endregion

        #region non-interface actions

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.RestoreArticleVersion)]
        [BackendActionContext(ActionCode.RestoreArticleVersion)]
        [BackendActionLog]
        [Record(ActionCode.PreviewArticleVersion)]
        public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, bool? boundToExternal, FormCollection collection)
        {
            ArticleVersion version = ArticleVersionService.Read(id);
            ArticleVersionViewModel model = ArticleVersionViewModel.Create(version, tabId, parentId, boundToExternal);
            TryUpdateModel(model);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                model.Data.Article = ArticleVersionService.Restore(model.Data, boundToExternal, IsReplayAction());
                return Redirect("Properties", new
                {
                    tabId, parentId, id,
                    successfulActionCode = backendActionCode, boundToExternal
                });
            }
            else
                return JsonHtml("Properties", model);
        }


        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.RemoveArticleVersion)]
        [BackendActionContext(ActionCode.RemoveArticleVersion)]
        [BackendActionLog]
        [Record]
        public ActionResult Remove(int id, bool? boundToExternal)
        {
            MessageResult result = ArticleVersionService.Remove(id, boundToExternal);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.MultipleRemoveArticleVersion)]
        [BackendActionContext(ActionCode.MultipleRemoveArticleVersion)]
        [BackendActionLog]
        [Record]
        public ActionResult MultipleRemove(int[] ids, bool? boundToExternal)
        {
            MessageResult result = ArticleVersionService.MultipleRemove(ids, boundToExternal);
            return JsonMessageResult(result);
        }

        #endregion


    }
}

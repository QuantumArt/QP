using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class ArticleController : QPController
    {
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Articles)]
        [BackendActionContext(ActionCode.Articles)]
        public ActionResult Index(string tabId, int parentId, bool? boundToExternal)
        {
            var articleList = ArticleService.InitList(parentId, boundToExternal);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Articles)]
        [BackendActionContext(ActionCode.Articles)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            GridCommand command,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleSearchQueryParam[]>))] ArticleSearchQueryParam[] searchQuery,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleContextQueryParam[]>))] ArticleContextQueryParam[] contextQuery,
            string customFilter,
            bool? onlyIds,
            int[] filterIds)
        {
            var ftsParser = DependencyResolver.Current.GetService<ArticleFullTextSearchQueryParser>();
            var serviceResult = ArticleService.List(parentId, new int[0], command.GetListCommand(), searchQuery, contextQuery, customFilter, ftsParser, onlyIds, filterIds);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArchiveArticles)]
        [BackendActionContext(ActionCode.ArchiveArticles)]
        public ActionResult ArchiveIndex(string tabId, int parentId, bool? boundToExternal)
        {
            var articleList = ArticleService.InitArchiveList(parentId);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId);
            model.ShowArchive = true;
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ArchiveArticles)]
        [BackendActionContext(ActionCode.ArchiveArticles)]
        public ActionResult _ArchiveIndex(
            string tabId,
            int parentId,
            GridCommand command,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleSearchQueryParam[]>))] ArticleSearchQueryParam[] searchQuery,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleContextQueryParam[]>))] ArticleContextQueryParam[] contextQuery,
            string customFilter,
            bool? onlyIds,
            int[] filterIds)
        {
            var ftsParser = DependencyResolver.Current.GetService<ArticleFullTextSearchQueryParser>();
            var serviceResult = ArticleService.List(parentId, new int[0], command.GetListCommand(), searchQuery, contextQuery, customFilter, ftsParser, onlyIds, filterIds);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Articles)]
        [BackendActionContext(ActionCode.Articles)]
        public ActionResult Tree(string tabId, int parentId, bool? boundToExternal)
        {
            var result = ArticleService.InitTree(parentId, false, boundToExternal);
            var model = ArticleListViewModel.Create(result, parentId, tabId);
            return JsonHtml("Tree", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectArticle)]
        [BackendActionContext(ActionCode.SelectArticle)]
        public ActionResult Select(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var articleList = ArticleService.InitList(parentId, boundToExternal);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId, false, true, id);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectArticle)]
        [BackendActionContext(ActionCode.SelectArticle)]
        public ActionResult _Select(
            string tabId,
            int parentId,
            int id,
            GridCommand command,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleSearchQueryParam[]>))] ArticleSearchQueryParam[] searchQuery,
            string customFilter)
        {
            var ftsParser = DependencyResolver.Current.GetService<ArticleFullTextSearchQueryParser>();
            var serviceResult = ArticleService.List(parentId, new[] { id }, command.GetListCommand(), searchQuery, null, customFilter, ftsParser);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectArticle)]
        [BackendActionContext(ActionCode.SelectArticle)]
        public ActionResult SelectTree(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var result = ArticleService.InitTree(parentId, false, boundToExternal);
            var model = ArticleListViewModel.Create(result, parentId, tabId, false, true, new[] { id });
            return JsonHtml("Tree", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectArticle)]
        [BackendActionContext(ActionCode.MultipleSelectArticle)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelect(string tabId, int parentId, int[] IDs, bool? boundToExternal)
        {
            var articleList = ArticleService.InitList(parentId, boundToExternal);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId, true, true, IDs);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectArticle)]
        [BackendActionContext(ActionCode.MultipleSelectArticle)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelect(
            string tabId,
            int parentId,
            string IDs,
            GridCommand command,
            [ModelBinder(typeof(JsonStringModelBinder<ArticleSearchQueryParam[]>))] ArticleSearchQueryParam[] searchQuery,
            string customFilter,
            bool? onlyIds)
        {
            var ftsParser = DependencyResolver.Current.GetService<ArticleFullTextSearchQueryParser>();
            var selectedArticleIDs = Converter.ToInt32Collection(IDs, ',');
            var serviceResult = ArticleService.List(parentId, selectedArticleIDs, command.GetListCommand(), searchQuery, null, customFilter, ftsParser, onlyIds);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectArticle)]
        [BackendActionContext(ActionCode.MultipleSelectArticle)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelectTree(string tabId, int parentId, int[] IDs, bool? boundToExternal)
        {
            var result = ArticleService.InitTree(parentId, true, boundToExternal);
            var model = ArticleListViewModel.Create(result, parentId, tabId, true, true, IDs);
            return JsonHtml("Tree", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticleStatus)]
        [BackendActionContext(ActionCode.ArticleStatus)]
        public ActionResult StatusHistoryList(string tabId, int parentId)
        {
            var model = ArticleStatusHistoryListViewModel.Create(tabId, parentId);
            return JsonHtml("StatusHistoryList", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ArticleStatus)]
        [BackendActionContext(ActionCode.ArticleStatus)]
        public ActionResult _StatusHistoryList(string tabId, int parentId, int id, GridCommand command)
        {
            var result = ArticleService.ArticleStatusHistory(command.GetListCommand(), parentId);
            return View(new GridModel { Data = result.Data, Total = result.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ViewVirtualArticle)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "id")]
        [BackendActionContext(ActionCode.ViewVirtualArticle)]
        public ActionResult VirtualProperties(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var data = ArticleService.ReadVirtual(id, parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            model.IsVirtual = true;
            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ViewArchiveArticle)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "id")]
        [BackendActionContext(ActionCode.ViewArchiveArticle)]
        public ActionResult ArchiveProperties(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var data = ArticleService.ReadArchive(id, parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewArticle)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
        [BackendActionContext(ActionCode.AddNewArticle)]
        public ActionResult New(string tabId, int parentId, int? fieldId, int? articleId, bool? isChild, bool? boundToExternal)
        {
            var data = ArticleService.New(parentId, fieldId, articleId, isChild, boundToExternal);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewArticle)]
        [BackendActionContext(ActionCode.AddNewArticle)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, string backendActionCode, bool? boundToExternal, FormCollection collection)
        {
            var data = ArticleService.NewForSave(parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ArticleService.Create(model.Data, backendActionCode, boundToExternal, CommonHelpers.IsXmlDbUpdateReplayAction(HttpContext));
                    PersistResultId(model.Data.Id);
                    return Redirect("Properties", new
                    {
                        tabId,
                        parentId,
                        id = model.Data.Id,
                        successfulActionCode = backendActionCode,
                        boundToExternal
                    });
                }
                catch (ActionNotAllowedException nae)
                {
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae);
                    return JsonHtml("Properties", model);
                }
            }

            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [RequestHeader("X-Requested-With", "XMLHttpRequest")]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.EditArticle)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "id")]
        [BackendActionContext(ActionCode.EditArticle)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? boundToExternal)
        {
            var data = ArticleService.Read(id, parentId, successfulActionCode);
            var model = ArticleViewModel.Create(data, parentId, tabId, successfulActionCode, boundToExternal);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.EditArticle)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateArticle)]
        [BackendActionContext(ActionCode.UpdateArticle)]
        [BackendActionLog]
        public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, bool? boundToExternal, FormCollection collection)
        {
            var data = ArticleService.ReadForUpdate(id, parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            TryUpdateModel(model);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ArticleService.Update(model.Data, backendActionCode, boundToExternal, IsReplayAction());
                    return Redirect("Properties", new
                    {
                        tabId,
                        parentId,
                        id = model.Data.Id,
                        successfulActionCode = backendActionCode,
                        boundToExternal
                    });
                }
                catch (ActionNotAllowedException nae)
                {
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
                    return JsonHtml("Properties", model);
                }
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeArticle)]
        [BackendActionContext(ActionCode.CreateLikeArticle)]
        [BackendActionLog]
        public ActionResult Copy(int id, bool? boundToExternal)
        {
            var result = ArticleService.Copy(id, boundToExternal, IsReplayAction());
            PersistResultId(result.Id);
            PersistFromId(id);
            return JsonMessageResult(result.Message);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveArticle)]
        [BackendActionContext(ActionCode.RemoveArticle)]
        [BackendActionLog]
        public ActionResult Remove(int parentId, int id, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.Remove(parentId, id, false, boundToExternal, IsReplayAction()));
        }

        public ActionResult RemovePreAction(int parentId, int id, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.RemovePreAction(parentId, id));
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveArticle)]
        [BackendActionContext(ActionCode.MultipleRemoveArticle)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemove(int parentId, int[] IDs, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.Remove(parentId, IDs, false, boundToExternal, IsReplayAction()));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemovePreAction(int parentId, int[] IDs, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.MultipleRemovePreAction(parentId, IDs));
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveArticle)]
        [BackendActionContext(ActionCode.RemoveArticle)]
        [BackendActionLog]
        public ActionResult RemoveFromArchive(int parentId, int id, bool? boundToExternal)
        {
            var result = ArticleService.Remove(parentId, id, true, boundToExternal, IsReplayAction());
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveArticleFromArchive)]
        [BackendActionContext(ActionCode.MultipleRemoveArticleFromArchive)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemoveFromArchive(int parentId, int[] IDs, bool? boundToExternal)
        {
            var result = ArticleService.Remove(parentId, IDs, true, boundToExternal, IsReplayAction());
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MoveArticleToArchive)]
        [BackendActionContext(ActionCode.MoveArticleToArchive)]
        [BackendActionLog]
        public ActionResult MoveToArchive(int id, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.MoveToArchive(id, boundToExternal, IsReplayAction()));
        }

        public ActionResult MoveToArchivePreAction(int id, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.MoveToArchivePreAction(id));
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RestoreArticleFromArchive)]
        [BackendActionContext(ActionCode.RestoreArticleFromArchive)]
        [BackendActionLog]
        public ActionResult RestoreFromArchive(int id, bool? boundToExternal)
        {
            var result = ArticleService.RestoreFromArchive(id, boundToExternal, IsReplayAction());
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultiplePublishArticles)]
        [BackendActionContext(ActionCode.MultiplePublishArticles)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultiplePublish(int parentId, int[] IDs, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.Publish(parentId, IDs, boundToExternal, IsReplayAction()));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultiplePublishPreAction(int[] IDs, bool? boundToExternal)
        {
            return JsonMessageResult(null);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleMoveArticleToArchive)]
        [BackendActionContext(ActionCode.MultipleMoveArticleToArchive)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleMoveToArchive(int parentId, int[] IDs, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.MoveToArchive(parentId, IDs, boundToExternal, IsReplayAction()));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleMoveToArchivePreAction(int[] IDs, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.MultipleMoveToArchivePreAction(IDs));
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRestoreArticleFromArchive)]
        [BackendActionContext(ActionCode.MultipleRestoreArticleFromArchive)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRestoreFromArchive(int parentId, int[] IDs, bool? boundToExternal)
        {
            var result = ArticleService.RestoreFromArchive(parentId, IDs, boundToExternal, IsReplayAction());
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelArticle)]
        [BackendActionContext(ActionCode.CancelArticle)]
        public ActionResult Cancel(int id)
        {
            ArticleService.Cancel(id);
            return JsonMessageResult(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockArticle)]
        [BackendActionContext(ActionCode.CaptureLockArticle)]
        [BackendActionLog]
        public ActionResult CaptureLock(int id)
        {
            ArticleService.CaptureLock(id);
            return JsonMessageResult(null);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        public ActionResult AggregatedArticle(string tabId, int parentId, int id, int aggregatedContentId)
        {
            var aggregatedArticle = ArticleService.GetAggregatedArticle(id, parentId, aggregatedContentId);
            ViewBag.AggregatedArticle = aggregatedArticle;
            return JsonHtml("AggregatedArticle", null);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        public ActionResult GetContextQuery(int id, string currentContext)
        {
            return Json(ArticleService.GetContextQuery(id, currentContext), JsonRequestBehavior.AllowGet);
        }

        [ActionAuthorize(ActionCode.Articles)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        [ConnectionScope]
        public JsonCamelCaseResult<JSendResponse> GetParentIds(List<int> ids, int fieldId, string filter)
        {
            return new JSendResponse
            {
                Status = JSendStatus.Success,
                Data = ArticleService.GetParentIds(ids, fieldId)
            };
        }

        [ActionAuthorize(ActionCode.Articles)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        [ConnectionScope]
        public JsonCamelCaseResult<JSendResponse> GetChildArticleIds(List<int> ids, int fieldId, string filter)
        {
            return new JSendResponse
            {
                Status = JSendStatus.Success,
                Data = ArticleService.GetChildArticles(ids, fieldId, filter)
            };
        }
    }
}

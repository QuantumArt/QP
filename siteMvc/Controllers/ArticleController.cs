using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using QP8.Infrastructure.Web.ActionResults;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Interfaces.Services;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class ArticleController : QPController
    {
        public ArticleController(IArticleService dbArticleService)
            : base(dbArticleService)
        {
        }

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
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

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

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Articles)]
        [BackendActionContext(ActionCode.Articles)]
        public ActionResult Tree(string tabId, int parentId, bool? boundToExternal)
        {
            var result = ArticleService.InitTree(parentId, false, boundToExternal);
            var model = ArticleListViewModel.Create(result, parentId, tabId);
            return JsonHtml("Tree", model);
        }

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
            string customFilter,
            bool? onlyIds)

        {
            var ftsParser = DependencyResolver.Current.GetService<ArticleFullTextSearchQueryParser>();
            var serviceResult = ArticleService.List(parentId, new[] { id }, command.GetListCommand(), searchQuery, null, customFilter, ftsParser, onlyIds);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

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

        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewArticle)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
        [BackendActionContext(ActionCode.AddNewArticle)]
        public ActionResult New(string tabId, int parentId, int? fieldId, int? articleId, bool? isChild, bool? boundToExternal)
        {
            var data = ArticleService.New(parentId, fieldId, articleId, isChild, boundToExternal);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            return JsonHtml("Properties", model);
        }

        [HttpPost, ActionName("New"), Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewArticle)]
        [BackendActionContext(ActionCode.AddNewArticle)]
        [BackendActionLog]
        public ActionResult NewPost(string tabId, int parentId, string backendActionCode, bool? boundToExternal)
        {
            var data = ArticleService.NewForSave(parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ArticleService.Create(model.Data, backendActionCode, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction());
                    PersistFromId(model.Data.Id, model.Data.UniqueId.Value);
                    PersistResultId(model.Data.Id, model.Data.UniqueId.Value);
                    var union = model.Data.AggregatedArticles.Any()
                      ? model.Data.FieldValues.Union(model.Data.AggregatedArticles.SelectMany(f => f.FieldValues))
                      : model.Data.FieldValues;
                    foreach (var fv in union.Where(f => new[] { FieldExactTypes.O2MRelation, FieldExactTypes.M2MRelation, FieldExactTypes.M2ORelation }.Contains(f.Field.ExactType)))
                    {
                        AppendFormGuidsFromIds($"field_{fv.Field.Id}", $"field_uniqueid_{fv.Field.Id}");
                    }

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

        [RequestHeader(RequestHeaders.XRequestedWith, "XMLHttpRequest")]
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

        [HttpPost, ActionName("Properties"), Record(ActionCode.EditArticle)]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateArticle)]
        [BackendActionContext(ActionCode.UpdateArticle)]
        [BackendActionLog]
        public ActionResult PropertiesPost(string tabId, int parentId, int id, string backendActionCode, bool? boundToExternal)
        {
            var data = ArticleService.ReadForUpdate(id, parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            PersistFromId(model.Data.Id, model.Data.UniqueId.Value);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ArticleService.Update(model.Data, backendActionCode, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction());
                    PersistResultId(model.Data.Id, model.Data.UniqueId.Value);
                    var union = model.Data.AggregatedArticles.Any()
                        ? model.Data.FieldValues.Union(model.Data.AggregatedArticles.SelectMany(f=>f.FieldValues))
                        : model.Data.FieldValues;
                    foreach (var fv in union.Where(f => new[] { FieldExactTypes.O2MRelation, FieldExactTypes.M2MRelation, FieldExactTypes.M2ORelation }.Contains(f.Field.ExactType)))
                    {
                        AppendFormGuidsFromIds($"field_{fv.Field.Id}", $"field_uniqueid_{fv.Field.Id}");
                    }

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
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.CreateLikeArticle)]
        [BackendActionContext(ActionCode.CreateLikeArticle)]
        [BackendActionLog]
        public ActionResult Copy(int id, bool? boundToExternal)
        {
            var article = ArticleRepository.GetById(id);
            var fromUniqueId = article.UniqueId.GetValueOrDefault();
            var result = ArticleService.Copy(article, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction(), HttpContext.GetGuidForSubstitution());

            PersistFromId(id, fromUniqueId);
            PersistResultId(result.Id, result.UniqueId);

            return JsonMessageResult(result.Message);
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveArticle)]
        [BackendActionContext(ActionCode.RemoveArticle)]
        [BackendActionLog]
        public ActionResult Remove(int parentId, int id, bool? boundToExternal)
        {
            var articleToRemove = ArticleRepository.GetById(id);
            PersistFromId(articleToRemove.Id, articleToRemove.UniqueId.Value);
            return JsonMessageResult(ArticleService.Remove(parentId, articleToRemove, false, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        public ActionResult RemovePreAction(int parentId, int id, bool? boundToExternal) => JsonMessageResult(ArticleService.RemovePreAction(parentId, id));

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRemoveArticle)]
        [BackendActionContext(ActionCode.MultipleRemoveArticle)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemove(int parentId, int[] IDs, bool? boundToExternal)
        {
            var articlesToRemove = ArticleRepository.GetByIds(IDs);
            var idsToRemove = articlesToRemove.Select(atr => atr.Id).ToArray();
            var guidsToRemove = articlesToRemove.Select(atr => atr.UniqueId.Value).ToArray();
            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.Remove(parentId, IDs, false, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemovePreAction(int parentId, int[] IDs, bool? boundToExternal) => JsonMessageResult(ArticleService.MultipleRemovePreAction(parentId, IDs));

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveArticle)]
        [BackendActionContext(ActionCode.RemoveArticle)]
        [BackendActionLog]
        public ActionResult RemoveFromArchive(int parentId, int id, bool? boundToExternal)
        {
            var articleToRemove = ArticleRepository.GetById(id);
            PersistFromId(articleToRemove.Id, articleToRemove.UniqueId.Value);
            var result = ArticleService.Remove(parentId, id, true, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction());
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRemoveArticleFromArchive)]
        [BackendActionContext(ActionCode.MultipleRemoveArticleFromArchive)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemoveFromArchive(int parentId, int[] IDs, bool? boundToExternal)
        {
            var articlesToRemove = ArticleRepository.GetByIds(IDs);
            var idsToRemove = articlesToRemove.Select(atr => atr.Id).ToArray();
            var guidsToRemove = articlesToRemove.Select(atr => atr.UniqueId.Value).ToArray();
            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.Remove(parentId, IDs, true, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MoveArticleToArchive)]
        [BackendActionContext(ActionCode.MoveArticleToArchive)]
        [BackendActionLog]
        public ActionResult MoveToArchive(int id, bool? boundToExternal)
        {
            var articleToArchive = ArticleRepository.GetById(id);
            PersistFromId(articleToArchive.Id, articleToArchive.UniqueId.Value);
            return JsonMessageResult(ArticleService.MoveToArchive(id, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        public ActionResult MoveToArchivePreAction(int id, bool? boundToExternal) => JsonMessageResult(ArticleService.MoveToArchivePreAction(id));

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RestoreArticleFromArchive)]
        [BackendActionContext(ActionCode.RestoreArticleFromArchive)]
        [BackendActionLog]
        public ActionResult RestoreFromArchive(int id, bool? boundToExternal)
        {
            var articleToRestore = ArticleRepository.GetById(id);
            PersistFromId(articleToRestore.Id, articleToRestore.UniqueId.Value);
            return JsonMessageResult(ArticleService.RestoreFromArchive(articleToRestore, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultiplePublishArticles)]
        [BackendActionContext(ActionCode.MultiplePublishArticles)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultiplePublish(int parentId, int[] IDs, bool? boundToExternal)
        {
            var articlesToRemove = ArticleRepository.GetByIds(IDs);
            var idsToRemove = articlesToRemove.Select(atr => atr.Id).ToArray();
            var guidsToRemove = articlesToRemove.Select(atr => atr.UniqueId.Value).ToArray();
            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.Publish(parentId, IDs, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultiplePublishPreAction(int[] IDs, bool? boundToExternal) => JsonMessageResult(null);

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleMoveArticleToArchive)]
        [BackendActionContext(ActionCode.MultipleMoveArticleToArchive)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleMoveToArchive(int parentId, int[] IDs, bool? boundToExternal)
        {
            var articlesToArchive = ArticleRepository.GetByIds(IDs);
            var idsToRemove = articlesToArchive.Select(atr => atr.Id).ToArray();
            var guidsToRemove = articlesToArchive.Select(atr => atr.UniqueId.Value).ToArray();
            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.MoveToArchive(parentId, IDs, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleMoveToArchivePreAction(int[] IDs, bool? boundToExternal) => JsonMessageResult(ArticleService.MultipleMoveToArchivePreAction(IDs));

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRestoreArticleFromArchive)]
        [BackendActionContext(ActionCode.MultipleRestoreArticleFromArchive)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRestoreFromArchive(int parentId, int[] IDs, bool? boundToExternal)
        {
            var articlesToRestore = ArticleRepository.GetByIds(IDs);
            var idsToRemove = articlesToRestore.Select(atr => atr.Id).ToArray();
            var guidsToRemove = articlesToRestore.Select(atr => atr.UniqueId.Value).ToArray();
            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.RestoreFromArchive(parentId, IDs, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.CancelArticle)]
        [BackendActionContext(ActionCode.CancelArticle)]
        public ActionResult Cancel(int id)
        {
            ArticleService.Cancel(id);
            return JsonMessageResult(null);
        }

        [HttpPost]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.CaptureLockArticle)]
        [BackendActionContext(ActionCode.CaptureLockArticle)]
        [BackendActionLog]
        public ActionResult CaptureLock(int id)
        {
            ArticleService.CaptureLock(id);
            return JsonMessageResult(null);
        }

        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public JsonCamelCaseResult<JSendResponse> GetAggregatedArticle(int id, int parentId, int aggregatedContentId)
        {
            ViewBag.AggregatedArticle = ArticleService.GetAggregatedArticle(id, parentId, aggregatedContentId);
            return JsonCamelCaseHtml("_AggregatedArticle");
        }

        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult GetContextQuery(int id, string currentContext) => Json(ArticleService.GetContextQuery(id, currentContext), JsonRequestBehavior.AllowGet);

        [ConnectionScope]
        [ActionAuthorize(ActionCode.Articles)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public JsonCamelCaseResult<JSendResponse> GetParentIds(List<int> ids, int fieldId, string filter) => new JSendResponse
        {
            Status = JSendStatus.Success,
            Data = ArticleService.GetParentIds(ids, fieldId)
        };

        [ConnectionScope]
        [ActionAuthorize(ActionCode.Articles)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public JsonCamelCaseResult<JSendResponse> GetChildArticleIds(List<int> ids, int fieldId, string filter) => new JSendResponse
        {
            Status = JSendStatus.Success,
            Data = ArticleService.GetChildArticles(ids, fieldId, filter)
        };
    }
}

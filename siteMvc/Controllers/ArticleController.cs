using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Article;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ArticleController : AuthQpController
    {
        private readonly ArticleFullTextSearchQueryParser _parser;

        public ArticleController(IArticleService dbArticleService, ArticleFullTextSearchQueryParser parser, QPublishingOptions options)
            : base(dbArticleService, options)
        {
            _parser = parser;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Articles)]
        [BackendActionContext(ActionCode.Articles)]
        public async Task<ActionResult> Index(string tabId, int parentId, bool? boundToExternal)
        {
            var articleList = ArticleService.InitList(parentId, boundToExternal);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.Articles)]
        [BackendActionContext(ActionCode.Articles)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            [ModelBinder(typeof(JsonStringModelBinder<IList<ArticleSearchQueryParam>>))] IList<ArticleSearchQueryParam> searchQuery,
            string customFilter,
            bool? onlyIds,
            int[] filterIds,
            string orderBy)
        {

            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ArticleService.List(
                parentId,
                new int[0],
                listCommand,
                searchQuery,
                null,
                customFilter,
                _parser,
                onlyIds,
                filterIds);

            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArchiveArticles)]
        [BackendActionContext(ActionCode.ArchiveArticles)]
        public async Task<ActionResult> ArchiveIndex(string tabId, int parentId, bool? boundToExternal)
        {
            var articleList = ArticleService.InitArchiveList(parentId);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId);
            model.ShowArchive = true;
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ArchiveArticles)]
        [BackendActionContext(ActionCode.ArchiveArticles)]
        public ActionResult _ArchiveIndex(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            [ModelBinder(typeof(JsonStringModelBinder<IList<ArticleSearchQueryParam>>))] IList<ArticleSearchQueryParam> searchQuery,
            string customFilter,
            bool? onlyIds,
            int[] filterIds,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ArticleService.List(
                parentId,
                new int[0],
                listCommand,
                searchQuery,
                null,
                customFilter,
                _parser,
                onlyIds,
                filterIds);

            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Articles)]
        [BackendActionContext(ActionCode.Articles)]
        public async Task<ActionResult> Tree(string tabId, int parentId, bool? boundToExternal)
        {
            var result = ArticleService.InitTree(parentId, false, boundToExternal);
            var model = ArticleListViewModel.Create(result, parentId, tabId);
            return await JsonHtml("Tree", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectArticle)]
        [BackendActionContext(ActionCode.SelectArticle)]
        public async Task<ActionResult> Select(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var articleList = ArticleService.InitList(parentId, boundToExternal);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId, false, true, id);
            return await JsonHtml("Index", model);
        }

        /// <param name="IDs">
        /// Идентификатор выбранного компонента: BackendEntityGrid сериализует один или несколько выбранных Id
        /// в строку через запятую. Т.о. для единственного Id, строковое представление совпадает с числовым.
        /// </param>
        [HttpPost]
        [ActionAuthorize(ActionCode.SelectArticle)]
        [BackendActionContext(ActionCode.SelectArticle)]
        public ActionResult _Select(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            [ModelBinder(typeof(JsonStringModelBinder<IList<ArticleSearchQueryParam>>))] IList<ArticleSearchQueryParam> searchQuery,
            string customFilter,
            bool? onlyIds,
            int[] filterIds,
            string orderBy,
            int IDs = 0)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ArticleService.List(
                parentId,
                new[] { IDs },
                listCommand,
                searchQuery,
                null,
                customFilter,
                _parser,
                onlyIds);

            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectArticle)]
        [BackendActionContext(ActionCode.SelectArticle)]
        public async Task<ActionResult> SelectTree(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var result = ArticleService.InitTree(parentId, false, boundToExternal);
            var model = ArticleListViewModel.Create(result, parentId, tabId, false, true, new[] { id });
            return await JsonHtml("Tree", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectArticle)]
        [BackendActionContext(ActionCode.MultipleSelectArticle)]
        public async Task<ActionResult> MultipleSelect(string tabId, int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var articleList = ArticleService.InitList(parentId, boundToExternal);
            var model = ArticleListViewModel.Create(articleList, parentId, tabId, true, true, selModel.Ids);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectArticle)]
        [BackendActionContext(ActionCode.MultipleSelectArticle)]
        public ActionResult _MultipleSelect(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            [FromForm(Name="IDs")]string ids,
            [ModelBinder(typeof(JsonStringModelBinder<IList<ArticleSearchQueryParam>>))] IList<ArticleSearchQueryParam> searchQuery,
            string customFilter,
            bool? onlyIds,
            string orderBy)
        {
            var selectedArticleIDs = Converter.ToInt32Collection(ids, ',');
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ArticleService.List(
                parentId,
                selectedArticleIDs,
                listCommand,
                searchQuery,
                null,
                customFilter,
                _parser,
                onlyIds);

            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectArticle)]
        [BackendActionContext(ActionCode.MultipleSelectArticle)]
        public async Task<ActionResult> MultipleSelectTree(string tabId, int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var result = ArticleService.InitTree(parentId, true, boundToExternal);
            var model = ArticleListViewModel.Create(result, parentId, tabId, true, true, selModel.Ids);
            return await JsonHtml("Tree", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticleStatus)]
        [BackendActionContext(ActionCode.ArticleStatus)]
        public async Task<ActionResult> StatusHistoryList(string tabId, int parentId)
        {
            var model = ArticleStatusHistoryListViewModel.Create(tabId, parentId);
            return await JsonHtml("StatusHistoryList", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ArticleStatus)]
        [BackendActionContext(ActionCode.ArticleStatus)]
        public ActionResult _StatusHistoryList(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var result = ArticleService.ArticleStatusHistory(listCommand, parentId);
            return new TelerikResult(result.Data, result.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ViewVirtualArticle)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "id")]
        [BackendActionContext(ActionCode.ViewVirtualArticle)]
        public async Task<ActionResult> VirtualProperties(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var data = ArticleService.ReadVirtual(id, parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            model.IsVirtual = true;
            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ViewArchiveArticle)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "id")]
        [BackendActionContext(ActionCode.ViewArchiveArticle)]
        public async Task<ActionResult> ArchiveProperties(string tabId, int parentId, int id, bool? boundToExternal)
        {
            var data = ArticleService.ReadArchive(id, parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            return await JsonHtml("Properties", model);
        }

        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewArticle)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
        [BackendActionContext(ActionCode.AddNewArticle)]
        public async Task<ActionResult> New(string tabId, int parentId, int? fieldId, int? articleId, bool? isChild, bool? boundToExternal)
        {
            var data = ArticleService.New(parentId, fieldId, articleId, isChild, boundToExternal);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, ActionName("New"), Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewArticle)]
        [BackendActionContext(ActionCode.AddNewArticle)]
        [BackendActionLog]
        public async Task<ActionResult> NewPost(string tabId, int parentId, string backendActionCode, bool? boundToExternal)
        {
            var data = ArticleService.NewForSave(parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ArticleService.Create(model.Data, backendActionCode, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction());

                    // ReSharper disable once PossibleInvalidOperationException
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
                    ModelState.TryAddModelException("OperationIsNotAllowedForAggregated", nae);
                    return await JsonHtml("Properties", model);
                }
            }

            return await JsonHtml("Properties", model);
        }

        [RequestHeader(RequestHeaders.XRequestedWith, "XMLHttpRequest")]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.EditArticle)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Article, "id")]
        [BackendActionContext(ActionCode.EditArticle)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode, bool? boundToExternal)
        {
            var data = ArticleService.Read(id, parentId, successfulActionCode);
            var model = ArticleViewModel.Create(data, parentId, tabId, successfulActionCode, boundToExternal);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, ActionName("Properties"), Record(ActionCode.EditArticle)]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateArticle)]
        [BackendActionContext(ActionCode.UpdateArticle)]
        [BackendActionLog]
        public async Task<ActionResult> PropertiesPost(string tabId, int parentId, int id, string backendActionCode, bool? boundToExternal)
        {
            var data = ArticleService.ReadForUpdate(id, parentId);
            var model = ArticleViewModel.Create(data, tabId, parentId, boundToExternal);

            // ReSharper disable once PossibleInvalidOperationException
            PersistFromId(model.Data.Id, model.Data.UniqueId.Value);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ArticleService.Update(model.Data, backendActionCode, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction());

                    // ReSharper disable once PossibleInvalidOperationException
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
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
                    return await JsonHtml("Properties", model);
                }
            }

            return await JsonHtml("Properties", model);
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

            // ReSharper disable once PossibleInvalidOperationException
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
        public ActionResult MultipleRemove(int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var articlesToRemove = ArticleRepository.GetByIds(selModel.Ids);
            var idsToRemove = articlesToRemove.Select(atr => atr.Id).ToArray();

            // ReSharper disable once PossibleInvalidOperationException
            var guidsToRemove = articlesToRemove.Select(atr => atr.UniqueId.Value).ToArray();
            PersistFromIds(idsToRemove, guidsToRemove);

            return JsonMessageResult(ArticleService.Remove(parentId, selModel.Ids, false, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        public ActionResult MultipleRemovePreAction(int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            return JsonMessageResult(ArticleService.MultipleRemovePreAction(parentId, selModel.Ids));
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.RemoveArticle)]
        [BackendActionContext(ActionCode.RemoveArticle)]
        [BackendActionLog]
        public ActionResult RemoveFromArchive(int parentId, int id, bool? boundToExternal)
        {
            var articleToRemove = ArticleRepository.GetById(id);

            // ReSharper disable once PossibleInvalidOperationException
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
        public ActionResult MultipleRemoveFromArchive(int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var articlesToRemove = ArticleRepository.GetByIds(selModel.Ids);
            var idsToRemove = articlesToRemove.Select(atr => atr.Id).ToArray();

            // ReSharper disable once PossibleInvalidOperationException
            var guidsToRemove = articlesToRemove.Select(atr => atr.UniqueId.Value).ToArray();
            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.Remove(parentId, selModel.Ids, true, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
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

            // ReSharper disable once PossibleInvalidOperationException
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

            // ReSharper disable once PossibleInvalidOperationException
            PersistFromId(articleToRestore.Id, articleToRestore.UniqueId.Value);

            return JsonMessageResult(ArticleService.RestoreFromArchive(articleToRestore, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultiplePublishArticles)]
        [BackendActionContext(ActionCode.MultiplePublishArticles)]
        [BackendActionLog]
        public ActionResult MultiplePublish(int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var articlesToRemove = ArticleRepository.GetByIds(selModel.Ids);
            var idsToRemove = articlesToRemove.Select(atr => atr.Id).ToArray();

            // ReSharper disable once PossibleInvalidOperationException
            var guidsToRemove = articlesToRemove.Select(atr => atr.UniqueId.Value).ToArray();

            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.Publish(parentId, selModel.Ids, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        public ActionResult MultiplePublishPreAction([FromBody]SelectedItemsViewModel selModel, bool? boundToExternal) => JsonMessageResult(null);

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleMoveArticleToArchive)]
        [BackendActionContext(ActionCode.MultipleMoveArticleToArchive)]
        [BackendActionLog]
        public ActionResult MultipleMoveToArchive(int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var articlesToArchive = ArticleRepository.GetByIds(selModel.Ids);
            var idsToRemove = articlesToArchive.Select(atr => atr.Id).ToArray();

            // ReSharper disable once PossibleInvalidOperationException
            var guidsToRemove = articlesToArchive.Select(atr => atr.UniqueId.Value).ToArray();

            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.MoveToArchive(parentId, selModel.Ids, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
        }

        public ActionResult MultipleMoveToArchivePreAction([FromBody]SelectedItemsViewModel selModel, bool? boundToExternal) => JsonMessageResult(ArticleService.MultipleMoveToArchivePreAction(selModel.Ids));

        [HttpPost, Record]
        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.MultipleRestoreArticleFromArchive)]
        [BackendActionContext(ActionCode.MultipleRestoreArticleFromArchive)]
        [BackendActionLog]
        public ActionResult MultipleRestoreFromArchive(int parentId, [FromBody]SelectedItemsViewModel selModel, bool? boundToExternal)
        {
            var articlesToRestore = ArticleRepository.GetByIds(selModel.Ids);
            var idsToRemove = articlesToRestore.Select(atr => atr.Id).ToArray();

            // ReSharper disable once PossibleInvalidOperationException
            var guidsToRemove = articlesToRestore.Select(atr => atr.UniqueId.Value).ToArray();

            PersistFromIds(idsToRemove, guidsToRemove);
            return JsonMessageResult(ArticleService.RestoreFromArchive(parentId, selModel.Ids, boundToExternal, HttpContext.IsXmlDbUpdateReplayAction()));
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
        public async Task<ActionResult> GetAggregatedArticle(int id, int parentId, int aggregatedContentId)
        {
            ViewBag.AggregatedArticle = ArticleService.GetAggregatedArticle(id, parentId, aggregatedContentId);
            return await JsonCamelCaseHtml("_AggregatedArticle");
        }

        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult GetContextQuery(int id, string currentContext) => Json(ArticleService.GetContextQuery(id, currentContext));

        [ConnectionScope]
        [ActionAuthorize(ActionCode.Articles)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public ActionResult GetParentIds(List<int> ids, int fieldId, string filter) => JsonCamelCase(new JSendResponse
        {
            Status = JSendStatus.Success,
            Data = ArticleService.GetParentIds(ids, fieldId)
        });

        [ConnectionScope]
        [ActionAuthorize(ActionCode.Articles)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public ActionResult GetChildArticleIds(List<int> ids, int fieldId, string filter) => JsonCamelCase(new JSendResponse
        {
            Status = JSendStatus.Success,
            Data = ArticleService.GetChildArticles(ids, fieldId, filter)
        });
    }
}

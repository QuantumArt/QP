using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Content;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;
using Quantumart.QP8.WebMvc.ViewModels.Field;
using Quantumart.QP8.WebMvc.ViewModels.Library;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ContentController : AuthQpController
    {
        private readonly IContentRepository _contentRepository;

        public ContentController(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Contents)]
        [BackendActionContext(ActionCode.Contents)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = ContentService.InitList(parentId);
            var model = ContentListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.Contents)]
        [BackendActionContext(ActionCode.Contents)]
        public ActionResult _Index(
            string tabId, int parentId, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? ContentListFilter.Empty;
            filter.SiteId = parentId > 0 ? (int?)parentId : null;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.List(filter, listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VirtualContents)]
        [BackendActionContext(ActionCode.VirtualContents)]
        public async Task<ActionResult> VirtualIndex(string tabId, int parentId)
        {
            var result = ContentService.InitList(parentId, true);
            var model = ContentListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.VirtualContents)]
        [BackendActionContext(ActionCode.VirtualContents)]
        public ActionResult _VirtualIndex(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var normFilter = ContentListFilter.Empty;
            normFilter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = VirtualContentService.List(normFilter, listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContent)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewContent)]
        public async Task<ActionResult> New(string tabId, int parentId, int? groupId)
        {
            var content = ContentService.New(parentId, groupId);
            var model = ContentViewModel.Create(content, tabId, parentId);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContent)]
        [BackendActionContext(ActionCode.AddNewContent)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, string backendActionCode, IFormCollection collection)
        {
            var content = ContentService.New(parentId, null);
            var model = ContentViewModel.Create(content, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ContentService.Save(model.Data);
                    PersistResultId(model.Data.Id);
                    PersistFieldIds(model.Data.GetFieldIds());
                    PersistLinkIds(model.Data.GetLinkIds());
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return await JsonHtml("Properties", model);
                }

                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = backendActionCode });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ContentProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "id")]
        [BackendActionContext(ActionCode.ContentProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode, bool? groupChanged = null)
        {
            var content = ContentService.Read(id);
            var model = ContentViewModel.Create(content, tabId, parentId);
            model.GroupChanged = groupChanged ?? false;
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.ContentProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateContent)]
        [BackendActionContext(ActionCode.UpdateContent)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string backendActionCode, IFormCollection collection)
        {
            var content = ContentService.ReadForUpdate(id);
            var model = ContentViewModel.Create(content, tabId, parentId);
            var oldGroupId = model.Data.GroupId;

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = ContentService.Update(model.Data);
                }
                catch (VirtualContentProcessingException vcpe)
                {
                    if (HttpContext.IsXmlDbUpdateReplayAction())
                    {
                        throw;
                    }

                    ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
                    return await JsonHtml("Properties", model);
                }

                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = backendActionCode, groupChanged = oldGroupId != model.Data.GroupId });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContentGroup)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewContentGroup)]
        public async Task<ActionResult> NewGroup(string tabId, int parentId)
        {
            var group = ContentService.NewGroup(parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);
            return await JsonHtml("GroupProperties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContentGroup)]
        [BackendActionContext(ActionCode.AddNewContentGroup)]
        [BackendActionLog]
        public async Task<ActionResult> NewGroup(string tabId, int parentId, IFormCollection collection)
        {
            var group = ContentService.NewGroupForSave(parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = ContentService.SaveGroup(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("GroupProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveContentGroup });
            }

            return await JsonHtml("GroupProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ContentGroupProperties)]
        [BackendActionContext(ActionCode.ContentGroupProperties)]
        public async Task<ActionResult> GroupProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var group = ContentService.ReadGroup(id, parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("GroupProperties", model);
        }

        [HttpPost, Record(ActionCode.ContentGroupProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateContentGroup)]
        [BackendActionContext(ActionCode.UpdateContentGroup)]
        [BackendActionLog]
        public async Task<ActionResult> GroupProperties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var group = ContentService.ReadGroupForUpdate(id, parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = ContentService.UpdateGroup(model.Data);
                return Redirect("GroupProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateContentGroup });
            }

            return await JsonHtml("GroupProperties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeContent)]
        [BackendActionContext(ActionCode.CreateLikeContent)]
        [BackendActionLog]
        public ActionResult Copy(int id, int? forceId, string forceFieldIds, string forceLinkIds)
        {
            var result = ContentService.Copy(id, forceId, forceFieldIds?.ToIntArray(), forceLinkIds?.ToIntArray());
            PersistResultId(result.Id);
            PersistFromId(id);
            PersistFieldIds(result.FieldIds);
            PersistLinkIds(result.LinkIds);
            return JsonMessageResult(result.Message);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "Id")]
        [ActionAuthorize(ActionCode.EnableArticlesPermissions)]
        [BackendActionContext(ActionCode.EnableArticlesPermissions)]
        [BackendActionLog]
        public ActionResult EnableArticlePermissions(int id)
        {
            var result = ContentService.EnableArticlePermissions(id);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [EntityAuthorize(ActionTypeCode.Remove, EntityTypeCode.Content, "Id")]
        [ActionAuthorize(ActionCode.SimpleRemoveContent)]
        [BackendActionContext(ActionCode.SimpleRemoveContent)]
        [BackendActionLog]
        public ActionResult SimpleRemove(int id)
        {
            var result = ContentService.SimpleRemove(id);
            return JsonMessageResult(result);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> SearchBlock(int id, string actionCode, string hostId)
        {
            var model = new ContentSearchBlockViewModel(id, actionCode, hostId);
            return await JsonHtml("SearchBlock", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentLibrary)]
        [BackendActionContext(ActionCode.ContentLibrary)]
        public async Task<ActionResult> Library(string tabId, int parentId, int id, int? filterFileTypeId, string subFolder, bool allowUpload = true)
        {
            var result = ContentService.Library(id, subFolder);
            var model = LibraryViewModel.Create(result, tabId, id, filterFileTypeId, allowUpload, LibraryMode.Content);
            return await JsonHtml("Library", model);
        }

        [EntityAuthorize(ActionTypeCode.List, EntityTypeCode.ContentFolder, "gridParentId")]
        public ActionResult _Files(
            string tabId, int parentId, int gridParentId, int page, int pageSize, string orderBy,
            [ModelBinder(typeof(JsonStringModelBinder<LibraryFileFilter>))] LibraryFileFilter searchQuery)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.GetFileList(listCommand, gridParentId, searchQuery);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.List, EntityTypeCode.ContentFolder, "folderId")]
        public JsonResult _FileList(
            int folderId, int? fileTypeId, string fileNameFilter, int pageSize, int pageNumber, int fileShortNameLength = 15)
        {
            var serviceResult = ContentService.GetFileList(
                new ListCommand
                {
                    PageSize = pageSize,
                    StartPage = pageNumber + 1
                },
                folderId,
                new LibraryFileFilter
                {
                    FileType = (FolderFileType?)fileTypeId,
                    FileNameFilter = fileNameFilter
                }
            );

            return Json(new
            {
                success = true,
                data = new ListResult<FileListItem>
                {
                    Data = serviceResult.Data.Select(f => FileListItem.Create(f, fileShortNameLength)).ToList(),
                    TotalRecords = serviceResult.TotalRecords
                }
            });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "folderId")]
        public JsonResult _FolderPath(int folderId)
        {
            var folder = ContentFolderService.GetById(folderId);
            return Json(new
            {
                success = true,
                path = folder.PathInfo.Path,
                url = folder.PathInfo.Url,
                libraryPath = folder.Path
            });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "contentId")]
        public ActionResult _RelateableFields(int? contentId, int? fieldId)
        {
            if (contentId == null)
            {
                return Json(new
                {
                    success = true
                });
            }

            if (contentId == 0 && fieldId.HasValue)
            {
                contentId = FieldService.Read(fieldId.Value).ContentId;
            }

            var content = ContentService.Read(contentId.Value);
            if (content == null)
            {
                throw new ArgumentException(string.Format(ContentStrings.ContentNotFound, contentId.Value));
            }

            Func<Field, bool> fieldFilter = f => true;
            if (fieldId.HasValue)
            {
                fieldFilter = f => !f.IsNew && f.Id != fieldId.Value;
            }

            return Json(new
            {
                success = true,
                data = content.RelateableFields.Where(fieldFilter).Select(f => new { id = f.Id, text = f.Name })
            });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "contentId")]
        public ActionResult _ClassifierFields(int contentId)
        {
            var content = ContentService.Read(contentId);
            if (content == null)
            {
                throw new ArgumentException(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            return Json(new
            {
                success = true,
                data = content.Fields
                    .Where(f => f.IsClassifier)
                    .OrderBy(f => f.Id)
                    .Select(f => new { id = f.Id, text = f.Name })
            });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContent)]
        [BackendActionContext(ActionCode.SelectContent)]
        public async Task<ActionResult> Select(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = ContentService.InitList(parentId);
            var model = new ContentSelectableListViewModel(result, tabId, parentId, selModel.Ids);
            return await JsonHtml("SelectIndex", model);
        }

        /// <param name="IDs">
        /// Идентификатор выбранного компонента: BackendEntityGrid сериализует один или несколько выбранных Id
        /// в строку через запятую. Т.о. для единственного Id, строковое представление совпадает с числовым.
        /// </param>
        [HttpPost]
        [ActionAuthorize(ActionCode.SelectContent)]
        [BackendActionContext(ActionCode.SelectContent)]
        public ActionResult _Select(
            string tabId, int parentId, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            int IDs = 0)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.List(filter, listCommand, IDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForObjectContainer)]
        [BackendActionContext(ActionCode.SelectContentForObjectContainer)]
        public async Task<ActionResult> SelectForObjectContainer(string tabId, int parentId, int id)
        {
            var result = ContentService.InitListForObject();
            var model = new ObjectContentViewModel(result, tabId, parentId, new[] { id }, ContentSelectMode.ForContainer);
            return await JsonHtml("SelectIndex", model);
        }

        /// <param name="IDs">
        /// Идентификатор выбранного компонента: BackendEntityGrid сериализует один или несколько выбранных Id
        /// в строку через запятую. Т.о. для единственного Id, строковое представление совпадает с числовым.
        /// </param>
        [HttpPost]
        [ActionAuthorize(ActionCode.SelectContentForObjectContainer)]
        [BackendActionContext(ActionCode.SelectContentForObjectContainer)]
        public ActionResult _SelectForObjectContainer(
            string tabId, int parentId, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            int IDs = 0)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.ListForContainer(filter, listCommand, IDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForObjectForm)]
        [BackendActionContext(ActionCode.SelectContentForObjectForm)]
        public async Task<ActionResult> SelectForObjectForm(string tabId, int parentId, int id)
        {
            var result = ContentService.InitListForObject();
            var model = new ObjectContentViewModel(result, tabId, parentId, new[] { id }, ContentSelectMode.ForForm);
            return await JsonHtml("SelectIndex", model);
        }

        /// <param name="IDs">
        /// Идентификатор выбранного компонента: BackendEntityGrid сериализует один или несколько выбранных Id
        /// в строку через запятую. Т.о. для единственного Id, строковое представление совпадает с числовым.
        /// </param>
        [HttpPost]
        [ActionAuthorize(ActionCode.SelectContentForObjectForm)]
        [BackendActionContext(ActionCode.SelectContentForObjectForm)]
        public ActionResult _SelectForObjectForm(
            string tabId, int parentId, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            int IDs = 0)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.ListForForm(filter, listCommand, IDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForJoin)]
        [BackendActionContext(ActionCode.SelectContentForJoin)]
        public async Task<ActionResult> SelectForJoin(string tabId, int parentId, int id)
        {
            var result = ContentService.InitList(parentId);
            var model = new JoinContentViewModel(result, tabId, parentId, new[] { id });
            return await JsonHtml("SelectIndex", model);
        }

        /// <param name="IDs">
        /// Идентификатор выбранного компонента: BackendEntityGrid сериализует один или несколько выбранных Id
        /// в строку через запятую. Т.о. для единственного Id, строковое представление совпадает с числовым.
        /// </param>
        [HttpPost]
        [ActionAuthorize(ActionCode.SelectContentForJoin)]
        [BackendActionContext(ActionCode.SelectContentForJoin)]
        public ActionResult _SelectForJoin(
            string tabId, int parentId, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            int IDs = 0)
        {

            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.ListForJoin(filter, listCommand, IDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForField)]
        [BackendActionContext(ActionCode.SelectContentForField)]
        public async Task<ActionResult> SelectForField(string tabId, int parentId, int id)
        {
            var result = ContentService.InitList(parentId);
            ContentSelectableListViewModel model = new FieldContentViewModel(result, tabId, parentId, new[] { id });
            return await JsonHtml("SelectIndex", model);
        }

        /// <param name="IDs">
        /// Идентификатор выбранного компонента: BackendEntityGrid сериализует один или несколько выбранных Id
        /// в строку через запятую. Т.о. для единственного Id, строковое представление совпадает с числовым.
        /// </param>
        [HttpPost]
        [ActionAuthorize(ActionCode.SelectContentForField)]
        [BackendActionContext(ActionCode.SelectContentForField)]
        public ActionResult _SelectForField(
            string tabId, int parentId, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            int IDs = 0)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.ListForField(filter, listCommand, IDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContent)]
        [BackendActionContext(ActionCode.MultipleSelectContent)]
        public async Task<ActionResult> MultipleSelect(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = ContentService.InitList(parentId);
            var model = new ContentSelectableListViewModel(result, tabId, parentId, selModel.Ids) { IsMultiple = true };
            return await JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectContent)]
        [BackendActionContext(ActionCode.MultipleSelectContent)]
        public ActionResult _MultipleSelect(
            string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.List(filter, listCommand, Converter.ToInt32Collection(ids, ','));
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForCustomAction)]
        [BackendActionContext(ActionCode.MultipleSelectContentForCustomAction)]
        public async Task<ActionResult> MultipleSelectForCustomAction(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = ContentService.InitList(parentId);
            var model = new CustomActionContentViewModel(result, tabId, parentId, selModel.Ids)
            {
                IsMultiple = true
            };

            return await JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectContentForCustomAction)]
        [BackendActionContext(ActionCode.MultipleSelectContentForCustomAction)]
        public ActionResult _MultipleSelectForCustomAction(
            string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            string customFilter)
        {
            filter = filter ?? new ContentListFilter();
            if (!string.IsNullOrEmpty(customFilter))
            {
                filter.CustomFilter = customFilter;
            }

            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.ListForCustomAction(filter, listCommand, Converter.ToInt32Collection(ids, ','));
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectContentForWorkflow)]
        public async Task<ActionResult> MultipleSelectForWorkflow(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = ContentService.InitList(parentId);
            var model = new WorkflowContentViewModel(result, tabId, parentId, selModel.Ids) { IsMultiple = true };
            return await JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectContentForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectContentForWorkflow)]
        public ActionResult _MultipleSelectForWorkflow(
            string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            string customFilter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            filter.CustomFilter = customFilter;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.ListForWorkflow(filter, listCommand, Converter.ToInt32Collection(ids, ','));
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForUnion)]
        [BackendActionContext(ActionCode.MultipleSelectContentForUnion)]
        public async Task<ActionResult> MultipleSelectForUnion(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = ContentService.InitList(parentId);
            var model = new UnionContentViewModel(result, tabId, parentId, selModel.Ids) { IsMultiple = true };
            return await JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectContentForUnion)]
        [BackendActionContext(ActionCode.MultipleSelectContentForUnion)]
        public ActionResult _MultipleSelectForUnion(
            string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy,
            [Bind(Prefix = "searchQuery")]
            [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = ContentService.ListForUnion(filter, listCommand, Converter.ToInt32Collection(ids, ','));
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public ActionResult GetContentFormScript(int contentId) => JsonCamelCase(new JSendResponse
        {
            Status = JSendStatus.Success,
            Data = _contentRepository.GetById(contentId).FormScript
        });
    }
}

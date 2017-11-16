using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using QP8.Infrastructure.Web.AspNet.ActionResults;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
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
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ContentController : QPController
    {
        private readonly IContentRepository _contentRepository;

        public ContentController(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Contents)]
        [BackendActionContext(ActionCode.Contents)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = ContentService.InitList(parentId);
            var model = ContentListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Contents)]
        [BackendActionContext(ActionCode.Contents)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command, [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? ContentListFilter.Empty;
            filter.SiteId = parentId > 0 ? (int?)parentId : null;
            var serviceResult = ContentService.List(filter, command.GetListCommand());
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VirtualContents)]
        [BackendActionContext(ActionCode.VirtualContents)]
        public ActionResult VirtualIndex(string tabId, int parentId)
        {
            var result = ContentService.InitList(parentId, true);
            var model = ContentListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.VirtualContents)]
        [BackendActionContext(ActionCode.VirtualContents)]
        public ActionResult _VirtualIndex(string tabId, int parentId, GridCommand command)
        {
            var normFilter = ContentListFilter.Empty;
            normFilter.SiteId = parentId;
            var serviceResult = VirtualContentService.List(normFilter, command.GetListCommand());
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContent)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewContent)]
        public ActionResult New(string tabId, int parentId, int? groupId)
        {
            var content = ContentService.New(parentId, groupId);
            var model = ContentViewModel.Create(content, tabId, parentId);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContent)]
        [BackendActionContext(ActionCode.AddNewContent)]
        [BackendActionLog]
        [ValidateInput(false)]
        public ActionResult New(string tabId, int parentId, string backendActionCode, FormCollection collection)
        {
            var content = ContentService.New(parentId, null);
            var model = ContentViewModel.Create(content, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
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
                    return JsonHtml("Properties", model);
                }

                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = backendActionCode });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ContentProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "id")]
        [BackendActionContext(ActionCode.ContentProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? groupChanged = null)
        {
            var content = ContentService.Read(id);
            var model = ContentViewModel.Create(content, tabId, parentId);
            model.GroupChanged = groupChanged ?? false;
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.ContentProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateContent)]
        [BackendActionContext(ActionCode.UpdateContent)]
        [BackendActionLog]
        [ValidateInput(false)]
        public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, FormCollection collection)
        {
            var content = ContentService.ReadForUpdate(id);
            var model = ContentViewModel.Create(content, tabId, parentId);
            var oldGroupId = model.Data.GroupId;
            TryUpdateModel(model);
            model.Validate(ModelState);
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
                    return JsonHtml("Properties", model);
                }

                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = backendActionCode, groupChanged = oldGroupId != model.Data.GroupId });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContentGroup)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewContentGroup)]
        public ActionResult NewGroup(string tabId, int parentId)
        {
            var group = ContentService.NewGroup(parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);
            return JsonHtml("GroupProperties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContentGroup)]
        [BackendActionContext(ActionCode.AddNewContentGroup)]
        [BackendActionLog]
        public ActionResult NewGroup(string tabId, int parentId, FormCollection collection)
        {
            var group = ContentService.NewGroupForSave(parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = ContentService.SaveGroup(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("GroupProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveContentGroup });
            }

            return JsonHtml("GroupProperties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ContentGroupProperties)]
        [BackendActionContext(ActionCode.ContentGroupProperties)]
        public ActionResult GroupProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var group = ContentService.ReadGroup(id, parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("GroupProperties", model);
        }

        [HttpPost, Record(ActionCode.ContentGroupProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateContentGroup)]
        [BackendActionContext(ActionCode.UpdateContentGroup)]
        [BackendActionLog]
        public ActionResult GroupProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            var group = ContentService.ReadGroupForUpdate(id, parentId);
            var model = ContentGroupViewModel.Create(group, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = ContentService.UpdateGroup(model.Data);
                return Redirect("GroupProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateContentGroup });
            }

            return JsonHtml("GroupProperties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeContent)]
        [BackendActionContext(ActionCode.CreateLikeContent)]
        [BackendActionLog]
        public ActionResult Copy(int id, int? forceId, string forceFieldIds, string forceLinkIds)
        {
            var result = ContentService.Copy(id, forceId, forceFieldIds.ToIntArray(), forceLinkIds.ToIntArray());
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
        public ActionResult SearchBlock(int id, string actionCode, string hostId)
        {
            var model = new ContentSearchBlockViewModel(id, actionCode, hostId);
            return JsonHtml("SearchBlock", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentLibrary)]
        [BackendActionContext(ActionCode.ContentLibrary)]
        public ActionResult Library(string tabId, int parentId, int id, int? filterFileTypeId, string subFolder, bool allowUpload = true)
        {
            var result = ContentService.Library(id, subFolder);
            var model = LibraryViewModel.Create(result, tabId, id, filterFileTypeId, allowUpload, LibraryMode.Content);
            return JsonHtml("Library", model);
        }

        [EntityAuthorize(ActionTypeCode.List, EntityTypeCode.ContentFolder, "gridParentId")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _Files(GridCommand command, int gridParentId, [ModelBinder(typeof(JsonStringModelBinder<LibraryFileFilter>))] LibraryFileFilter searchQuery)
        {
            var serviceResult = ContentService.GetFileList(command.GetListCommand(), gridParentId, searchQuery);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.List, EntityTypeCode.ContentFolder, "folderId")]
        public JsonResult _FileList(int folderId, int? fileTypeId, string fileNameFilter, int pageSize, int pageNumber, int fileShortNameLength = 15)
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

            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    data = new ListResult<FileListItem>
                    {
                        Data = serviceResult.Data.Select(f => FileListItem.Create(f, fileShortNameLength)).ToList(),
                        TotalRecords = serviceResult.TotalRecords
                    }
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "folderId")]
        public JsonResult _FolderPath(int folderId)
        {
            var folder = ContentFolderService.GetById(folderId);
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    path = folder.PathInfo.Path,
                    url = folder.PathInfo.Url,
                    libraryPath = folder.Path
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Content, "contentId")]
        public ActionResult _RelateableFields(int? contentId, int? fieldId)
        {
            if (contentId == null)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        success = true
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
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

            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    data = content.RelateableFields.Where(fieldFilter).Select(f => new { id = f.Id, text = f.Name })
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
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

            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    data = content.Fields
                        .Where(f => f.IsClassifier)
                        .OrderBy(f => f.Id)
                        .Select(f => new { id = f.Id, text = f.Name })
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContent)]
        [BackendActionContext(ActionCode.SelectContent)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Select(string tabId, int parentId, int[] IDs)
        {
            var result = ContentService.InitList(parentId);
            var model = new ContentSelectableListViewModel(result, tabId, parentId, IDs);
            return JsonHtml("SelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectContent)]
        [BackendActionContext(ActionCode.SelectContent)]
        public ActionResult _Select(
            string tabId,
            int parentId,
            int id,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var serviceResult = ContentService.List(filter, command.GetListCommand(), id);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForObjectContainer)]
        [BackendActionContext(ActionCode.SelectContentForObjectContainer)]
        public ActionResult SelectForObjectContainer(string tabId, int parentId, int id)
        {
            var result = ContentService.InitListForObject();
            var model = new ObjectContentViewModel(result, tabId, parentId, new[] { id }, ContentSelectMode.ForContainer);
            return JsonHtml("SelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectContentForObjectContainer)]
        [BackendActionContext(ActionCode.SelectContentForObjectContainer)]
        public ActionResult _SelectForObjectContainer(
            string tabId,
            int id,
            GridCommand command,
            int parentId,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var serviceResult = ContentService.ListForContainer(filter, command.GetListCommand(), id);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForObjectForm)]
        [BackendActionContext(ActionCode.SelectContentForObjectForm)]
        public ActionResult SelectForObjectForm(string tabId, int parentId, int id)
        {
            var result = ContentService.InitListForObject();
            var model = new ObjectContentViewModel(result, tabId, parentId, new[] { id }, ContentSelectMode.ForForm);
            return JsonHtml("SelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectContentForObjectForm)]
        [BackendActionContext(ActionCode.SelectContentForObjectForm)]
        public ActionResult _SelectForObjectForm(
            string tabId,
            int id,
            GridCommand command,
            int parentId,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var serviceResult = ContentService.ListForForm(filter, command.GetListCommand(), id);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForJoin)]
        [BackendActionContext(ActionCode.SelectContentForJoin)]
        public ActionResult SelectForJoin(string tabId, int parentId, int id)
        {
            var result = ContentService.InitList(parentId);
            var model = new JoinContentViewModel(result, tabId, parentId, new[] { id });
            return JsonHtml("SelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectContentForJoin)]
        [BackendActionContext(ActionCode.SelectContentForJoin)]
        public ActionResult _SelectForJoin(
            string tabId,
            int parentId,
            int id,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var serviceResult = ContentService.ListForJoin(filter, command.GetListCommand(), id);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectContentForField)]
        [BackendActionContext(ActionCode.SelectContentForField)]
        public ActionResult SelectForField(string tabId, int parentId, int id)
        {
            var result = ContentService.InitList(parentId);
            ContentSelectableListViewModel model = new FieldContentViewModel(result, tabId, parentId, new[] { id });
            return JsonHtml("SelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectContentForField)]
        [BackendActionContext(ActionCode.SelectContentForField)]
        public ActionResult _SelectForField(
            string tabId,
            int parentId,
            int id,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var serviceResult = ContentService.ListForField(filter, command.GetListCommand(), id);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContent)]
        [BackendActionContext(ActionCode.MultipleSelectContent)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelect(string tabId, int parentId, int[] IDs)
        {
            var result = ContentService.InitList(parentId);
            var model = new ContentSelectableListViewModel(result, tabId, parentId, IDs) { IsMultiple = true };
            return JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectContent)]
        [BackendActionContext(ActionCode.MultipleSelectContent)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelect(
            string tabId,
            int parentId,
            string IDs,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var serviceResult = ContentService.List(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForCustomAction)]
        [BackendActionContext(ActionCode.MultipleSelectContentForCustomAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelectForCustomAction(string tabId, int parentId, int[] IDs)
        {
            var result = ContentService.InitList(parentId);
            var model = new CustomActionContentViewModel(result, tabId, parentId, IDs)
            {
                IsMultiple = true
            };

            return JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForCustomAction)]
        [BackendActionContext(ActionCode.MultipleSelectContentForCustomAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelectForCustomAction(
            string tabId,
            int parentId,
            string IDs,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            string customFilter)
        {
            filter = filter ?? new ContentListFilter();
            if (!string.IsNullOrEmpty(customFilter))
            {
                filter.CustomFilter = customFilter;
            }

            var serviceResult = ContentService.ListForCustomAction(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectContentForWorkflow)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelectForWorkflow(string tabId, int parentId, int[] IDs)
        {
            var result = ContentService.InitList(parentId);
            var model = new WorkflowContentViewModel(result, tabId, parentId, IDs) { IsMultiple = true };
            return JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectContentForWorkflow)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelectForWorkflow(
            string tabId,
            int parentId,
            string IDs,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter,
            string customFilter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            filter.CustomFilter = customFilter;
            var serviceResult = ContentService.ListForWorkflow(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForUnion)]
        [BackendActionContext(ActionCode.MultipleSelectContentForUnion)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelectForUnion(string tabId, int parentId, int[] IDs)
        {
            var result = ContentService.InitList(parentId);
            var model = new UnionContentViewModel(result, tabId, parentId, IDs) { IsMultiple = true };
            return JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectContentForUnion)]
        [BackendActionContext(ActionCode.MultipleSelectContentForUnion)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelectForUnion(
            string tabId,
            int parentId,
            string IDs,
            GridCommand command,
            [Bind(Prefix = "searchQuery")] [ModelBinder(typeof(JsonStringModelBinder<ContentListFilter>))] ContentListFilter filter)
        {
            filter = filter ?? new ContentListFilter();
            filter.SiteId = parentId;
            var serviceResult = ContentService.ListForUnion(filter, command.GetListCommand(), Converter.ToInt32Collection(IDs, ','));
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [ConnectionScope]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public JsonCamelCaseResult<JSendResponse> GetContentFormScript(int contentId) => new JSendResponse
        {
            Status = JSendStatus.Success,
            Data = _contentRepository.GetById(contentId).FormScript
        };
    }
}

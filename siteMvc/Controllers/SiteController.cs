using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Library;
using Quantumart.QP8.WebMvc.ViewModels.Site;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class SiteController : QPController
    {
        private readonly ISearchInArticlesService _searchInArticlesService;

        public SiteController(ISearchInArticlesService searchInArticlesService)
        {
            _searchInArticlesService = searchInArticlesService ?? throw new ArgumentNullException(nameof(searchInArticlesService));
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Sites)]
        [BackendActionContext(ActionCode.Sites)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = SiteService.InitList(parentId);
            var model = SiteListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Sites)]
        [BackendActionContext(ActionCode.Sites)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = SiteService.List(command.GetListCommand());
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectSites)]
        [BackendActionContext(ActionCode.MultipleSelectSites)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelect(string tabId, int parentId, int[] IDs)
        {
            var result = SiteService.MultipleInitList(parentId);
            var model = SiteListViewModel.Create(result, tabId, parentId, true, IDs);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectSites)]
        [BackendActionContext(ActionCode.MultipleSelectSites)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelect(string tabId, int parentId, string IDs, GridCommand command)
        {
            var selectedSiteIDs = Converter.ToInt32Collection(IDs, ',');
            var serviceResult = SiteService.List(command.GetListCommand(), selectedSiteIDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SearchInArticles)]
        [EntityAuthorize(ActionTypeCode.Search, EntityTypeCode.Site, "id")]
        [BackendActionContext(ActionCode.SearchInArticles)]
        public ActionResult SearchInArticles(string tabId, int parentId, int id, string query)
        {
            var model = SearchInArticlesListViewModel.Create(id, tabId, parentId);
            model.Query = query;
            return JsonHtml("SearchInArticles", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SearchInArticles)]
        [BackendActionContext(ActionCode.SearchInArticles)]
        [ValidateInput(false)]
        public ActionResult _SearchInArticles(string tabId, int parentId, int id, GridCommand command, string searchQuery)
        {
            var searchResult = _searchInArticlesService.SearchInArticles(id, QPContext.CurrentUserId, searchQuery, command.GetListCommand(), out var totalRecord);
            return new TelerikResult(searchResult, totalRecord);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewSite)]
        [BackendActionContext(ActionCode.AddNewSite)]
        public ActionResult New(string tabId, int parentId)
        {
            var site = SiteService.New();
            var model = SiteViewModel.Create(site, tabId, parentId);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewSite)]
        [BackendActionContext(ActionCode.AddNewSite)]
        [BackendActionLog]
        [ValidateInput(false)]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var site = SiteService.NewForSave();
            var model = SiteViewModel.Create(site, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = SiteService.Save(model.Data, model.ActiveVeCommandsIds, model.ActiveVeStyleIds);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveSite });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Site, "id")]
        [BackendActionContext(ActionCode.SiteProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var site = SiteService.Read(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = site.LockedByAnyoneElse;
            var model = SiteViewModel.Create(site, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateSite)]
        [BackendActionContext(ActionCode.UpdateSite)]
        [BackendActionLog]
        [ValidateInput(false)]
        [Record(ActionCode.SiteProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var site = SiteService.ReadForUpdate(id);
            var model = SiteViewModel.Create(site, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = SiteService.Update(model.Data, model.ActiveVeCommandsIds, model.ActiveVeStyleIds);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateSite });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteLibrary)]
        [BackendActionContext(ActionCode.SiteLibrary)]
        public ActionResult Library(string tabId, int parentId, int id, int? filterFileTypeId, string subFolder, bool allowUpload = true)
        {
            var result = SiteService.Library(id, subFolder);
            var model = LibraryViewModel.Create(result, tabId, id, filterFileTypeId, allowUpload, LibraryMode.Site);
            return JsonHtml("Library", model);
        }

        [EntityAuthorize(ActionTypeCode.List, EntityTypeCode.SiteFolder, "gridParentId")]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult _Files(GridCommand command, int gridParentId, [ModelBinder(typeof(JsonStringModelBinder<LibraryFileFilter>))] LibraryFileFilter searchQuery)
        {
            var serviceResult = SiteService.GetFileList(command.GetListCommand(), gridParentId, searchQuery);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [EntityAuthorize(ActionTypeCode.List, EntityTypeCode.SiteFolder, "folderId")]
        public JsonResult _FileList(int folderId, int? fileTypeId, string fileNameFilter, int pageSize, int pageNumber, int fileShortNameLength = 15)
        {
            var serviceResult = SiteService.GetFileList(
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
                });

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
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "folderId")]
        public JsonResult _FolderPath(int folderId)
        {
            var folder = SiteFolderService.GetById(folderId);
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

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelSite)]
        [BackendActionContext(ActionCode.CancelSite)]
        public ActionResult Cancel(int id)
        {
            SiteService.Cancel(id);
            return Json(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [EntityAuthorize(ActionTypeCode.Remove, EntityTypeCode.Site, "Id")]
        [ActionAuthorize(ActionCode.SimpleRemoveSite)]
        [BackendActionContext(ActionCode.SimpleRemoveSite)]
        [BackendActionLog]
        public ActionResult SimpleRemove(int id)
        {
            var result = SiteService.SimpleRemove(id);
            return Json(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssembleContents)]
        [BackendActionContext(ActionCode.AssembleContents)]
        [BackendActionLog]
        public ActionResult AssembleContents(int id) => Json(SiteService.AssembleContents(id));

        [HttpPost]
        public ActionResult AssembleContentsPreAction(int id) => Json(SiteService.AssembleContentsPreAction(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockSite)]
        [BackendActionContext(ActionCode.CaptureLockSite)]
        [BackendActionLog]
        public ActionResult CaptureLock(int id)
        {
            SiteService.CaptureLock(id);
            return Json(null);
        }

        public FileResult GetClassesZip(int id)
        {
            var name = SiteService.Read(id).TempArchiveForClasses;
            return File(name, MediaTypeNames.Application.Octet, $"{id}.zip");
        }
    }
}

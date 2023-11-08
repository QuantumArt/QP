using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Library;
using Quantumart.QP8.WebMvc.ViewModels.Site;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class SiteController : AuthQpController
    {
        private readonly ISearchInArticlesService _searchInArticlesService;

        public SiteController(ISearchInArticlesService searchInArticlesService)
        {
            _searchInArticlesService = searchInArticlesService ?? throw new ArgumentNullException(nameof(searchInArticlesService));
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Sites)]
        [BackendActionContext(ActionCode.Sites)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = SiteService.InitList(parentId);
            var model = SiteListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.Sites)]
        [BackendActionContext(ActionCode.Sites)]
        public ActionResult _Index(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = SiteService.List(listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectSites)]
        [BackendActionContext(ActionCode.MultipleSelectSites)]
        public async Task<ActionResult> MultipleSelect(string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel)
        {
            var result = SiteService.MultipleInitList(parentId);
            var model = SiteListViewModel.Create(result, tabId, parentId, true, selModel.Ids);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectSites)]
        [BackendActionContext(ActionCode.MultipleSelectSites)]
        public ActionResult _MultipleSelect(string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy)
        {
            var selectedSiteIDs = Converter.ToInt32Collection(ids, ',');
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = SiteService.List(listCommand, selectedSiteIDs);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SearchInArticles)]
        [EntityAuthorize(ActionTypeCode.Search, EntityTypeCode.Site, "id")]
        [BackendActionContext(ActionCode.SearchInArticles)]
        public async Task<ActionResult> SearchInArticles(string tabId, int parentId, int id, string query)
        {
            var model = SearchInArticlesListViewModel.Create(id, tabId, parentId);
            model.Query = query;
            return await JsonHtml("SearchInArticles", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.SearchInArticles)]
        [BackendActionContext(ActionCode.SearchInArticles)]
        public ActionResult _SearchInArticles(
            string tabId, int parentId, int id, int page, int pageSize, string orderBy, string searchQuery)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var searchResult = _searchInArticlesService.SearchInArticles(id, QPContext.CurrentUserId, searchQuery, listCommand, out var totalRecord);
            return new TelerikResult(searchResult, totalRecord);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewSite)]
        [BackendActionContext(ActionCode.AddNewSite)]
        public async Task<ActionResult> New(string tabId, int parentId)
        {
            var site = SiteService.New();
            var model = SiteViewModel.Create(site, tabId, parentId);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewSite)]
        [BackendActionContext(ActionCode.AddNewSite)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            var site = SiteService.NewForSave();
            var model = SiteViewModel.Create(site, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = SiteService.Save(model.Data, model.ActiveVeCommandsIds, model.ActiveVeStyleIds);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveSite });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Site, "id")]
        [BackendActionContext(ActionCode.SiteProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var site = SiteService.Read(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = site.LockedByAnyoneElse;
            var model = SiteViewModel.Create(site, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateSite)]
        [BackendActionContext(ActionCode.UpdateSite)]
        [BackendActionLog]
        [Record(ActionCode.SiteProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var site = SiteService.ReadForUpdate(id);
            var model = SiteViewModel.Create(site, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = SiteService.Update(model.Data, model.ActiveVeCommandsIds, model.ActiveVeStyleIds);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateSite });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteLibrary)]
        [BackendActionContext(ActionCode.SiteLibrary)]
        public async Task<ActionResult> Library(string tabId, int parentId, int id, int? filterFileTypeId, string subFolder, bool allowUpload = true)
        {
            var result = SiteService.Library(id, subFolder);
            var model = LibraryViewModel.Create(result, tabId, id, filterFileTypeId, allowUpload, LibraryMode.Site);
            return await JsonHtml("Library", model);
        }

        [EntityAuthorize(ActionTypeCode.List, EntityTypeCode.SiteFolder, "gridParentId")]
        public ActionResult _Files(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            int gridParentId,
            [ModelBinder(typeof(JsonStringModelBinder<LibraryFileFilter>))] LibraryFileFilter searchQuery,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = SiteService.GetFileList(listCommand, gridParentId, searchQuery);
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
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "folderId")]
        public JsonResult _FolderPath(int folderId)
        {
            var folder = SiteFolderService.GetById(folderId);
            return Json(new
            {
                success = true,
                path = folder.PathInfo.Path,
                url = folder.PathInfo.Url,
                libraryPath = folder.OsSpecificPath,
                prefixUploadUrl = folder.PathInfo.BaseUploadUrl
            });
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
        public ActionResult AssembleContents(int id)
        {
            return Json(SiteService.AssembleContents(id));
        }

        [HttpPost]
        public ActionResult AssembleContentsPreAction(int id)
        {
            return Json(SiteService.AssembleContentsPreAction(id));
        }

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
            var dir = Path.GetDirectoryName(name);
            var fileName = Path.GetFileName(name);
            var readStream = new PhysicalFileProvider(dir).GetFileInfo(fileName).CreateReadStream();
            return File(readStream, MediaTypeNames.Application.Octet, $"{id}.zip");
        }
    }
}

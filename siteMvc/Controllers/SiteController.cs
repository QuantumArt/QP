using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Telerik.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Utils.FullTextSearch;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Web.UI;
using System.Web.SessionState;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services.MultistepActions;
using System.Collections.Specialized;
using System.Web.Script.Serialization;


namespace Quantumart.QP8.WebMvc.Controllers
{	
	public class SiteController : QPController
	{
		ISearchInArticlesService searchInArticlesService;

		public SiteController(ISearchInArticlesService searchInArticlesService)
		{
			if (searchInArticlesService == null)
				throw new ArgumentNullException("searchInArticlesService");
			this.searchInArticlesService = searchInArticlesService;
		}

		#region list actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.Sites)]
		[BackendActionContext(ActionCode.Sites)]
		public ActionResult Index(string tabId, int parentId)
		{
			SiteInitListResult result = SiteService.InitList(parentId);
			SiteListViewModel model = SiteListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.Sites)]
		[BackendActionContext(ActionCode.Sites)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<SiteListItem> serviceResult = SiteService.List(command.GetListCommand());
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}


		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectSites)]
		[BackendActionContext(ActionCode.MultipleSelectSites)]
		public ActionResult MultipleSelect(string tabId, int parentId, int[] IDs)
		{
			SiteInitListResult result = SiteService.MultipleInitList(parentId);
			SiteListViewModel model = SiteListViewModel.Create(result, tabId, parentId, true, IDs);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectSites)]
		[BackendActionContext(ActionCode.MultipleSelectSites)]
		public ActionResult _MultipleSelect(string tabId, int parentId, string IDs, GridCommand command)
		{
			int[] selectedSiteIDs = Converter.ToInt32Collection(IDs, ',');
            ListResult<SiteListItem> serviceResult = SiteService.List(command.GetListCommand(), selectedSiteIDs);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SearchInArticles)]
		[EntityAuthorize(ActionTypeCode.Search, EntityTypeCode.Site, "id")]
		[BackendActionContext(ActionCode.SearchInArticles)]
		public ActionResult SearchInArticles(string tabId, int parentId, int id, string query)
		{
			SearchInArticlesListViewModel model = SearchInArticlesListViewModel.Create(id, tabId, parentId);
			model.Query = query;
			return this.JsonHtml("SearchInArticles", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SearchInArticles)]
		[BackendActionContext(ActionCode.SearchInArticles)]
		[ValidateInput(false)]
		public ActionResult _SearchInArticles(string tabId, int parentId, int id, GridCommand command, string searchQuery)
		{
			int totalRecord;
			var seachResult = searchInArticlesService.SearchInArticles(id, QPContext.CurrentUserId, searchQuery, command.GetListCommand(), out totalRecord);
			return View(new GridModel() { Data = seachResult, Total = totalRecord });
		}

		#endregion

		#region form actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewSite)]
		[BackendActionContext(ActionCode.AddNewSite)]
		public ActionResult New(string tabId, int parentId)
		{
			Site site = SiteService.New();
			SiteViewModel model = SiteViewModel.Create(site, tabId, parentId);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewSite)]
		[BackendActionContext(ActionCode.AddNewSite)]
		[BackendActionLog]
		[ValidateInput(false)]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			Site site = SiteService.NewForSave();
            SiteViewModel model = SiteViewModel.Create(site, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
                model.Data = SiteService.Save(model.Data, model.ActiveVeCommandsIds, model.ActiveVeStyleIds);
				this.PersistResultId(model.Data.Id);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveSite });
			}
			else
				return JsonHtml("Properties", model);
		}


        [HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SiteProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Site, "id")]
		[BackendActionContext(ActionCode.SiteProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			Site site = SiteService.Read(id);			
			ViewData[SpecialKeys.IsEntityReadOnly] = site.LockedByAnyoneElse;
			SiteViewModel model = SiteViewModel.Create(site, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateSite)]
		[BackendActionContext(ActionCode.UpdateSite)]
		[BackendActionLog]
		[ValidateInput(false)]
		[Record(ActionCode.SiteProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			Site site = SiteService.ReadForUpdate(id);
			SiteViewModel model = SiteViewModel.Create(site, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = SiteService.Update(model.Data, model.ActiveVeCommandsIds, model.ActiveVeStyleIds);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateSite });
			}
			else
				return JsonHtml("Properties", model);
		}

		#endregion

		#region library actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SiteLibrary)]
		[BackendActionContext(ActionCode.SiteLibrary)]
		public ActionResult Library(string tabId, int parentId, int id, int? filterFileTypeId, string subFolder, bool allowUpload = true)
		{
			LibraryResult result = SiteService.Library(id, subFolder);
			LibraryViewModel model = LibraryViewModel.Create(result, tabId, id, filterFileTypeId, allowUpload, LibraryMode.Site);
			return this.JsonHtml("Library", model);
		}

		[EntityAuthorize(ActionTypeCode.List, EntityTypeCode.SiteFolder, "gridParentId")]
		[GridAction(EnableCustomBinding = true)]		
		public ActionResult _Files(GridCommand command, int gridParentId, [ModelBinder(typeof(JsonStringModelBinder<LibraryFileFilter>))] LibraryFileFilter searchQuery)
		{
			ListResult<FolderFile> serviceResult = SiteService.GetFileList(command.GetListCommand(), gridParentId, searchQuery);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		/// <summary>
		/// Получить список файлов в папке
		/// </summary>
		/// <param name="folderId"></param>
		/// <param name="pageSize"></param>
		/// <param name="pageNumber"></param>
		/// <returns></returns>
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[EntityAuthorize(ActionTypeCode.List, EntityTypeCode.SiteFolder, "folderId")]
		public JsonResult _FileList(int folderId, int? fileTypeId, string fileNameFilter, int pageSize, int pageNumber, int fileShortNameLength = 15)
		{
			var serviceResult = SiteService.GetFileList(new ListCommand { PageSize = pageSize, StartPage = pageNumber + 1 }, folderId,
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

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "folderId")]
		public JsonResult _FolderPath(int folderId)
		{
			Folder folder = SiteFolderService.GetById(folderId);
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

		#endregion

		#region non-interface actions

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.CancelSite)]
		[BackendActionContext(ActionCode.CancelSite)]
		public ActionResult Cancel(int id)
		{
			SiteService.Cancel(id);
			return Json(null);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[EntityAuthorize(ActionTypeCode.Remove, EntityTypeCode.Site, "Id")]
		[ActionAuthorize(ActionCode.SimpleRemoveSite)]
		[BackendActionContext(ActionCode.SimpleRemoveSite)]
		[BackendActionLog]
		public ActionResult SimpleRemove(int id)
		{
			MessageResult result = SiteService.SimpleRemove(id);
			return Json(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
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
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.CaptureLockSite)]
		[BackendActionContext(ActionCode.CaptureLockSite)]
		[BackendActionLog]
		public ActionResult CaptureLock(int id)
		{
			SiteService.CaptureLock(id);
			return Json(null);
		}

		#endregion
    }
}

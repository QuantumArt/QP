using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.ViewModels.Library;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class ContentFolderController : QPController
	{

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewContentFolder)]
		[BackendActionContext(ActionCode.AddNewContentFolder)]
		public ActionResult New(string tabId, int parentId, int id)
		{
			ContentFolder folder = ContentFolderService.New(parentId, id);
			ContentFolderViewModel model = ContentFolderViewModel.Create(folder, tabId, parentId);
			return this.JsonHtml("FolderProperties", model);
		}

		[HttpPost]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewContentFolder)]
		[BackendActionContext(ActionCode.AddNewContentFolder)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, int id, FormCollection collection)
		{
			ContentFolder folder = ContentFolderService.NewForSave(parentId, id);
			ContentFolderViewModel model = ContentFolderViewModel.Create(folder, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = ContentFolderService.Save(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveSiteFolder });
			}
			else
				return JsonHtml("FolderProperties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ContentFolderProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "id")]
		[BackendActionContext(ActionCode.ContentFolderProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			ContentFolder folder = ContentFolderService.Read(id);
			ContentFolderViewModel model = ContentFolderViewModel.Create(folder, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("FolderProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateContentFolder)]
		[BackendActionContext(ActionCode.UpdateContentFolder)]
		[BackendActionLog]
		[Record(ActionCode.ContentFolderProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			ContentFolder folder = ContentFolderService.ReadForUpdate(id);
			ContentFolderViewModel model = ContentFolderViewModel.Create(folder, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = ContentFolderService.Update(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateSite });
			}
			else
				return JsonHtml("FolderProperties", model);
		}

		public ActionResult RemovePreAction(int parentId, int id)
		{
			return Json(ContentFolderService.RemovePreAction(id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveContentFolder)]
		[BackendActionContext(ActionCode.RemoveContentFolder)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = ContentFolderService.Remove(id);
			return JsonMessageResult(result);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ContentFileProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.ContentFolder, "parentId")]
		[BackendActionContext(ActionCode.ContentFileProperties)]		
		public ActionResult FileProperties(string tabId, int parentId, string id, string successfulActionCode)
		{
			FolderFile file = ContentFolderService.GetFile(parentId, id);
			FileViewModel model = FileViewModel.Create(file, tabId, parentId, false);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("FileProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.UpdateContentFile)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
		[BackendActionContext(ActionCode.UpdateContentFile)]
		[BackendActionLog]
		public ActionResult FileProperties(string tabId, int parentId, string id, FormCollection collection)
		{
			FolderFile file = ContentFolderService.GetFile(parentId, id);
			FileViewModel model = FileViewModel.Create(file, tabId, parentId, false);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				ContentFolderService.SaveFile(model.File);
				return Redirect("FileProperties", new { tabId = tabId, parentId = parentId, id = model.Id, successfulActionCode = Constants.ActionCode.UpdateContentFile });
			}
			else
				return (ActionResult)this.JsonHtml("FileProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.MultipleRemoveContentFile)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
		[BackendActionContext(ActionCode.MultipleRemoveContentFile)]
		[BackendActionLog]
		public ActionResult MultipleRemoveFiles(int parentId, string[] IDs)
		{
			MessageResult result = ContentFolderService.RemoveFiles(parentId, IDs);
			return Json(result);
		}
		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.RemoveContentFile)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.ContentFolder, "parentId")]
		[BackendActionContext(ActionCode.RemoveContentFile)]
		[BackendActionLog]
		public ActionResult RemoveFile(int parentId, string id)
		{
			string[] IDs = { id };
			MessageResult result = ContentFolderService.RemoveFiles(parentId, IDs);
			return Json(result);
		}
	}
}

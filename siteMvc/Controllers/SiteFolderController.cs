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
    public class SiteFolderController : QPController
    {
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewSiteFolder)]
		[BackendActionContext(ActionCode.AddNewSiteFolder)]
		public ActionResult New(string tabId, int parentId, int id)
		{			
			SiteFolder folder = SiteFolderService.New(parentId, id);			
			SiteFolderViewModel model = SiteFolderViewModel.Create(folder, tabId, parentId);
			return this.JsonHtml("FolderProperties", model);
		}

		[HttpPost]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "id")]
		[ActionAuthorize(ActionCode.AddNewSiteFolder)]
		[BackendActionContext(ActionCode.AddNewSiteFolder)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, int id, FormCollection collection)
		{			
			SiteFolder folder = SiteFolderService.NewForSave(parentId, id);			
			SiteFolderViewModel model = SiteFolderViewModel.Create(folder, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = SiteFolderService.Save(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveSiteFolder });
			}
			else
				return JsonHtml("FolderProperties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SiteFolderProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "id")]
		[BackendActionContext(ActionCode.SiteFolderProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			SiteFolder folder = SiteFolderService.Read(id);
			SiteFolderViewModel model = SiteFolderViewModel.Create(folder, tabId, parentId);			
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("FolderProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateSiteFolder)]
		[BackendActionContext(ActionCode.UpdateSiteFolder)]
		[BackendActionLog]
		[Record]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			SiteFolder folder = SiteFolderService.ReadForUpdate(id);
			SiteFolderViewModel model = SiteFolderViewModel.Create(folder, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = SiteFolderService.Update(model.Data);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateSite });
			}
			else
				return JsonHtml("FolderProperties", model);
		}

		public ActionResult RemovePreAction(int parentId, int id)
		{
			return Json(SiteFolderService.RemovePreAction(id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveSiteFolder)]
		[BackendActionContext(ActionCode.RemoveSiteFolder)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = SiteFolderService.Remove(id);
			return JsonMessageResult(result);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SiteFileProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.SiteFolder, "parentId")]
		[BackendActionContext(ActionCode.SiteFileProperties)]
		public ActionResult FileProperties(string tabId, int parentId, string id, string successfulActionCode)
		{
			FolderFile file = SiteFolderService.GetFile(parentId, id);
			FileViewModel model = FileViewModel.Create(file, tabId, parentId, true);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("FileProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.UpdateSiteFile)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
		[BackendActionContext(ActionCode.UpdateSiteFile)]
		[BackendActionLog]
		public ActionResult FileProperties(string tabId, int parentId, string id, FormCollection collection)
		{
			FolderFile file = SiteFolderService.GetFile(parentId, id);
			FileViewModel model = FileViewModel.Create(file, tabId, parentId, true);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				SiteFolderService.SaveFile(model.File);
				return Redirect("FileProperties", new { tabId = tabId, parentId = parentId, id = model.Id, successfulActionCode = Constants.ActionCode.UpdateSiteFile });
			}
			else
				return (ActionResult)this.JsonHtml("FileProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.MultipleRemoveSiteFile)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
		[BackendActionContext(ActionCode.MultipleRemoveSiteFile)]
		[BackendActionLog]
		public ActionResult MultipleRemoveFiles(int parentId, string[] IDs)
		{
			MessageResult result = SiteFolderService.RemoveFiles(parentId, IDs);
			return Json(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.RemoveSiteFile)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.SiteFolder, "parentId")]
		[BackendActionContext(ActionCode.RemoveSiteFile)]
		[BackendActionLog]
		public ActionResult RemoveFile(int parentId, string id)
		{
			string[] IDs = { id };
			MessageResult result = SiteFolderService.RemoveFiles(parentId, IDs);
			return Json(result);
		}
    }
}

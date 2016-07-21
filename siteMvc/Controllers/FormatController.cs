using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
	[ValidateInput(false)]
	public class FormatController : QPController
    {
		IFormatService _formatService;

		public FormatController(IFormatService formatService)
		{
			this._formatService = formatService;
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.PageObjectFormats)]
		[BackendActionContext(ActionCode.PageObjectFormats)]
		public ActionResult IndexPageObjectFormats(string tabId, int parentId)
		{
			FormatInitListResult result = _formatService.InitFormatList(parentId, false);
			ObjectFormatListViewModel model = ObjectFormatListViewModel.Create(result, tabId, parentId, false);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.PageObjectFormats)]
		[BackendActionContext(ActionCode.PageObjectFormats)]
		public ActionResult _IndexPageObjectFormats(string tabId, int parentId, GridCommand command)
		{
			ListResult<ObjectFormatListItem> serviceResult = _formatService.GetPageObjectFormatsByObjectId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.TemplateObjectFormats)]
		[BackendActionContext(ActionCode.TemplateObjectFormats)]
		public ActionResult IndexTemplateObjectFormats(string tabId, int parentId)
		{
			FormatInitListResult result = _formatService.InitFormatList(parentId, true);
			ObjectFormatListViewModel model = ObjectFormatListViewModel.Create(result, tabId, parentId, true);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.TemplateObjectFormats)]
		[BackendActionContext(ActionCode.TemplateObjectFormats)]
		public ActionResult _IndexTemplateObjectFormats(string tabId, int parentId, GridCommand command)
		{
			ListResult<ObjectFormatListItem> serviceResult = _formatService.GetTemplateObjectFormatsByObjectId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.PageObjectFormatVersions)]
		[BackendActionContext(ActionCode.PageObjectFormatVersions)]
		public ActionResult IndexPageObjectFormatVersions(string tabId, int parentId)
		{
			FormatVersionInitListResult result = _formatService.InitFormatVersionList();
			ObjectFormatVersionListViewModel model = ObjectFormatVersionListViewModel.Create(result, tabId, parentId, false);
			return this.JsonHtml("IndexVersions", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.PageObjectFormatVersions)]
		[BackendActionContext(ActionCode.PageObjectFormatVersions)]
		public ActionResult _IndexPageObjectFormatVersions(string tabId, int parentId, GridCommand command)
		{
			ListResult<ObjectFormatVersionListItem> serviceResult = _formatService.GetPageObjectFormatVersionsByFormatId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.TemplateObjectFormatVersions)]
		[BackendActionContext(ActionCode.TemplateObjectFormatVersions)]
		public ActionResult IndexTemplateObjectFormatVersions(string tabId, int parentId)
		{
			FormatVersionInitListResult result = _formatService.InitFormatVersionList();
			ObjectFormatVersionListViewModel model = ObjectFormatVersionListViewModel.Create(result, tabId, parentId, true);
			return this.JsonHtml("IndexVersions", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.TemplateObjectFormatVersions)]
		[BackendActionContext(ActionCode.TemplateObjectFormatVersions)]
		public ActionResult _IndexTemplateObjectFormatVersions(string tabId, int parentId, GridCommand command)
		{
			ListResult<ObjectFormatVersionListItem> serviceResult = _formatService.GetTemplateObjectFormatVersionsByFormatId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.CancelTemplateObjectFormat)]
		[BackendActionContext(ActionCode.CancelTemplateObjectFormat)]
		public ActionResult CancelTemplateObjectFormat(int id)
		{
			_formatService.CancelFormat(id, false);
			return Json(null);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.CancelPageObjectFormat)]
		[BackendActionContext(ActionCode.CancelPageObjectFormat)]
		public ActionResult CancelPageObjectFormat(int id)
		{
			_formatService.CancelFormat(id, true);
			return Json(null);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.AddNewPageObjectFormat)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.PageObjectFormat, "parentId")]
		[BackendActionContext(ActionCode.AddNewPageObjectFormat)]
		public ActionResult NewPageObjectFormat(string tabId, int parentId)
		{
			ObjectFormat format = _formatService.NewPageObjectFormatProperties(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.AddNewPageObjectFormat)]
		[BackendActionContext(ActionCode.AddNewPageObjectFormat)]
		[BackendActionLog]
		[Record]
		public ActionResult NewPageObjectFormat(string tabId, int parentId, FormCollection collection)
		{
			ObjectFormat format = _formatService.NewPageObjectFormatPropertiesForUpdate(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _formatService.SaveObjectFormatProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("PageObjectFormatProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SavePageObjectFormat });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.AddNewTemplateObjectFormat)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.TemplateObjectFormat, "parentId")]
		[BackendActionContext(ActionCode.AddNewTemplateObjectFormat)]
		public ActionResult NewTemplateObjectFormat(string tabId, int parentId)
		{
			ObjectFormat format = _formatService.NewTemplateObjectFormatProperties(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.AddNewTemplateObjectFormat)]
		[BackendActionContext(ActionCode.AddNewTemplateObjectFormat)]
		[BackendActionLog]
		[Record]
		public ActionResult NewTemplateObjectFormat(string tabId, int parentId, FormCollection collection)
		{
			ObjectFormat format = _formatService.NewTemplateObjectFormatPropertiesForUpdate(parentId, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _formatService.SaveObjectFormatProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("TemplateObjectFormatProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveTemplateObjectFormat });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.PageObjectFormatProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormat, "id")]
		[BackendActionContext(ActionCode.PageObjectFormatProperties)]
		public ActionResult PageObjectFormatProperties(string tabId, int parentId, int id, string successfulActionCode)
		{
			ObjectFormat format = _formatService.ReadPageObjectFormatProperties(id, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);
			ViewData[SpecialKeys.IsEntityReadOnly] = format.LockedByAnyoneElse;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.UpdatePageObjectFormat)]
		[BackendActionContext(ActionCode.UpdatePageObjectFormat)]
		[BackendActionLog]
		[Record(ActionCode.PageObjectFormatProperties)]
		public ActionResult PageObjectFormatProperties(string tabId, int parentId, int id, FormCollection collection)
		{
			ObjectFormat format = _formatService.ReadPageObjectFormatPropertiesForUpdate(id, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, true);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _formatService.UpdateObjectFormatProperties(model.Data);
				return Redirect("PageObjectFormatProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdatePageObjectFormat });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.TemplateObjectFormatProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "id")]
		[BackendActionContext(ActionCode.TemplateObjectFormatProperties)]
		public ActionResult TemplateObjectFormatProperties(string tabId, int parentId, int id, string successfulActionCode)
		{
			ObjectFormat format = _formatService.ReadTemplateObjectFormatProperties(id, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);
			ViewData[SpecialKeys.IsEntityReadOnly] = format.LockedByAnyoneElse;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.UpdateTemplateObjectFormat)]
		[BackendActionContext(ActionCode.UpdateTemplateObjectFormat)]
		[BackendActionLog]
		[Record(ActionCode.TemplateObjectFormatProperties)]
		public ActionResult TemplateObjectFormatProperties(string tabId, int parentId, int id, FormCollection collection)
		{
			ObjectFormat format = _formatService.ReadTemplateObjectFormatPropertiesForUpdate(id, _formatService.IsSiteDotNetByObjectId(parentId));
			ObjectFormatViewModel model = ObjectFormatViewModel.Create(format, tabId, parentId, _formatService, false);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _formatService.UpdateObjectFormatProperties(model.Data);
				return Redirect("TemplateObjectFormatProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateTemplateObjectFormat });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.TemplateObjectFormatVersionProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormatVersion, "id")]
		[BackendActionContext(ActionCode.TemplateObjectFormatVersionProperties)]
		public ActionResult TemplateObjectFormatVersionProperties(string tabId, int parentId, int id, string successfulActionCode)
		{
			ObjectFormatVersion version = _formatService.ReadTemplateObjectFormatVersionProperties(id);
			ObjectFormatVersionViewModel model = ObjectFormatVersionViewModel.Create(version, tabId, parentId, false);
			ViewData[SpecialKeys.IsEntityReadOnly] = true;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("VersionProperties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.PageObjectFormatVersionProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormatVersion, "parentId")]
		[BackendActionContext(ActionCode.PageObjectFormatVersionProperties)]
		public ActionResult PageObjectFormatVersionProperties(string tabId, int parentId, int id, string successfulActionCode)
		{
			ObjectFormatVersion version = _formatService.ReadPageObjectFormatVersionProperties(id);
			ObjectFormatVersionViewModel model = ObjectFormatVersionViewModel.Create(version, tabId, parentId, true);
			ViewData[SpecialKeys.IsEntityReadOnly] = true;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("VersionProperties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.CompareWithCurrentTemplateObjectFormatVersion)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "parentId")]
		[BackendActionContext(ActionCode.CompareWithCurrentTemplateObjectFormatVersion)]
		public ActionResult CompareWithCurrentTemplateObjectFormatVersion(string tabId, int parentId, int id, string successfulActionCode)
		{
			ObjectFormatVersion version = _formatService.GetMergedObjectFormatVersion(new int[] { id, ObjectFormatVersion.CurrentVersionId }, parentId, false);
			ObjectFormatVersionCompareViewModel model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, false);
			ViewData[SpecialKeys.IsEntityReadOnly] = true;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("CompareObjectFormatVersionProperties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.CompareWithCurrentPageObjectFormatVersion)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormat, "parentId")]
		[BackendActionContext(ActionCode.CompareWithCurrentPageObjectFormatVersion)]
		public ActionResult CompareWithCurrentPageObjectFormatVersion(string tabId, int parentId, int id, string successfulActionCode)
		{
			ObjectFormatVersion version = _formatService.GetMergedObjectFormatVersion(new int[] { id, ObjectFormatVersion.CurrentVersionId }, parentId, true);
			ObjectFormatVersionCompareViewModel model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, true);
			ViewData[SpecialKeys.IsEntityReadOnly] = true;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("CompareObjectFormatVersionProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.CompareTemplateObjectFormatVersions)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "parentId")]
		[BackendActionContext(ActionCode.CompareTemplateObjectFormatVersions)]
		public ActionResult CompareTemplateObjectFormatVersions(string tabId, int parentId, int[] IDs)
		{
			ObjectFormatVersion version = _formatService.GetMergedObjectFormatVersion(IDs, parentId, false);
			ObjectFormatVersionCompareViewModel model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, false);
			ViewData[SpecialKeys.IsEntityReadOnly] = true;
			return this.JsonHtml("CompareObjectFormatVersionProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.ComparePageObjectFormatVersions)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObjectFormat, "parentId")]
		[BackendActionContext(ActionCode.ComparePageObjectFormatVersions)]
		public ActionResult ComparePageObjectFormatVersions(string tabId, int parentId, int[] IDs)
		{
			ObjectFormatVersion version = _formatService.GetMergedObjectFormatVersion(IDs, parentId, true);
			ObjectFormatVersionCompareViewModel model = ObjectFormatVersionCompareViewModel.Create(version, tabId, parentId, true);
			ViewData[SpecialKeys.IsEntityReadOnly] = true;
			ViewData[SpecialKeys.IsEntityReadOnly] = true;
			return this.JsonHtml("CompareObjectFormatVersionProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.RemoveTemplateObjectFormat)]
		[BackendActionContext(ActionCode.RemoveTemplateObjectFormat)]
		[BackendActionLog]
		[Record]
		public ActionResult RemoveTemplateObjectFormat(int id)
		{
			MessageResult result = _formatService.RemoveObjectFormat(id, false);
			return this.JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.RemovePageObjectFormat)]
		[BackendActionContext(ActionCode.RemovePageObjectFormat)]
		[BackendActionLog]
		[Record]
		public ActionResult RemovePageObjectFormat(int id)
		{
			MessageResult result = _formatService.RemoveObjectFormat(id, true);
			return this.JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.MultipleRemoveTemplateObjectFormatVersion)]
		[BackendActionContext(ActionCode.MultipleRemoveTemplateObjectFormatVersion)]
		[BackendActionLog]
		[Record]
		public ActionResult MultipleRemoveTemplateObjectFormatVersion(int parentId, int[] IDs)
		{
			return this.JsonMessageResult(_formatService.MultipleRemoveObjectFormatVersion(IDs));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.RestorePageObjectFormatVersion)]
		[BackendActionContext(ActionCode.RestorePageObjectFormatVersion)]
		[BackendActionLog]
		[Record]
		public ActionResult RestorePageObjectFormatVersion(string tabId, int parentId, int id)
		{
			return this.JsonMessageResult(_formatService.RestoreObjectFormatVersion(id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.RestoreTemplateObjectFormatVersion)]
		[BackendActionContext(ActionCode.RestoreTemplateObjectFormatVersion)]
		[BackendActionLog]
		[Record]
		public ActionResult RestoreTemplateObjectFormatVersion(string tabId, int parentId, int id)
		{
			return this.JsonMessageResult(_formatService.RestoreObjectFormatVersion(id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.MultipleRemovePageObjectFormatVersion)]
		[BackendActionContext(ActionCode.MultipleRemovePageObjectFormatVersion)]
		[BackendActionLog]
		[Record]
		public ActionResult MultipleRemovePageObjectFormatVersion(int parentId, int[] IDs)
		{
			return this.JsonMessageResult(_formatService.MultipleRemoveObjectFormatVersion(IDs));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.CaptureLockTemplateObjectFormat)]
		[BackendActionContext(ActionCode.CaptureLockTemplateObjectFormat)]
		[BackendActionLog]
		public ActionResult CaptureLockTemplateObjectFormat(int id)
		{
			_formatService.CaptureLockTemplateObjectFormat(id);
			return this.JsonMessageResult(null);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.CaptureLockPageObjectFormat)]
		[BackendActionContext(ActionCode.CaptureLockPageObjectFormat)]
		[BackendActionLog]
		public ActionResult CaptureLockPageObjectFormat(int id)
		{
			_formatService.CaptureLockPageObjectFormat(id);
			return Json(null);
		}
    }
}

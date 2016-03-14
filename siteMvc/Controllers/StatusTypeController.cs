using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.StatusType;
using Telerik.Web.Mvc;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class StatusTypeController : QPController
    {
		IStatusTypeService _statusTypeService;

		public StatusTypeController(IStatusTypeService statusTypeService)
		{
			this._statusTypeService = statusTypeService;
		}

		#region	list actions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.StatusTypes)]
		[BackendActionContext(ActionCode.StatusTypes)]
		public ActionResult Index(string tabId, int parentId)
		{
			StatusTypeInitListResult result = _statusTypeService.InitList(parentId);
			StatusTypeListViewModel model = StatusTypeListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.StatusTypes)]
		[BackendActionContext(ActionCode.StatusTypes)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<StatusTypeListItem> serviceResult = _statusTypeService.GetStatusesBySiteId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.StatusTypeProperties)]
		[BackendActionContext(ActionCode.StatusTypeProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			StatusType status = _statusTypeService.ReadProperties(id);
			StatusTypeViewModel model = StatusTypeViewModel.Create(status, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateStatusType)]
		[BackendActionContext(ActionCode.UpdateStatusType)]
		[BackendActionLog]
		[Record(ActionCode.StatusTypeProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			StatusType status = _statusTypeService.ReadPropertiesForUpdate(id);
			StatusTypeViewModel model = StatusTypeViewModel.Create(status, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _statusTypeService.UpdateProperties(model.Data);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateVisualEditorPlugin });
			}
			else
				return JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewStatusType)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewStatusType)]
		public ActionResult New(string tabId, int parentId)
		{
			StatusType status = _statusTypeService.NewStatusTypeProperties(parentId);
			StatusTypeViewModel model = StatusTypeViewModel.Create(status, tabId, parentId);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewStatusType)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewStatusType)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			StatusType status = _statusTypeService.NewStatusTypePropertiesForUpdate(parentId);
			StatusTypeViewModel model = StatusTypeViewModel.Create(status, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _statusTypeService.SaveProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveStatusType });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveStatusType)]
		[BackendActionContext(ActionCode.RemoveStatusType)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = _statusTypeService.Remove(id);
			return Json(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectStatusesForWorkflow)]
		[BackendActionContext(ActionCode.MultipleSelectStatusesForWorkflow)]
		public ActionResult MultipleSelectForWorkflow(string tabId, int parentId, int[] IDs)
		{
			StatusTypeInitListResult result = _statusTypeService.InitList(parentId);
			StatusTypeSelectableListViewModel model = new StatusTypeSelectableListViewModel(result, tabId, parentId, IDs);
			return this.JsonHtml("MultiSelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectStatusesForWorkflow)]
		[BackendActionContext(ActionCode.MultipleSelectStatusesForWorkflow)]
		public ActionResult _MultipleSelectForWorkflow(string tabId, string IDs, GridCommand command, int parentId)
		{
			ListResult<StatusTypeListItem> serviceResult = _statusTypeService.ListForWorkflow(command.GetListCommand(), Converter.ToInt32Collection(IDs, ','), parentId);			
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
    }
}

using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VisualEditorStyleController : QPController
    {
        IVisualEditorService _visualEditorService;

		public VisualEditorStyleController(IVisualEditorService visualEditorService)
		{
			this._visualEditorService = visualEditorService;
		}

		#region	list actions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.VisualEditorStyles)]
		[BackendActionContext(ActionCode.VisualEditorStyles)]
		public ActionResult Index(string tabId, int parentId)
        {
			VisualEditorStyleInitListResult result = _visualEditorService.InitVisualEditorStyleList(parentId);
			VisualEditorStyleListViewModel model = VisualEditorStyleListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
        }

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.VisualEditorStyles)]
		[BackendActionContext(ActionCode.VisualEditorStyles)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<VisualEditorStyleListItem> serviceResult = _visualEditorService.GetVisualEditorStyles(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.VisualEditorStyleProperties)]
		[BackendActionContext(ActionCode.VisualEditorStyleProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			VisualEditorStyle style = _visualEditorService.ReadVisualEditorStyleProperties(id);
			ViewData[SpecialKeys.IsEntityReadOnly] = style.IsSystem;
			VisualEditorStyleViewModel model = VisualEditorStyleViewModel.Create(style, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateVisualEditorStyle)]
		[BackendActionContext(ActionCode.UpdateVisualEditorStyle)]
		[BackendActionLog]
		[Record(ActionCode.VisualEditorStyleProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			VisualEditorStyle style = _visualEditorService.ReadVisualEditorStylePropertiesForUpdate(id);
			VisualEditorStyleViewModel model = VisualEditorStyleViewModel.Create(style, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _visualEditorService.UpdateVisualEditorStyleProperties(model.Data);
			    return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateVisualEditorPlugin });
			}
			else
			    return JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveVisualEditorStyle)]
		[BackendActionContext(ActionCode.RemoveVisualEditorStyle)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = _visualEditorService.RemoveVisualEditorStyle(id);
			return JsonMessageResult(result);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewVisualEditorStyle)]
		[BackendActionContext(ActionCode.AddNewVisualEditorStyle)]
		public ActionResult New(string tabId, int parentId)
		{
			VisualEditorStyle style = _visualEditorService.NewVisualEditorStyleProperties(parentId);
			VisualEditorStyleViewModel model = VisualEditorStyleViewModel.Create(style, tabId, parentId);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewVisualEditorStyle)]
		[BackendActionContext(ActionCode.AddNewVisualEditorStyle)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			VisualEditorStyle style = _visualEditorService.NewVisualEditorStylePropertiesForUpdate(parentId);
			VisualEditorStyleViewModel model = VisualEditorStyleViewModel.Create(style, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _visualEditorService.SaveVisualEditorStyleProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveVisualEditorStyle });
			}
			else
				return JsonHtml("Properties", model);
		}
    }
}

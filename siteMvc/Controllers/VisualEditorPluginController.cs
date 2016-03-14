using System.Web.Mvc;
using System.Linq;
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
    public class VisualEditorPluginController : QPController
    {
		IVisualEditorService _visualEditorService;

		public VisualEditorPluginController(IVisualEditorService visualEditorService)
		{
			this._visualEditorService = visualEditorService;
		}

		#region	list actions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.VisualEditorPlugins)]
		[BackendActionContext(ActionCode.VisualEditorPlugins)]
		public ActionResult Index(string tabId, int parentId)
        {
			VisualEditorInitListResult result = _visualEditorService.InitList(parentId);
			VisualEditorListViewModel model = VisualEditorListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
        }

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.VisualEditorPlugins)]
		[BackendActionContext(ActionCode.VisualEditorPlugins)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{			
			ListResult<VisualEditorPluginListItem> serviceResult = _visualEditorService.GetVisualEditorPlugins(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.VisualEditorPluginProperties)]
		[BackendActionContext(ActionCode.VisualEditorPluginProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			VisualEditorPlugin plugin = _visualEditorService.ReadVisualEditorPluginProperties(id);
			VisualEditorPluginViewModel model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateVisualEditorPlugin)]
		[BackendActionContext(ActionCode.UpdateVisualEditorPlugin)]
		[BackendActionLog]
		[Record(ActionCode.VisualEditorPluginProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			VisualEditorPlugin plugin = _visualEditorService.ReadVisualEditorPluginPropertiesForUpdate(id);
			VisualEditorPluginViewModel model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				int[] oldIds = model.Data.VeCommands.Select(n => n.Id).ToArray();
				model.Data = _visualEditorService.UpdateVisualEditorProperties(model.Data);
				int[] newIds = model.Data.VeCommands.Select(n => n.Id).ToArray();
				this.PersistResultId(model.Data.Id);
				this.PersistCommandIds(oldIds, newIds);
			    return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateVisualEditorPlugin });
			}
			else
			    return JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveVisualEditorPlugin)]
		[BackendActionContext(ActionCode.RemoveVisualEditorPlugin)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = _visualEditorService.Remove(id);
			return JsonMessageResult(result);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewVisualEditorPlugin)]
		[BackendActionContext(ActionCode.AddNewVisualEditorPlugin)]
		public ActionResult New(string tabId, int parentId)
		{
			VisualEditorPlugin plugin = _visualEditorService.NewVisualEditorPluginProperties(parentId);
			VisualEditorPluginViewModel model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewVisualEditorPlugin)]
		[BackendActionContext(ActionCode.AddNewVisualEditorPlugin)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			VisualEditorPlugin plugin = _visualEditorService.NewVisualEditorPluginPropertiesForUpdate(parentId);
			VisualEditorPluginViewModel model = VisualEditorPluginViewModel.Create(plugin, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _visualEditorService.SaveVisualEditorPluginProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				this.PersistCommandIds(null, model.Data.VeCommands.Select(n => n.Id).ToArray());
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveVisualEditorPlugin });
			}
			else
				return JsonHtml("Properties", model);
		}
	}
}

using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Telerik.Web.Mvc;
using Quantumart.QP8.WebMvc.Controllers.Base;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class WorkflowPermissionController : PermissionControllerBase
    {
		public WorkflowPermissionController(IPermissionService service) : base(service) { }

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.WorkflowPermissions)]
		[BackendActionContext(ActionCode.WorkflowPermissions)]
		public override ActionResult Index(string tabId, int parentId)
		{
			return base.Index(tabId, parentId);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.WorkflowPermissions)]
		[BackendActionContext(ActionCode.WorkflowPermissions)]
		public override ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			return base._Index(tabId, parentId, command);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.AddNewWorkflowPermission)]
		[BackendActionContext(ActionCode.AddNewWorkflowPermission)]
		public override ActionResult New(string tabId, int parentId)
		{
			return base.New(tabId, parentId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.AddNewWorkflowPermission)]
		[BackendActionContext(ActionCode.AddNewWorkflowPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			return base.New(tabId, parentId, collection);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.WorkflowPermissionProperties)]
		[BackendActionContext(ActionCode.WorkflowPermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			return base.Properties(tabId, parentId, id, successfulActionCode);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.UpdateWorkflowPermission)]
		[BackendActionContext(ActionCode.UpdateWorkflowPermission)]
		[BackendActionLog]
		[Record(ActionCode.WorkflowPermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			return base.Properties(tabId, parentId, id, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.MultipleRemoveWorkflowPermission)]
		[BackendActionContext(ActionCode.MultipleRemoveWorkflowPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			return base.MultipleRemove(parentId, IDs);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.RemoveWorkflowPermission)]
		[BackendActionContext(ActionCode.RemoveWorkflowPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult Remove(int parentId, int id)
		{
			return base.Remove(parentId, id);
		}


		protected override string ControllerName { get { return "WorkflowPermission"; } }
	}
}

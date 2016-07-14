using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Telerik.Web.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class ActionPermissionController : PermissionWithChangeControllerBase
    {
		public ActionPermissionController(IPermissionService service, IActionPermissionChangeService changeService) : base(service, changeService) { }


		#region Action Permissions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ActionPermissions)]
		[BackendActionContext(ActionCode.ActionPermissions)]
		public override ActionResult Index(string tabId, int parentId)
		{
			return base.Index(tabId, parentId);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.ActionPermissions)]
		[BackendActionContext(ActionCode.ActionPermissions)]
		public override ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			return base._Index(tabId, parentId, command);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewActionPermission)]		
		[BackendActionContext(ActionCode.AddNewActionPermission)]
		public override ActionResult New(string tabId, int parentId)
		{
			return base.New(tabId, parentId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewActionPermission)]
		[BackendActionContext(ActionCode.AddNewActionPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			return base.New(tabId, parentId, collection);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ActionPermissionProperties)]
		[BackendActionContext(ActionCode.ActionPermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			return base.Properties(tabId, parentId, id, successfulActionCode);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateActionPermission)]
		[BackendActionContext(ActionCode.UpdateActionPermission)]
		[BackendActionLog]
		[Record(ActionCode.ActionPermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			return base.Properties(tabId, parentId, id, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveActionPermission)]
		[BackendActionContext(ActionCode.MultipleRemoveActionPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			return base.MultipleRemove(parentId, IDs);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveActionPermission)]
		[BackendActionContext(ActionCode.RemoveActionPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult Remove(int parentId, int id)
		{
			return base.Remove(parentId, id);
		} 
		#endregion

		#region Change
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ChangeActionPermission)]
		[BackendActionContext(ActionCode.ChangeActionPermission)]
		public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack)
		{
			return base.Change(tabId, parentId, userId, groupId, isPostBack);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateActionPermissionChanges)]
		[BackendActionContext(ActionCode.UpdateActionPermissionChanges)]
		[BackendActionLog]
		public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, FormCollection collection)
		{
			return base.Change(tabId, parentId, userId, groupId, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveEntityTypePermissionChanges)]
		[BackendActionContext(ActionCode.RemoveEntityTypePermissionChanges)]
		[BackendActionLog]
		public override ActionResult RemoveForNode(int parentId, int? userId, int? groupId)
		{
			return base.RemoveForNode(parentId, userId, groupId);
		}
		#endregion

		protected override string ControllerName { get { return "ActionPermission"; } }
	}
}

using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Telerik.Web.Mvc;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.ActionPermissions;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class EntityTypePermissionController : PermissionWithChangeControllerBase
    {
		public EntityTypePermissionController(IPermissionService service, IActionPermissionChangeService changeService) : base(service, changeService) { }

		#region Entity Permissions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.EntityTypePermissions)]
		[BackendActionContext(ActionCode.EntityTypePermissions)]
		public override ActionResult Index(string tabId, int parentId)
		{
			return base.Index(tabId, parentId);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.EntityTypePermissions)]
		[BackendActionContext(ActionCode.EntityTypePermissions)]
		public override ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			return base._Index(tabId, parentId, command);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewEntityTypePermission)]		
		[BackendActionContext(ActionCode.AddNewEntityTypePermission)]
		public override ActionResult New(string tabId, int parentId)
		{
			return base.New(tabId, parentId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewEntityTypePermission)]
		[BackendActionContext(ActionCode.AddNewEntityTypePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			return base.New(tabId, parentId, collection);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.EntityTypePermissionProperties)]
		[BackendActionContext(ActionCode.EntityTypePermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			return base.Properties(tabId, parentId, id, successfulActionCode);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateEntityTypePermission)]
		[BackendActionContext(ActionCode.UpdateEntityTypePermission)]
		[BackendActionLog]
		[Record(ActionCode.EntityTypePermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			return base.Properties(tabId, parentId, id, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveEntityTypePermission)]
		[BackendActionContext(ActionCode.MultipleRemoveEntityTypePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			return base.MultipleRemove(parentId, IDs);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveEntityTypePermission)]
		[BackendActionContext(ActionCode.RemoveEntityTypePermission)]
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
		[ActionAuthorize(ActionCode.ChangeEntityTypePermission)]
		[BackendActionContext(ActionCode.ChangeEntityTypePermission)]
		public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack)
		{
			return base.Change(tabId, parentId, userId, groupId, isPostBack);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateEntityTypePermissionChanges)]
		[BackendActionContext(ActionCode.UpdateEntityTypePermissionChanges)]
		[BackendActionLog]
		public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, FormCollection collection)
		{
			return base.Change(tabId, parentId, userId, groupId, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveActionPermissionChanges)]
		[BackendActionContext(ActionCode.RemoveActionPermissionChanges)]
		[BackendActionLog]
		public override ActionResult RemoveForNode(int parentId, int? userId, int? groupId)
		{
			return base.RemoveForNode(parentId, userId, groupId);
		}
		#endregion

		protected override string ControllerName { get { return "EntityTypePermission"; } }

    }
}

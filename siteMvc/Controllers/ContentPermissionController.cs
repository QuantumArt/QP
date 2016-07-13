using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Telerik.Web.Mvc;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public sealed class ContentPermissionController : PermissionWithChildControllerBase
    {		
		public ContentPermissionController(IPermissionService service, IChildEntityPermissionService childContentService)
			: base(service, childContentService) 
		{
			
		}

		#region Content Permissions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ContentPermissions)]
		[BackendActionContext(ActionCode.ContentPermissions)]
		public override ActionResult Index(string tabId, int parentId)
		{
			return base.Index(tabId, parentId);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.ContentPermissions)]
		[BackendActionContext(ActionCode.ContentPermissions)]
		public override ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			return base._Index(tabId, parentId, command);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewContentPermission)]
		[BackendActionContext(ActionCode.AddNewContentPermission)]
		public override ActionResult New(string tabId, int parentId)
		{
			return base.New(tabId, parentId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewContentPermission)]
		[BackendActionContext(ActionCode.AddNewContentPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			return base.New(tabId, parentId, collection);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ContentPermissionProperties)]
		[BackendActionContext(ActionCode.ContentPermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			return base.Properties(tabId, parentId, id, successfulActionCode);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateContentPermission)]
		[BackendActionContext(ActionCode.UpdateContentPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			return base.Properties(tabId, parentId, id, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveContentPermission)]
		[BackendActionContext(ActionCode.MultipleRemoveContentPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			return base.MultipleRemove(parentId, IDs);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveContentPermission)]
		[BackendActionContext(ActionCode.RemoveContentPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult Remove(int parentId, int id)
		{
			return base.Remove(parentId, id);
		} 
		#endregion

		#region Child Content Permissions

		#region list
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ChildContentPermissions)]
		[BackendActionContext(ActionCode.ChildContentPermissions)]
		public override ActionResult ChildIndex(string tabId, int parentId)
		{
			return base.ChildIndex(tabId, parentId);
		}		

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.ChildContentPermissions)]
		[BackendActionContext(ActionCode.ChildContentPermissions)]
		public override ActionResult _ChildIndex(string tabId, int parentId, int? userId, int? groupId, GridCommand command)
		{
			return base._ChildIndex(tabId, parentId, userId, groupId, command);
		}
		#endregion

		#region Changes
		#region MultipleChange
		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleChangeChildContentPermissions)]
		[BackendActionContext(ActionCode.MultipleChangeChildContentPermissions)]
		public override ActionResult MultipleChangeAsChild(string tabId, int parentId, int[] IDs, int? userId, int? groupId)
		{
			return base.MultipleChangeAsChild(tabId, parentId, IDs, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleChangeChildContentPermissions)]
		[BackendActionContext(ActionCode.MultipleChangeChildContentPermissions)]
		[BackendActionLog]
		[Record(ActionCode.MultipleChangeChildContentPermissions)]
		public override ActionResult SaveMultipleChangeAsChild(string tabId, int parentId, FormCollection collection)
		{
			return base.SaveMultipleChangeAsChild(tabId, parentId, collection);
		}
		#endregion

		#region AllChange
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ChangeAllChildContentPermissions)]
		[BackendActionContext(ActionCode.ChangeAllChildContentPermissions)]
		public override ActionResult AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId)
		{
			return base.AllChangeAsChild(tabId, parentId, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.ChangeAllChildContentPermissions)]
		[BackendActionContext(ActionCode.ChangeAllChildContentPermissions)]
		[BackendActionLog]
		[Record(ActionCode.ChangeAllChildContentPermissions)]
		public override ActionResult AllChangeAsChild(string tabId, int parentId, FormCollection collection)
		{
			return base.AllChangeAsChild(tabId, parentId, collection);
		}
		#endregion

		#region Change
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ChangeChildContentPermission)]
		[BackendActionContext(ActionCode.ChangeChildContentPermission)]
		public override ActionResult ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId)
		{
			return base.ChangeAsChild(tabId, parentId, id, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.ChangeChildContentPermission)]
		[BackendActionContext(ActionCode.ChangeChildContentPermission)]
		[BackendActionLog]
		[Record(ActionCode.ChangeChildContentPermission)]
		public override ActionResult ChangeAsChild(string tabId, int parentId, FormCollection collection)
		{
			return base.ChangeAsChild(tabId, parentId, collection);
		}
		#endregion		
		#endregion

		#region Remove
		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveChildContentPermissions)]
		[BackendActionContext(ActionCode.MultipleRemoveChildContentPermissions)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemoveAsChild(int parentId, int[] IDs, int? userId, int? groupId)
		{
			return base.MultipleRemoveAsChild(parentId, IDs, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveAllChildContentPermissions)]
		[BackendActionContext(ActionCode.RemoveAllChildContentPermissions)]
		[BackendActionLog]
		[Record]
		public override ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId)
		{
			return base.AllRemoveAsChild(parentId, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveChildContentPermission)]
		[BackendActionContext(ActionCode.RemoveChildContentPermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId)
		{
			return base.RemoveAsChild(parentId, id, userId, groupId);
		}
		#endregion

		protected override string SaveChildPermissionAction { get { return ActionCode.SaveChildContentPermission; } }
		protected override string MultipleChangeAction { get { return ActionCode.MultipleChangeChildContentPermissions; } }
		protected override string AllChangeAction { get { return ActionCode.ChangeAllChildContentPermissions; } }
		protected override string ChangeAction { get { return ActionCode.ChangeChildContentPermission; } }

		#endregion

		protected override string ControllerName { get { return "ContentPermission"; } }		
		
	}
}

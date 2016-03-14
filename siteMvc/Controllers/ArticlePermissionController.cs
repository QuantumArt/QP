using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Telerik.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.Controllers.Base;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class ArticlePermissionController : PermissionWithChildControllerBase
    {
		public ArticlePermissionController(IPermissionService service, IChildEntityPermissionService childContentService) : base(service, childContentService) { }

		#region Article Permissions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ArticlePermissions)]
		[BackendActionContext(ActionCode.ArticlePermissions)]
		public override ActionResult Index(string tabId, int parentId)
		{
			return base.Index(tabId, parentId);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.ArticlePermissions)]
		[BackendActionContext(ActionCode.ArticlePermissions)]
		public override ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			return base._Index(tabId, parentId, command);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewArticlePermission)]		
		[BackendActionContext(ActionCode.AddNewArticlePermission)]
		public override ActionResult New(string tabId, int parentId)
		{
			return base.New(tabId, parentId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewArticlePermission)]
		[BackendActionContext(ActionCode.AddNewArticlePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			return base.New(tabId, parentId, collection);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ArticlePermissionProperties)]
		[BackendActionContext(ActionCode.ArticlePermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			return base.Properties(tabId, parentId, id, successfulActionCode);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateArticlePermission)]
		[BackendActionContext(ActionCode.UpdateArticlePermission)]
		[BackendActionLog]
		[Record(ActionCode.ArticlePermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			return base.Properties(tabId, parentId, id, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveArticlePermission)]
		[BackendActionContext(ActionCode.MultipleRemoveArticlePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			return base.MultipleRemove(parentId, IDs);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveArticlePermission)]
		[BackendActionContext(ActionCode.RemoveArticlePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult Remove(int parentId, int id)
		{
			return base.Remove(parentId, id);
		} 
		#endregion


		#region Child Article Permissions

		#region list
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ChildArticlePermissions)]
		[BackendActionContext(ActionCode.ChildArticlePermissions)]
		public override ActionResult ChildIndex(string tabId, int parentId)
		{
			return base.ChildIndex(tabId, parentId);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.ChildArticlePermissions)]
		[BackendActionContext(ActionCode.ChildArticlePermissions)]
		public override ActionResult _ChildIndex(string tabId, int parentId, int? userId, int? groupId, GridCommand command)
		{
			return base._ChildIndex(tabId, parentId, userId, groupId, command);
		}
		#endregion

		#region Changes
		#region MultipleChange
		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleChangeChildArticlePermissions)]
		[BackendActionContext(ActionCode.MultipleChangeChildArticlePermissions)]
		public override ActionResult MultipleChangeAsChild(string tabId, int parentId, int[] IDs, int? userId, int? groupId)
		{
			return base.MultipleChangeAsChild(tabId, parentId, IDs, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleChangeChildArticlePermissions)]
		[BackendActionContext(ActionCode.MultipleChangeChildArticlePermissions)]
		[BackendActionLog]
		[Record]
		public override ActionResult SaveMultipleChangeAsChild(string tabId, int parentId, FormCollection collection)
		{
			return base.SaveMultipleChangeAsChild(tabId, parentId, collection);
		}
		#endregion

		#region AllChange
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ChangeAllChildArticlePermissions)]
		[BackendActionContext(ActionCode.ChangeAllChildArticlePermissions)]
		public override ActionResult AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId)
		{
			return base.AllChangeAsChild(tabId, parentId, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.ChangeAllChildArticlePermissions)]
		[BackendActionContext(ActionCode.ChangeAllChildArticlePermissions)]
		[BackendActionLog]
		[Record(ActionCode.ChangeAllChildArticlePermissions)]
		public override ActionResult AllChangeAsChild(string tabId, int parentId, FormCollection collection)
		{
			return base.AllChangeAsChild(tabId, parentId, collection);
		}
		#endregion

		#region Change
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ChangeChildArticlePermission)]
		[BackendActionContext(ActionCode.ChangeChildArticlePermission)]
		public override ActionResult ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId)
		{
			return base.ChangeAsChild(tabId, parentId, id, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.ChangeChildArticlePermission)]
		[BackendActionContext(ActionCode.ChangeChildArticlePermission)]
		[BackendActionLog]
		[Record(ActionCode.ChangeChildArticlePermission)]
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
		[ActionAuthorize(ActionCode.MultipleRemoveChildArticlePermissions)]
		[BackendActionContext(ActionCode.MultipleRemoveChildArticlePermissions)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemoveAsChild(int parentId, int[] IDs, int? userId, int? groupId)
		{
			return base.MultipleRemoveAsChild(parentId, IDs, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveAllChildArticlePermissions)]
		[BackendActionContext(ActionCode.RemoveAllChildArticlePermissions)]
		[BackendActionLog]
		[Record]
		public override ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId)
		{
			return base.AllRemoveAsChild(parentId, userId, groupId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveChildArticlePermission)]
		[BackendActionContext(ActionCode.RemoveChildArticlePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId)
		{
			return base.RemoveAsChild(parentId, id, userId, groupId);
		}
		#endregion

		protected override string SaveChildPermissionAction { get { return ActionCode.SaveChildArticlePermission; } }
		protected override string MultipleChangeAction { get { return ActionCode.MultipleChangeChildArticlePermissions; } }
		protected override string AllChangeAction { get { return ActionCode.ChangeAllChildArticlePermissions; } }
		protected override string ChangeAction { get { return ActionCode.ChangeChildArticlePermission; } }

		#endregion

		protected override string ControllerName { get { return "ArticlePermission"; } }
	}
}

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
	public class SitePermissionController : PermissionControllerBase
    {		
		public SitePermissionController(IPermissionService service) : base(service){}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SitePermissions)]
		[BackendActionContext(ActionCode.SitePermissions)]
		public override ActionResult Index(string tabId, int parentId)
		{
			return base.Index(tabId, parentId);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SitePermissions)]
		[BackendActionContext(ActionCode.SitePermissions)]
		public override ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			return base._Index(tabId, parentId, command);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewSitePermission)]		
		[BackendActionContext(ActionCode.AddNewSitePermission)]
		public override ActionResult New(string tabId, int parentId)
		{
			return base.New(tabId, parentId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewSitePermission)]
		[BackendActionContext(ActionCode.AddNewSitePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			return base.New(tabId, parentId, collection);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SitePermissionProperties)]
		[BackendActionContext(ActionCode.SitePermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			return base.Properties(tabId, parentId, id, successfulActionCode);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateSitePermission)]
		[BackendActionContext(ActionCode.UpdateSitePermission)]
		[BackendActionLog]
		[Record(ActionCode.SitePermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			return base.Properties(tabId, parentId, id, collection);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveSitePermission)]
		[BackendActionContext(ActionCode.MultipleRemoveSitePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			return base.MultipleRemove(parentId, IDs);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveSitePermission)]
		[BackendActionContext(ActionCode.RemoveSitePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult Remove(int parentId, int id)
		{
			return base.Remove(parentId, id);
		}


		protected override string ControllerName { get { return "SitePermission"; } }
	}
}

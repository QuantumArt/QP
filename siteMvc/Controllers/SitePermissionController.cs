using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Telerik.Web.Mvc;
using Quantumart.QP8.WebMvc.Controllers.Base;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class SitePermissionController : PermissionControllerBase
    {
		public SitePermissionController(IPermissionService service) : base(service){}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
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
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.AddNewSitePermission)]
		[BackendActionContext(ActionCode.AddNewSitePermission)]
		public override ActionResult New(string tabId, int parentId)
		{
			return base.New(tabId, parentId);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.AddNewSitePermission)]
		[BackendActionContext(ActionCode.AddNewSitePermission)]
		[BackendActionLog]
		[Record]
		public override ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			return base.New(tabId, parentId, collection);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.SitePermissionProperties)]
		[BackendActionContext(ActionCode.SitePermissionProperties)]
		public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			return base.Properties(tabId, parentId, id, successfulActionCode);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
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
		[ConnectionScope()]
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
		[ConnectionScope()]
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

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class SiteFolderPermissionController : PermissionControllerBase
    {
        public SiteFolderPermissionController(IPermissionService service) : base(service) { }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderPermissions)]
        [BackendActionContext(ActionCode.SiteFolderPermissions)]
        public override ActionResult Index(string tabId, int parentId)
        {
            return base.Index(tabId, parentId);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SiteFolderPermissions)]
        [BackendActionContext(ActionCode.SiteFolderPermissions)]
        public override ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            return base._Index(tabId, parentId, command);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionContext(ActionCode.AddNewSiteFolderPermission)]
        public override ActionResult New(string tabId, int parentId)
        {
            return base.New(tabId, parentId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionContext(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            return base.New(tabId, parentId, collection);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderPermissionProperties)]
        [BackendActionContext(ActionCode.SiteFolderPermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.UpdateSiteFolderPermission)]
        [BackendActionContext(ActionCode.UpdateSiteFolderPermission)]
        [BackendActionLog]
        [Record(ActionCode.SiteFolderPermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            return base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.MultipleRemoveSiteFolderPermission)]
        [BackendActionContext(ActionCode.MultipleRemoveSiteFolderPermission)]
        [BackendActionLog]
        [Record]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemove(int parentId, int[] IDs)
        {
            return base.MultipleRemove(parentId, IDs);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.RemoveSiteFolderPermission)]
        [BackendActionContext(ActionCode.RemoveSiteFolderPermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult Remove(int parentId, int id)
        {
            return base.Remove(parentId, id);
        }

        protected override string ControllerName => "SiteFolderPermission";
    }
}

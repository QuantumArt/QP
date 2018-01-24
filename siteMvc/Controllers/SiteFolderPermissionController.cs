using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class SiteFolderPermissionController : PermissionControllerBase
    {
        public SiteFolderPermissionController(IPermissionService service)
            : base(service)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderPermissions)]
        [BackendActionContext(ActionCode.SiteFolderPermissions)]
        public override ActionResult Index(string tabId, int parentId) => base.Index(tabId, parentId);

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SiteFolderPermissions)]
        [BackendActionContext(ActionCode.SiteFolderPermissions)]
        public override ActionResult _Index(string tabId, int parentId, GridCommand command) => base._Index(tabId, parentId, command);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionContext(ActionCode.AddNewSiteFolderPermission)]
        public override ActionResult New(string tabId, int parentId) => base.New(tabId, parentId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionContext(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult New(string tabId, int parentId, FormCollection collection) => base.New(tabId, parentId, collection);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderPermissionProperties)]
        [BackendActionContext(ActionCode.SiteFolderPermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode) => base.Properties(tabId, parentId, id, successfulActionCode);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateSiteFolderPermission)]
        [BackendActionContext(ActionCode.UpdateSiteFolderPermission)]
        [BackendActionLog]
        [Record(ActionCode.SiteFolderPermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection) => base.Properties(tabId, parentId, id, collection);

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveSiteFolderPermission)]
        [BackendActionContext(ActionCode.MultipleRemoveSiteFolderPermission)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemove(int parentId, int[] IDs) => base.MultipleRemove(parentId, IDs);

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveSiteFolderPermission)]
        [BackendActionContext(ActionCode.RemoveSiteFolderPermission)]
        [BackendActionLog]
        public override ActionResult Remove(int parentId, int id) => base.Remove(parentId, id);

        protected override string ControllerName => "SiteFolderPermission";
    }
}

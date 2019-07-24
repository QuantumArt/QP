using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ActionPermissionController : PermissionWithChangeControllerBase
    {
        public ActionPermissionController(IPermissionService service, IActionPermissionChangeService changeService)
            : base(service, changeService)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionPermissions)]
        [BackendActionContext(ActionCode.ActionPermissions)]
        public override ActionResult Index(string tabId, int parentId) => base.Index(tabId, parentId);

        [HttpPost]
        [ActionAuthorize(ActionCode.ActionPermissions)]
        [BackendActionContext(ActionCode.ActionPermissions)]
        public override ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            string orderBy) => base._Index(
                tabId,
                parentId,
                page,
                pageSize,
                orderBy);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewActionPermission)]
        [BackendActionContext(ActionCode.AddNewActionPermission)]
        public override ActionResult New(string tabId, int parentId) => base.New(tabId, parentId);

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewActionPermission)]
        [BackendActionContext(ActionCode.AddNewActionPermission)]
        [BackendActionLog]
        public override ActionResult New(string tabId, int parentId, FormCollection collection) => base.New(tabId, parentId, collection);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionPermissionProperties)]
        [BackendActionContext(ActionCode.ActionPermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode) => base.Properties(tabId, parentId, id, successfulActionCode);

        [HttpPost, Record(ActionCode.ActionPermissionProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateActionPermission)]
        [BackendActionContext(ActionCode.UpdateActionPermission)]
        [BackendActionLog]
        public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection) => base.Properties(tabId, parentId, id, collection);

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveActionPermission)]
        [BackendActionContext(ActionCode.MultipleRemoveActionPermission)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemove(int parentId, int[] IDs) => base.MultipleRemove(parentId, IDs);

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveActionPermission)]
        [BackendActionContext(ActionCode.RemoveActionPermission)]
        [BackendActionLog]
        public override ActionResult Remove(int parentId, int id) => base.Remove(parentId, id);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeActionPermission)]
        [BackendActionContext(ActionCode.ChangeActionPermission)]
        public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack) => base.Change(tabId, parentId, userId, groupId, isPostBack);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateActionPermissionChanges)]
        [BackendActionContext(ActionCode.UpdateActionPermissionChanges)]
        [BackendActionLog]
        public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, FormCollection collection) => base.Change(tabId, parentId, userId, groupId, collection);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveEntityTypePermissionChanges)]
        [BackendActionContext(ActionCode.RemoveEntityTypePermissionChanges)]
        [BackendActionLog]
        public override ActionResult RemoveForNode(int parentId, int? userId, int? groupId) => base.RemoveForNode(parentId, userId, groupId);

        protected override string ControllerName => "ActionPermission";
    }
}

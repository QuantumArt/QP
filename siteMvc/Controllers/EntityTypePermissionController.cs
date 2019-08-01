using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class EntityTypePermissionController : PermissionWithChangeControllerBase
    {
        public EntityTypePermissionController(IPermissionService service, IActionPermissionChangeService changeService)
            : base(service, changeService)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.EntityTypePermissions)]
        [BackendActionContext(ActionCode.EntityTypePermissions)]
        public override ActionResult Index(string tabId, int parentId) => base.Index(tabId, parentId);

        [HttpPost]
        [ActionAuthorize(ActionCode.EntityTypePermissions)]
        [BackendActionContext(ActionCode.EntityTypePermissions)]
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
        [ActionAuthorize(ActionCode.AddNewEntityTypePermission)]
        [BackendActionContext(ActionCode.AddNewEntityTypePermission)]
        public override ActionResult New(string tabId, int parentId) => base.New(tabId, parentId);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewEntityTypePermission)]
        [BackendActionContext(ActionCode.AddNewEntityTypePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult New(string tabId, int parentId, FormCollection collection) => base.New(tabId, parentId, collection);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.EntityTypePermissionProperties)]
        [BackendActionContext(ActionCode.EntityTypePermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode) => base.Properties(tabId, parentId, id, successfulActionCode);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateEntityTypePermission)]
        [BackendActionContext(ActionCode.UpdateEntityTypePermission)]
        [BackendActionLog]
        [Record(ActionCode.EntityTypePermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, FormCollection collection) => base.Properties(tabId, parentId, id, collection);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveEntityTypePermission)]
        [BackendActionContext(ActionCode.MultipleRemoveEntityTypePermission)]
        [BackendActionLog]
        [Record]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemove(int parentId, int[] IDs) => base.MultipleRemove(parentId, IDs);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveEntityTypePermission)]
        [BackendActionContext(ActionCode.RemoveEntityTypePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult Remove(int parentId, int id) => base.Remove(parentId, id);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeEntityTypePermission)]
        [BackendActionContext(ActionCode.ChangeEntityTypePermission)]
        public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack) => base.Change(tabId, parentId, userId, groupId, isPostBack);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateEntityTypePermissionChanges)]
        [BackendActionContext(ActionCode.UpdateEntityTypePermissionChanges)]
        [BackendActionLog]
        public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, FormCollection collection) => base.Change(tabId, parentId, userId, groupId, collection);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveActionPermissionChanges)]
        [BackendActionContext(ActionCode.RemoveActionPermissionChanges)]
        [BackendActionLog]
        public override ActionResult RemoveForNode(int parentId, int? userId, int? groupId) => base.RemoveForNode(parentId, userId, groupId);

        protected override string ControllerName => "EntityTypePermission";
    }
}

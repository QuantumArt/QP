using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class EntityTypePermissionController : PermissionWithChangeControllerBase
    {
        public EntityTypePermissionController(IPermissionService service, IActionPermissionChangeService changeService) : base(service, changeService) { }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
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
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewEntityTypePermission)]
        [BackendActionContext(ActionCode.AddNewEntityTypePermission)]
        public override ActionResult New(string tabId, int parentId)
        {
            return base.New(tabId, parentId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewEntityTypePermission)]
        [BackendActionContext(ActionCode.AddNewEntityTypePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            return base.New(tabId, parentId, collection);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.EntityTypePermissionProperties)]
        [BackendActionContext(ActionCode.EntityTypePermissionProperties)]
        public override ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
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
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveEntityTypePermission)]
        [BackendActionContext(ActionCode.MultipleRemoveEntityTypePermission)]
        [BackendActionLog]
        [Record]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemove(int parentId, int[] IDs)
        {
            return base.MultipleRemove(parentId, IDs);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveEntityTypePermission)]
        [BackendActionContext(ActionCode.RemoveEntityTypePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult Remove(int parentId, int id)
        {
            return base.Remove(parentId, id);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeEntityTypePermission)]
        [BackendActionContext(ActionCode.ChangeEntityTypePermission)]
        public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack)
        {
            return base.Change(tabId, parentId, userId, groupId, isPostBack);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateEntityTypePermissionChanges)]
        [BackendActionContext(ActionCode.UpdateEntityTypePermissionChanges)]
        [BackendActionLog]
        public override ActionResult Change(string tabId, int parentId, int? userId, int? groupId, FormCollection collection)
        {
            return base.Change(tabId, parentId, userId, groupId, collection);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveActionPermissionChanges)]
        [BackendActionContext(ActionCode.RemoveActionPermissionChanges)]
        [BackendActionLog]
        public override ActionResult RemoveForNode(int parentId, int? userId, int? groupId)
        {
            return base.RemoveForNode(parentId, userId, groupId);
        }

        protected override string ControllerName => "EntityTypePermission";
    }
}

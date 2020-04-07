using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ActionPermissionController : PermissionWithChangeControllerBase
    {
        public ActionPermissionController(ActionPermissionService service, ActionPermissionChangeService changeService)
            : base(service, changeService)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionPermissions)]
        [BackendActionContext(ActionCode.ActionPermissions)]
        public override async Task<ActionResult> Index(string tabId, int parentId)
        {
            return await base.Index(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ActionPermissions)]
        [BackendActionContext(ActionCode.ActionPermissions)]
        public override ActionResult _Index(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            return base._Index(tabId, parentId, page, pageSize, orderBy);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewActionPermission)]
        [BackendActionContext(ActionCode.AddNewActionPermission)]
        public override async Task<ActionResult> New(string tabId, int parentId)
        {
            return await base.New(tabId, parentId);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewActionPermission)]
        [BackendActionContext(ActionCode.AddNewActionPermission)]
        [BackendActionLog]
        public override async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            return await base.New(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionPermissionProperties)]
        [BackendActionContext(ActionCode.ActionPermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return await base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost, Record(ActionCode.ActionPermissionProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateActionPermission)]
        [BackendActionContext(ActionCode.UpdateActionPermission)]
        [BackendActionLog]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            return await base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveActionPermission)]
        [BackendActionContext(ActionCode.MultipleRemoveActionPermission)]
        [BackendActionLog]
        public override ActionResult MultipleRemove(int parentId, [FromBody] SelectedItemsViewModel model)
        {
            return base.MultipleRemove(parentId, model);
        }

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
        public override async Task<ActionResult> Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack)
        {
            return await base.Change(tabId, parentId, userId, groupId, isPostBack);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateActionPermissionChanges)]
        [BackendActionContext(ActionCode.UpdateActionPermissionChanges)]
        [BackendActionLog]
        public override async Task<ActionResult> Change(string tabId, int parentId, int? userId, int? groupId, IFormCollection collection)
        {
            return await base.Change(tabId, parentId, userId, groupId, collection);
        }

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

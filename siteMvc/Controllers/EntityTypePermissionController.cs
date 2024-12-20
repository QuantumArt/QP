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
    public class EntityTypePermissionController : PermissionWithChangeControllerBase
    {
        public EntityTypePermissionController(EntityTypePermissionService service, EntityTypePermissionChangeService changeService)
            : base(service, changeService)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.EntityTypePermissions)]
        [BackendActionContext(ActionCode.EntityTypePermissions)]
        public override async Task<ActionResult> Index(string tabId, int parentId)
        {
            return await base.Index(tabId, parentId);
        }

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
        public override async Task<ActionResult> New(string tabId, int parentId)
        {
            return await base.New(tabId, parentId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewEntityTypePermission)]
        [BackendActionContext(ActionCode.AddNewEntityTypePermission)]
        [BackendActionLog]
        [Record]
        public override async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            return await base.New(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.EntityTypePermissionProperties)]
        [BackendActionContext(ActionCode.EntityTypePermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return await base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateEntityTypePermission)]
        [BackendActionContext(ActionCode.UpdateEntityTypePermission)]
        [BackendActionLog]
        [Record(ActionCode.EntityTypePermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            return await base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveEntityTypePermission)]
        [BackendActionContext(ActionCode.MultipleRemoveEntityTypePermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult MultipleRemove(int parentId, [FromBody] SelectedItemsViewModel model) => base.MultipleRemove(parentId, model);

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

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeEntityTypePermission)]
        [BackendActionContext(ActionCode.ChangeEntityTypePermission)]
        public override async Task<ActionResult> Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack)
        {
            return await base.Change(tabId, parentId, userId, groupId, isPostBack);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateEntityTypePermissionChanges)]
        [BackendActionContext(ActionCode.UpdateEntityTypePermissionChanges)]
        [BackendActionLog]
        [Record(ActionCode.ChangeEntityTypePermission)]
        public override async Task<ActionResult> Change(string tabId, int parentId, int? userId, int? groupId, IFormCollection collection)
        {
            var result = await base.Change(tabId, parentId, userId, groupId, collection);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveEntityTypePermissionChanges)]
        [BackendActionContext(ActionCode.RemoveEntityTypePermissionChanges)]
        [BackendActionLog]
        [Record]
        public override ActionResult RemoveForNode(int parentId, int? userId, int? groupId)
        {
            var result = base.RemoveForNode(parentId, userId, groupId);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        protected override string ControllerName => "EntityTypePermission";
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public sealed class ContentPermissionController : PermissionWithChildControllerBase
    {
        public ContentPermissionController(ContentPermissionService service, ChildContentPermissionService childContentService)
            : base(service, childContentService)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentPermissions)]
        [BackendActionContext(ActionCode.ContentPermissions)]
        public override async Task<ActionResult> Index(string tabId, int parentId)
        {
            return await base.Index(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ContentPermissions)]
        [BackendActionContext(ActionCode.ContentPermissions)]
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
        [ActionAuthorize(ActionCode.AddNewContentPermission)]
        [BackendActionContext(ActionCode.AddNewContentPermission)]
        public override async Task<ActionResult> New(string tabId, int parentId)
        {
            return await base.New(tabId, parentId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewContentPermission)]
        [BackendActionContext(ActionCode.AddNewContentPermission)]
        [BackendActionLog]
        [Record]
        public override async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            return await base.New(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ContentPermissionProperties)]
        [BackendActionContext(ActionCode.ContentPermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return await base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateContentPermission)]
        [BackendActionContext(ActionCode.UpdateContentPermission)]
        [BackendActionLog]
        [Record]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            return await base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveContentPermission)]
        [BackendActionContext(ActionCode.MultipleRemoveContentPermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult MultipleRemove(int parentId, [FromBody] SelectedItemsViewModel model)
        {
            return base.MultipleRemove(parentId, model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveContentPermission)]
        [BackendActionContext(ActionCode.RemoveContentPermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult Remove(int parentId, int id) => base.Remove(parentId, id);

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChildContentPermissions)]
        [BackendActionContext(ActionCode.ChildContentPermissions)]
        public override async Task<ActionResult> ChildIndex(string tabId, int parentId)
        {
            return await base.ChildIndex(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ChildContentPermissions)]
        [BackendActionContext(ActionCode.ChildContentPermissions)]
        public override ActionResult _ChildIndex(
            string tabId,
            int parentId,
            int? userId,
            int? groupId,
            int page,
            int pageSize,
            string orderBy) => base._ChildIndex(
                tabId,
                parentId,
                userId,
                groupId,
                page,
                pageSize,
                orderBy);

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleChangeChildContentPermissions)]
        [BackendActionContext(ActionCode.MultipleChangeChildContentPermissions)]
        public override async Task<ActionResult> MultipleChangeAsChild(string tabId, int parentId, [FromBody] SelectedItemsViewModel model, int? userId, int? groupId)
        {
            return await base.MultipleChangeAsChild(tabId, parentId, model, userId, groupId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleChangeChildContentPermissions)]
        [BackendActionContext(ActionCode.MultipleChangeChildContentPermissions)]
        [BackendActionLog]
        [Record(ActionCode.MultipleChangeChildContentPermissions)]
        public override async Task<ActionResult> SaveMultipleChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await base.SaveMultipleChangeAsChild(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeAllChildContentPermissions)]
        [BackendActionContext(ActionCode.ChangeAllChildContentPermissions)]
        public override async Task<ActionResult> AllChangeAsChild(string tabId, int parentId, int? userId, int? groupId)
        {
            return await base.AllChangeAsChild(tabId, parentId, userId, groupId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ChangeAllChildContentPermissions)]
        [BackendActionContext(ActionCode.ChangeAllChildContentPermissions)]
        [BackendActionLog]
        [Record(ActionCode.ChangeAllChildContentPermissions)]
        public override async Task<ActionResult> AllChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await base.AllChangeAsChild(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ChangeChildContentPermission)]
        [BackendActionContext(ActionCode.ChangeChildContentPermission)]
        public override async Task<ActionResult> ChangeAsChild(string tabId, int parentId, int id, int? userId, int? groupId)
        {
            return await base.ChangeAsChild(tabId, parentId, id, userId, groupId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.ChangeChildContentPermission)]
        [BackendActionContext(ActionCode.ChangeChildContentPermission)]
        [BackendActionLog]
        [Record(ActionCode.ChangeChildContentPermission)]
        public override async Task<ActionResult> ChangeAsChild(string tabId, int parentId, IFormCollection collection)
        {
            return await base.ChangeAsChild(tabId, parentId, collection);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveChildContentPermissions)]
        [BackendActionContext(ActionCode.MultipleRemoveChildContentPermissions)]
        [BackendActionLog]
        [Record]
        public override ActionResult MultipleRemoveAsChild(int parentId, [FromBody] SelectedItemsViewModel model, int? userId, int? groupId)
        {
            var result = base.MultipleRemoveAsChild(parentId, model, userId, groupId);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveAllChildContentPermissions)]
        [BackendActionContext(ActionCode.RemoveAllChildContentPermissions)]
        [BackendActionLog]
        [Record]
        public override ActionResult AllRemoveAsChild(int parentId, int? userId, int? groupId)
        {
            var result = base.AllRemoveAsChild(parentId, userId, groupId);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveChildContentPermission)]
        [BackendActionContext(ActionCode.RemoveChildContentPermission)]
        [BackendActionLog]
        [Record]
        public override ActionResult RemoveAsChild(int parentId, int id, int? userId, int? groupId)
        {
            var result = base.RemoveAsChild(parentId, id, userId, groupId);
            PersistUserAndGroupIds(userId, groupId);
            return result;
        }

        protected override string SaveChildPermissionAction => ActionCode.SaveChildContentPermission;

        protected override string MultipleChangeAction => ActionCode.MultipleChangeChildContentPermissions;

        protected override string AllChangeAction => ActionCode.ChangeAllChildContentPermissions;

        protected override string ChangeAction => ActionCode.ChangeChildContentPermission;

        protected override string ControllerName => "ContentPermission";
    }
}

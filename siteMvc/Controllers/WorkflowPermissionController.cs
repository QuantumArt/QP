using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Controllers.Base;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class WorkflowPermissionController : PermissionControllerBase
    {
        public WorkflowPermissionController(IPermissionService service)
            : base(service)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.WorkflowPermissions)]
        [BackendActionContext(ActionCode.WorkflowPermissions)]
        public override async Task<ActionResult> Index(string tabId, int parentId)
        {
            return await base.Index(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.WorkflowPermissions)]
        [BackendActionContext(ActionCode.WorkflowPermissions)]
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
        [ActionAuthorize(ActionCode.AddNewWorkflowPermission)]
        [BackendActionContext(ActionCode.AddNewWorkflowPermission)]
        public override async Task<ActionResult> New(string tabId, int parentId)
        {
            return await base.New(tabId, parentId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewWorkflowPermission)]
        [BackendActionContext(ActionCode.AddNewWorkflowPermission)]
        [BackendActionLog]
        [Record]
        public override async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            return await base.New(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.WorkflowPermissionProperties)]
        [BackendActionContext(ActionCode.WorkflowPermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return await base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateWorkflowPermission)]
        [BackendActionContext(ActionCode.UpdateWorkflowPermission)]
        [BackendActionLog]
        [Record(ActionCode.WorkflowPermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            return await base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveWorkflowPermission)]
        [BackendActionContext(ActionCode.MultipleRemoveWorkflowPermission)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public override ActionResult MultipleRemove(int parentId, int[] IDs)
        {
            return base.MultipleRemove(parentId, IDs);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveWorkflowPermission)]
        [BackendActionContext(ActionCode.RemoveWorkflowPermission)]
        [BackendActionLog]
        public override ActionResult Remove(int parentId, int id)
        {
            return base.Remove(parentId, id);
        }

        protected override string ControllerName => "WorkflowPermission";
    }
}

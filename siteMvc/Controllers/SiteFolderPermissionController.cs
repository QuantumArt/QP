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
    public class SiteFolderPermissionController : PermissionControllerBase
    {
        public SiteFolderPermissionController(SiteFolderPermissionService service)
            : base(service)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderPermissions)]
        [BackendActionContext(ActionCode.SiteFolderPermissions)]
        public override async Task<ActionResult> Index(string tabId, int parentId)
        {
            return await base.Index(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.SiteFolderPermissions)]
        [BackendActionContext(ActionCode.SiteFolderPermissions)]
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
        [ActionAuthorize(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionContext(ActionCode.AddNewSiteFolderPermission)]
        public override async Task<ActionResult> New(string tabId, int parentId)
        {
            return await base.New(tabId, parentId);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionContext(ActionCode.AddNewSiteFolderPermission)]
        [BackendActionLog]
        [Record]
        public override async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            return await base.New(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SiteFolderPermissionProperties)]
        [BackendActionContext(ActionCode.SiteFolderPermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return await base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateSiteFolderPermission)]
        [BackendActionContext(ActionCode.UpdateSiteFolderPermission)]
        [BackendActionLog]
        [Record(ActionCode.SiteFolderPermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            return await base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveSiteFolderPermission)]
        [BackendActionContext(ActionCode.MultipleRemoveSiteFolderPermission)]
        [BackendActionLog]
        public override ActionResult MultipleRemove(int parentId, [FromBody] SelectedItemsViewModel model) => base.MultipleRemove(parentId, model);

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

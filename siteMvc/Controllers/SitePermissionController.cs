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
    public class SitePermissionController : PermissionControllerBase
    {
        public SitePermissionController(SitePermissionService service)
            : base(service)
        {
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SitePermissions)]
        [BackendActionContext(ActionCode.SitePermissions)]
        public override async Task<ActionResult> Index(string tabId, int parentId)
        {
            return await base.Index(tabId, parentId);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.SitePermissions)]
        [BackendActionContext(ActionCode.SitePermissions)]
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
        [ActionAuthorize(ActionCode.AddNewSitePermission)]
        [BackendActionContext(ActionCode.AddNewSitePermission)]
        public override async Task<ActionResult> New(string tabId, int parentId)
        {
            return await base.New(tabId, parentId);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewSitePermission)]
        [BackendActionContext(ActionCode.AddNewSitePermission)]
        [BackendActionLog]
        public override async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            return await base.New(tabId, parentId, collection);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SitePermissionProperties)]
        [BackendActionContext(ActionCode.SitePermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            return await base.Properties(tabId, parentId, id, successfulActionCode);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateSitePermission)]
        [BackendActionContext(ActionCode.UpdateSitePermission)]
        [BackendActionLog]
        [Record(ActionCode.SitePermissionProperties)]
        public override async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            return await base.Properties(tabId, parentId, id, collection);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveSitePermission)]
        [BackendActionContext(ActionCode.MultipleRemoveSitePermission)]
        [BackendActionLog]
        public override ActionResult MultipleRemove(int parentId, [FromBody] SelectedItemsViewModel model)
        {
            return base.MultipleRemove(parentId, model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveSitePermission)]
        [BackendActionContext(ActionCode.RemoveSitePermission)]
        [BackendActionLog]
        public override ActionResult Remove(int parentId, int id)
        {
            return base.Remove(parentId, id);
        }

        protected override string ControllerName => "SitePermission";
    }
}

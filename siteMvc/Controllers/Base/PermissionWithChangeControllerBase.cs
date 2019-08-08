using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;

namespace Quantumart.QP8.WebMvc.Controllers.Base
{
    public abstract class PermissionWithChangeControllerBase : PermissionControllerBase
    {
        protected readonly IActionPermissionChangeService ChangeService;

        protected PermissionWithChangeControllerBase(IPermissionService service, IActionPermissionChangeService changeService)
            : base(service)
        {
            ChangeService = changeService;
        }

        public virtual async Task<ActionResult> Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack)
        {
            var permission = ChangeService.ReadOrDefault(parentId, userId, groupId);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service, ChangeService.ViewModelSettings, isPostBack);
            return await JsonHtml("ActionPermissionChange", model);
        }

        public virtual async Task<ActionResult> Change(string tabId, int parentId, int? userId, int? groupId, FormCollection collection)
        {
            var permission = ChangeService.ReadOrDefaultForChange(parentId, userId, groupId);
            var model = PermissionViewModel.Create(permission, tabId, parentId, Service, ChangeService.ViewModelSettings);

            await TryUpdateModelAsync(model);

            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                model.Data = ChangeService.Change(model.Data);
                return Redirect("Change", new
                {
                    tabId,
                    parentId,
                    userId = model.Data.UserId,
                    groupId = model.Data.GroupId,
                    isPostBack = true,
                    successfulActionCode = ActionCode.ChangeEntityTypePermission
                });
            }

            model.IsPostBack = true;

            return await JsonHtml("ActionPermissionChange", model);
        }

        public virtual ActionResult RemoveForNode(int parentId, int? userId, int? groupId) => JsonMessageResult(ChangeService.Remove(parentId, userId, groupId));
    }
}

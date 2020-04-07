using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.ActionPermissions;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ActionPermissionTreeController : AuthQpController
    {
        private readonly IActionPermissionTreeService _service;

        public ActionPermissionTreeController(IActionPermissionTreeService service)
        {
            _service = service;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionPermissionTree)]
        [BackendActionContext(ActionCode.ActionPermissionTree)]
        public async Task<ActionResult> TreeView(string tabId)
        {
            var model = ActionPermissionsTreeViewModel.Create(tabId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ActionPermissionTree)]
        [BackendActionContext(ActionCode.ActionPermissionTree)]
        public ActionResult GetTreeNodes([FromBody] PermissionTreeQuery model)
        {
            return Json(_service.GetTreeNodes(model));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ActionPermissionTree)]
        [BackendActionContext(ActionCode.ActionPermissionTree)]
        public ActionResult GetTreeNode([FromBody] PermissionTreeQuery model)
        {
            return Json(_service.GetTreeNode(model));
        }
    }
}

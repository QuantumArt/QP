using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.ActionPermissions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.ActionPermissions;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ActionPermissionTreeController : QPController
    {
        private readonly IActionPermissionTreeService _service;

        public ActionPermissionTreeController(IActionPermissionTreeService service)
        {
            _service = service;
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ActionPermissionTree)]
        [BackendActionContext(ActionCode.ActionPermissionTree)]
        public ActionResult TreeView(string tabId)
        {
            var model = ActionPermissionsTreeViewModel.Create(tabId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ActionPermissionTree)]
        [BackendActionContext(ActionCode.ActionPermissionTree)]
        public ActionResult GetTreeNodes(int? entityTypeId, int? userId, int? groupId)
        {
            return Json(_service.GetTreeNodes(entityTypeId, userId, groupId));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ActionPermissionTree)]
        [BackendActionContext(ActionCode.ActionPermissionTree)]
        public ActionResult GetTreeNode(int? entityTypeId, int? actionId, int? userId, int? groupId)
        {
            return Json(_service.GetTreeNode(entityTypeId, actionId, userId, groupId));
        }
    }
}

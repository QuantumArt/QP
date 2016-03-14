using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.ActionPermissions;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Quantumart.QP8.BLL.Services.ActionPermissions;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ActionPermissionTreeController : QPController
    {
		private readonly IActionPermissionTreeService service;
		public ActionPermissionTreeController(IActionPermissionTreeService service)
		{
			this.service = service;
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.ActionPermissionTree)]
		[BackendActionContext(ActionCode.ActionPermissionTree)]
		public ActionResult TreeView(string tabId)
        {
			ActionPermissionsTreeViewModel model = ActionPermissionsTreeViewModel.Create(tabId);
			return this.JsonHtml("Index", model);
        }

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.ActionPermissionTree)]
		[BackendActionContext(ActionCode.ActionPermissionTree)]
		public ActionResult GetTreeNodes(int? entityTypeId, int? userId, int? groupId)
		{
			return Json(service.GetTreeNodes(entityTypeId, userId, groupId));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ActionAuthorize(ActionCode.ActionPermissionTree)]
		[BackendActionContext(ActionCode.ActionPermissionTree)]
		public ActionResult GetTreeNode(int? entityTypeId, int? actionId, int? userId, int? groupId)
		{
			return Json(service.GetTreeNode(entityTypeId, actionId, userId, groupId));
		}
    }
}

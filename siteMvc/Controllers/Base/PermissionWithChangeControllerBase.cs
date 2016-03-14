using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.BLL.Services.ActionPermissions;

namespace Quantumart.QP8.WebMvc.Controllers.Base
{
	public abstract class PermissionWithChangeControllerBase : PermissionControllerBase
	{
		protected readonly IActionPermissionChangeService changeService;
		
		public PermissionWithChangeControllerBase(IPermissionService service, IActionPermissionChangeService changeService) : base(service) 
		{
			this.changeService = changeService;
		}

		#region Change				
		public virtual ActionResult Change(string tabId, int parentId, int? userId, int? groupId, bool? isPostBack)
		{
			EntityPermission permission = changeService.ReadOrDefault(parentId, userId, groupId);
			PermissionViewModel model = PermissionViewModel.Create(permission, tabId, parentId, service, changeService.ViewModelSettings, isPostBack);
			return this.JsonHtml("ActionPermissionChange", model);
		}

		
		public virtual ActionResult Change(string tabId, int parentId, int? userId, int? groupId, FormCollection collection)
		{
			EntityPermission permission = changeService.ReadOrDefaultForChange(parentId, userId, groupId);
			PermissionViewModel model = PermissionViewModel.Create(permission, tabId, parentId, service, changeService.ViewModelSettings);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = changeService.Change(model.Data);
				return Redirect("Change", new
				{
					tabId = tabId,
					parentId = parentId,
					userId = model.Data.UserId,
					groupId = model.Data.GroupId,
					isPostBack = true,
					successfulActionCode = Constants.ActionCode.ChangeEntityTypePermission
				});
			}
			else
			{
				model.IsPostBack = true;
				return JsonHtml("ActionPermissionChange", model);
			}
		}

		public virtual ActionResult RemoveForNode(int parentId, int? userId, int? groupId)
		{
			return JsonMessageResult(changeService.Remove(parentId, userId, groupId));
		}
		#endregion
	}
}
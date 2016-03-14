using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.Constants;
using Telerik.Web.Mvc;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Exceptions;

namespace Quantumart.QP8.WebMvc.Controllers.Base
{
	public abstract class PermissionControllerBase : QPController
	{
		protected readonly IPermissionService service;
		public PermissionControllerBase(IPermissionService service)
		{
			this.service = service;
		}

		protected abstract string ControllerName { get; }

		public virtual ActionResult Index(string tabId, int parentId)
		{
			PermissionInitListResult result = service.InitList(parentId);
			PermissionListViewModel model = PermissionListViewModel.Create(result, tabId, parentId, service, ControllerName);
			return this.JsonHtml("EntityPermissionIndex", model);
		}

		public virtual ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<EntityPermissionListItem> serviceResult = service.List(parentId, command.GetListCommand());
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		public virtual ActionResult New(string tabId, int parentId)
		{
			EntityPermission permission = service.New(parentId);
			PermissionViewModel model = PermissionViewModel.Create(permission, tabId, parentId, service);
			return this.JsonHtml("EntityPermissionProperties", model);
		}

		public virtual ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			EntityPermission permission = service.New(parentId);
			PermissionViewModel model = PermissionViewModel.Create(permission, tabId, parentId, service);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = service.Save(model.Data);
					this.PersistResultId(model.Data.Id);
					return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveSitePermission });
				}
				catch (ActionNotAllowedException nae)
				{
					ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
					return JsonHtml("EntityPermissionProperties", model);
				}
			}
			else
				return JsonHtml("EntityPermissionProperties", model);
		}

		public virtual ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			EntityPermission permission = service.Read(id);
			PermissionViewModel model = PermissionViewModel.Create(permission, tabId, parentId, service);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("EntityPermissionProperties", model);
		}

		public virtual ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			EntityPermission permission = service.ReadForUpdate(id);
			PermissionViewModel model = PermissionViewModel.Create(permission, tabId, parentId, service);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = service.Update(model.Data);
					return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateSitePermission });
				}
				catch (ActionNotAllowedException nae)
				{
					ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae.Message);
					return JsonHtml("EntityPermissionProperties", model);
				}
			}
			else
				return JsonHtml("EntityPermissionProperties", model);
		}

		public virtual ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			MessageResult result = service.MultipleRemove(parentId, IDs);
			return JsonMessageResult(result);
		}

		public virtual ActionResult Remove(int parentId, int id)
		{
			MessageResult result = service.Remove(parentId, id);
			return JsonMessageResult(result);
		}

	}
}

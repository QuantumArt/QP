using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Telerik.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class UserGroupController : QPController
	{
		IUserGroupService service;

		public UserGroupController(IUserGroupService service)
		{
			this.service = service;
		}

		#region List
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.UserGroups)]
		[BackendActionContext(ActionCode.UserGroups)]
		public ActionResult Index(string tabId, int parentId)
        {
			UserGroupInitListResult result = service.InitList(parentId);
			UserGroupListViewModel model = UserGroupListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.UserGroups)]
		[BackendActionContext(ActionCode.UserGroups)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<UserGroupListItem> serviceResult = service.List(command.GetListCommand());
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.UserGroups)]
		[BackendActionContext(ActionCode.UserGroups)]
		public ActionResult Tree(string tabId, int parentId)
		{
			UserGroupInitTreeResult result = service.InitTree(parentId);
			UserGroupTreeViewModel model = UserGroupTreeViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Tree", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.SelectUserGroup)]
		[BackendActionContext(ActionCode.SelectUserGroup)]
		public ActionResult Select(string tabId, int parentId, int id)
		{
			UserGroupSelectableListViewModel model = UserGroupSelectableListViewModel.Create(tabId, parentId, new[] { id });
			return this.JsonHtml("SelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SelectUserGroup)]
		[BackendActionContext(ActionCode.SelectUserGroup)]
		public ActionResult _Select(string tabId, int id, GridCommand command)
		{
			ListResult<UserGroupListItem> serviceResult = service.List(command.GetListCommand(), new[] { id });
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewUserGroup)]		
		[BackendActionContext(ActionCode.AddNewUserGroup)]
		public ActionResult New(string tabId, int parentId)
		{
			UserGroup group = service.NewProperties();
			UserGroupViewModel model = UserGroupViewModel.Create(group, tabId, parentId, service);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewUserGroup)]
		[BackendActionContext(ActionCode.AddNewUserGroup)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			UserGroup group = service.NewProperties();
			UserGroupViewModel model = UserGroupViewModel.Create(group, tabId, parentId, service);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = service.SaveProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveUserGroup });
			}
			else
				return JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.CreateLikeUserGroup)]
		[BackendActionContext(ActionCode.CreateLikeUserGroup)]
		[BackendActionLog]
		[Record]
		public ActionResult Copy(int id)
		{
			CopyResult result = service.Copy(id);
			this.PersistResultId(result.Id);
			this.PersistFromId(id);
			return JsonMessageResult(result.Message);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.UserGroupProperties)]
		[BackendActionContext(ActionCode.UserGroupProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			UserGroup group = service.ReadProperties(id);
			ViewData[SpecialKeys.IsEntityReadOnly] = group.IsReadOnly;
			UserGroupViewModel model = UserGroupViewModel.Create(group, tabId, parentId, service);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateUserGroup)]
		[BackendActionContext(ActionCode.UpdateUserGroup)]
		[BackendActionLog]
		[Record(ActionCode.UserGroupProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			UserGroup group = service.ReadProperties(id);
			UserGroupViewModel model = UserGroupViewModel.Create(group, tabId, parentId, service);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = service.UpdateProperties(model.Data);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateUserGroup });
			}
			else
				return JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveUserGroup)]
		[BackendActionContext(ActionCode.RemoveUserGroup)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = service.Remove(id);
			return JsonMessageResult(result);
		}

		public ActionResult RemovePreAction(int id)
		{
			return Json(service.RemovePreAction(id));
		}

	}
}

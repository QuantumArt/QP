using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;
using Telerik.Web.Mvc;
using AutoMapper;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.Resources;


namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VirtualContentController : QPController
	{

		/// <summary>
		/// Возвращает список дочерних полей
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <param name="parentEntityId">идентификатор родительской сущности</param>
		/// <param name="entityId">идентификатор сущности</param>
		/// <param name="returnSelf">возвращает саму сущность по entityId (необходимо в случае генерации корня поддерева)</param>
		/// <returns>список дочерних сущностей</returns>
		[HttpPost]
		public JsonResult GetChildFieldList(int virtualContentId, int? joinedContentId, string entityId, string selectItemIDs, string parentAlias)
		{
			var entityList = VirtualContentService.GetChildFieldList(virtualContentId, joinedContentId, entityId, selectItemIDs, parentAlias);
			return Json(entityList, JsonRequestBehavior.AllowGet);
		}
		
		#region form actions
		
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewVirtualContents)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewVirtualContents)]
		public ActionResult New(string tabId, int parentId, int? groupId)
		{
			Content content = VirtualContentService.New(parentId, groupId);
			VirtualContentViewModel model = VirtualContentViewModel.Create(content, tabId, parentId);
			return this.JsonHtml("Properties", model);
		}


		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewVirtualContents)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewVirtualContents)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			Content content = VirtualContentService.NewForSave(parentId);
			VirtualContentViewModel model = VirtualContentViewModel.Create(content, tabId, parentId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = VirtualContentService.Save(model.Data);
					this.PersistResultId(model.Data.Id);
					this.PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
					return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveVirtualContent });
				}
				catch (UserQueryContentCreateViewException uqe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
					return JsonHtml("Properties", model);
				}
				catch (VirtualContentProcessingException vcpe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
					return JsonHtml("Properties", model);
				}
				catch (CycleInContentGraphException)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("CycleInContentGraphException", ContentStrings.CyclesInContentTree);
					return JsonHtml("Properties", model);
				}
			}
			else
				return (ActionResult)this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.VirtualContentProperties)]
		[BackendActionContext(ActionCode.VirtualContentProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? groupChanged)
		{
			Content content = VirtualContentService.Read(id);
			VirtualContentViewModel model = VirtualContentViewModel.Create(content, tabId, parentId);
			model.GroupChanged = groupChanged.HasValue ? groupChanged.Value : false;
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateVirtualContent)]
		[BackendActionContext(ActionCode.UpdateVirtualContent)]
		[BackendActionLog]
		[Record(ActionCode.VirtualContentProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, FormCollection collection)
		{
			Content content = VirtualContentService.ReadForUpdate(id);
			VirtualContentViewModel model = VirtualContentViewModel.Create(content, tabId, parentId);
			int oldGroupId = model.Data.GroupId;
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = VirtualContentService.Update(model.Data);
					this.PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
				}
				catch (UserQueryContentCreateViewException uqe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);					
					return JsonHtml("Properties", model);
				}
				catch (VirtualContentProcessingException vcpe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
					return JsonHtml("Properties", model);
				}
				catch (CycleInContentGraphException)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("CycleInContentGraphException", ContentStrings.CyclesInContentTree);					
					return JsonHtml("Properties", model);
				}				
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = backendActionCode, groupChanged = oldGroupId != model.Data.GroupId });
			}
			else
				return JsonHtml("Properties", model);
		}


		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveVirtualContent)]
		[BackendActionContext(ActionCode.RemoveVirtualContent)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = VirtualContentService.Remove(id);
			return JsonMessageResult(result);
		}
		#endregion

	}
}
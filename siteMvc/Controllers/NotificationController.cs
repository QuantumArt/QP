using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Notification;
using Telerik.Web.Mvc;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Exceptions;

namespace Quantumart.QP8.WebMvc.Controllers
{
	[ValidateInput(false)]
	public class NotificationController : QPController
	{
        INotificationService _notificationService;

		public NotificationController(INotificationService notificationService)
		{
			this._notificationService = notificationService;
		}

		#region list actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.Notifications)]
		[BackendActionContext(ActionCode.Notifications)]
		public ActionResult Index(string tabId, int parentId)
		{
			NotificationInitListResult result = _notificationService.InitList(parentId);
			NotificationListViewModel model = NotificationListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.Notifications)]
		[BackendActionContext(ActionCode.Notifications)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<NotificationListItem> serviceResult = _notificationService.GetNotificationsByContentId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.NotificationObjectFormatProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "id")]
		[BackendActionContext(ActionCode.NotificationObjectFormatProperties)]
		public ActionResult NotificationTemplateFormatProperties(string tabId, int parentId, int id, string successfulActionCode)
		{
			NotificationObjectFormat format = _notificationService.ReadNotificationTemplateFormat(id);
			var template = _notificationService.ReadPageTemplateByObjectFormatId(id);
			NotificationTemplateFormatViewModel model = NotificationTemplateFormatViewModel.Create(format, tabId, parentId, template.Id, template.SiteId);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("NotificationTemplateFormatProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.NotificationObjectFormatProperties)]
		[BackendActionContext(ActionCode.NotificationObjectFormatProperties)]
		[BackendActionLog]
		[Record(ActionCode.NotificationObjectFormatProperties)]
		public ActionResult NotificationTemplateFormatProperties(string tabId, int parentId, int id, FormCollection collection)
		{
			NotificationObjectFormat format = _notificationService.ReadNotificationTemplateFormatForUpdate(id);
			var template = _notificationService.ReadPageTemplateByObjectFormatId(id);
			NotificationTemplateFormatViewModel model = NotificationTemplateFormatViewModel.Create(format, tabId, parentId, template.Id, template.SiteId);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _notificationService.UpdateNotificationTemplateFormat(model.Data);
				return Redirect("NotificationTemplateFormatProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateContentGroup });
			}
			else
				return JsonHtml("NotificationTemplateFormatProperties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.AddNewNotification)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
		[BackendActionContext(ActionCode.AddNewNotification)]
		public ActionResult New(string tabId, int parentId)
		{
			Notification notification = _notificationService.NewNotificationProperties(parentId);
            NotificationViewModel model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.AddNewNotification)]
		[BackendActionContext(ActionCode.AddNewNotification)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			Notification notification = _notificationService.NewNotificationPropertiesForUpdate(parentId);
            NotificationViewModel model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
                    model.Data = _notificationService.SaveNotificationProperties(model.Data, model.CreateDefaultFormat, GetBackendUrl());
					this.PersistResultId(model.Data.Id);
					if (model.CreateDefaultFormat)
						this.PersistNotificationFormatId(model.Data.FormatId);
					return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveNotification });
				}
				catch (ActionNotAllowedException nae)
				{
					ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae);
					return JsonHtml("Properties", model);
				}
			}
			else
				return JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.NotificationProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Notification, "id")]
		[BackendActionContext(ActionCode.NotificationProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			Notification notification = _notificationService.ReadNotificationProperties(id);
			ViewData[SpecialKeys.IsEntityReadOnly] = notification.WorkFlowId.HasValue;
            NotificationViewModel model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.UpdateNotification)]
		[BackendActionContext(ActionCode.UpdateNotification)]
		[BackendActionLog]
		[Record(ActionCode.NotificationProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			Notification notification = _notificationService.ReadNotificationPropertiesForUpdate(id);
            NotificationViewModel model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _notificationService.UpdateNotificationProperties(model.Data, model.CreateDefaultFormat, GetBackendUrl());
				if (model.CreateDefaultFormat)
					this.PersistNotificationFormatId(model.Data.FormatId);
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateNotification });
			}
			else
				return JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.UnbindNotification)]
		[BackendActionContext(ActionCode.UnbindNotification)]
		[BackendActionLog]
		[Record]
		public ActionResult Unbind(int Id)
		{
			MessageResult result = _notificationService.UnbindNotification(Id);
			return JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.RemoveNotification)]
		[BackendActionContext(ActionCode.RemoveNotification)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = _notificationService.Remove(id);
			return JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.MultipleRemoveNotification)]
		[BackendActionContext(ActionCode.MultipleRemoveNotification)]
		[BackendActionLog]
		[Record]
		public ActionResult MultipleRemove(int parentId, int[] IDs)
		{
			return JsonMessageResult(_notificationService.MultipleRemove(IDs));
		}

		[HttpPost]
		public ActionResult AssembleNotificationPreAction(int id)
		{
			return Json(_notificationService.AssembleNotificationPreAction(id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.AssembleNotification)]
		[BackendActionContext(ActionCode.AssembleNotification)]
		[BackendActionLog]
		public ActionResult AssembleNotification(int id)
		{
			return Json(_notificationService.AssembleNotification(id));
		}

		[HttpPost]
		public ActionResult MultipleAssembleNotificationPreAction(int[] IDs)
		{
			return Json(_notificationService.MultipleAssembleNotificationPreAction(IDs));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.MultipleAssembleNotification)]
		[BackendActionContext(ActionCode.MultipleAssembleNotification)]
		[BackendActionLog]
		public ActionResult MultipleAssembleNotification(int[] IDs)
		{
			return Json(_notificationService.MultipleAssembleNotification(IDs));
		}

	}
}

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Notification;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class NotificationController : QPController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Notifications)]
        [BackendActionContext(ActionCode.Notifications)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = _notificationService.InitList(parentId);
            var model = NotificationListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Notifications)]
        [BackendActionContext(ActionCode.Notifications)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = _notificationService.GetNotificationsByContentId(command.GetListCommand(), parentId);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.NotificationObjectFormatProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObjectFormat, "id")]
        [BackendActionContext(ActionCode.NotificationObjectFormatProperties)]
        public ActionResult NotificationTemplateFormatProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var format = _notificationService.ReadNotificationTemplateFormat(id);
            var template = _notificationService.ReadPageTemplateByObjectFormatId(id);
            var model = NotificationTemplateFormatViewModel.Create(format, tabId, parentId, template.Id, template.SiteId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("NotificationTemplateFormatProperties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.NotificationObjectFormatProperties)]
        [BackendActionContext(ActionCode.NotificationObjectFormatProperties)]
        [BackendActionLog]
        [Record(ActionCode.NotificationObjectFormatProperties)]
        public ActionResult NotificationTemplateFormatProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            var format = _notificationService.ReadNotificationTemplateFormatForUpdate(id);
            var template = _notificationService.ReadPageTemplateByObjectFormatId(id);
            var model = NotificationTemplateFormatViewModel.Create(format, tabId, parentId, template.Id, template.SiteId);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _notificationService.UpdateNotificationTemplateFormat(model.Data);
                return Redirect("NotificationTemplateFormatProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateContentGroup });
            }

            return JsonHtml("NotificationTemplateFormatProperties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewNotification)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
        [BackendActionContext(ActionCode.AddNewNotification)]
        public ActionResult New(string tabId, int parentId)
        {
            var notification = _notificationService.NewNotificationProperties(parentId);
            var model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewNotification)]
        [BackendActionContext(ActionCode.AddNewNotification)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var notification = _notificationService.NewNotificationPropertiesForUpdate(parentId);
            var model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                try
                {
                    model.Data = _notificationService.SaveNotificationProperties(model.Data, model.CreateDefaultFormat, CommonHelpers.GetBackendUrl(HttpContext));
                    PersistResultId(model.Data.Id);
                    if (model.CreateDefaultFormat)
                    {
                        PersistNotificationFormatId(model.Data.FormatId);
                    }

                    return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveNotification });
                }
                catch (ActionNotAllowedException nae)
                {
                    ModelState.AddModelError("OperationIsNotAllowedForAggregated", nae);
                    return JsonHtml("Properties", model);
                }
            }

            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.NotificationProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Notification, "id")]
        [BackendActionContext(ActionCode.NotificationProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var notification = _notificationService.ReadNotificationProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = notification.WorkFlowId.HasValue;
            var model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateNotification)]
        [BackendActionContext(ActionCode.UpdateNotification)]
        [BackendActionLog]
        [Record(ActionCode.NotificationProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var notification = _notificationService.ReadNotificationPropertiesForUpdate(id);
            var model = NotificationViewModel.Create(notification, tabId, parentId, _notificationService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _notificationService.UpdateNotificationProperties(model.Data, model.CreateDefaultFormat, CommonHelpers.GetBackendUrl(HttpContext));
                if (model.CreateDefaultFormat)
                {
                    PersistNotificationFormatId(model.Data.FormatId);
                }

                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateNotification });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UnbindNotification)]
        [BackendActionContext(ActionCode.UnbindNotification)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult Unbind(int Id)
        {
            var result = _notificationService.UnbindNotification(Id);
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveNotification)]
        [BackendActionContext(ActionCode.RemoveNotification)]
        [BackendActionLog]
        public ActionResult Remove(int id)
        {
            var result = _notificationService.Remove(id);
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveNotification)]
        [BackendActionContext(ActionCode.MultipleRemoveNotification)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemove(int parentId, int[] IDs) => JsonMessageResult(_notificationService.MultipleRemove(IDs));

        [HttpPost]
        public ActionResult AssembleNotificationPreAction(int id) => Json(_notificationService.AssembleNotificationPreAction(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssembleNotification)]
        [BackendActionContext(ActionCode.AssembleNotification)]
        [BackendActionLog]
        public ActionResult AssembleNotification(int id) => Json(_notificationService.AssembleNotification(id));

        [HttpPost]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssembleNotificationPreAction(int[] IDs) => Json(_notificationService.MultipleAssembleNotificationPreAction(IDs));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.MultipleAssembleNotification)]
        [BackendActionContext(ActionCode.MultipleAssembleNotification)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssembleNotification(int[] IDs) => Json(_notificationService.MultipleAssembleNotification(IDs));
    }
}

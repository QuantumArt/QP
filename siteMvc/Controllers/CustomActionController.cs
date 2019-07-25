using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Web.Mvc;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Web.AspNet.ActionResults;
using QP8.Infrastructure.Web.AspNet.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class CustomActionController : QPController
    {
        private readonly ICustomActionService _service;

        public CustomActionController(ICustomActionService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult Execute(string tabId, int parentId, int[] ids, string actionCode)
        {
            CustomActionPrepareResult customActionToExecute = null;
            try
            {
                customActionToExecute = _service.PrepareForExecuting(actionCode, tabId, ids, parentId);
                Logger.Log.Debug($"Executing custom action url: {customActionToExecute.CustomAction.FullUrl}");

                if (!customActionToExecute.IsActionAccessable)
                {
                    throw new SecurityException(customActionToExecute.SecurityErrorMesage);
                }

                if (customActionToExecute.CustomAction.Action.IsInterface)
                {
                    var model = ExecuteCustomActionViewModel.Create(tabId, parentId, ids, customActionToExecute.CustomAction);
                    return JsonHtml("ExecuteAction", model);
                }

                return Json(new { Url = customActionToExecute.CustomAction.FullUrl, PreActionUrl = customActionToExecute.CustomAction.PreActionFullUrl });
            }
            catch (Exception ex)
            {
                if (customActionToExecute?.CustomAction?.Action == null)
                {
                    throw;
                }

                if (customActionToExecute.CustomAction.Action.IsInterface)
                {
                    return new JsonNetResult<object>(new { success = false, message = ex.Message });
                }

                return new JsonResult { Data = MessageResult.Error(ex.Message), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public ActionResult ExecutePreAction(string tabId, int parentId, int[] ids, string actionCode) => Json(MessageResult.Confirm($"Action: {actionCode}, ParentId: {parentId}, IDs: {string.Join(";", ids)}"));

        [HttpPost]
        public ActionResult Proxy(string url, string actionCode, int level, int[] ids, int? parentEntityId)
        {
            var urlToProcess = UrlHelpers.ConvertToAbsoluteUrl(url);

            Logger.Log.Debug($"Proxy custom action url: {urlToProcess.ToJsonLog()}");
            var parts = urlToProcess.Split("?".ToCharArray(), 2);
            var request = WebRequest.Create(parts[0]);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var postBytes = new ASCIIEncoding().GetBytes(parts[1]);
            request.ContentLength = postBytes.Length;

            using (var postStream = request.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
            }

            try
            {
                var result = string.Empty;
                var response = request.GetResponse().GetResponseStream();
                if (response != null)
                {
                    result = new StreamReader(response).ReadToEnd();
                }

                if (level >= PermissionLevel.Modify)
                {
                    CreateLogs(actionCode, ids, parentEntityId);
                }

                return Content(result);
            }
            catch (Exception ex)
            {
                return JsonMessageResult(MessageResult.Error(ex.Message));
            }
        }

        private static void CreateLogs(string actionCode, int[] ids, int? parentEntityId)
        {
            var backendLog = DependencyResolver.Current.GetService<IBackendActionLogRepository>();
            BackendActionContext.SetCurrent(actionCode, ids.Select(n => n.ToString()), parentEntityId);

            var logs = BackendActionLog.CreateLogs(BackendActionContext.Current, backendLog);
            backendLog.Save(logs);
            BackendActionContext.ResetCurrent();
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CustomActions)]
        [BackendActionContext(ActionCode.CustomActions)]
        public ActionResult Index(string tabId, int parentId)
        {
            var initList = _service.InitList(parentId);
            var model = CustomActionListViewModel.Create(initList, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.CustomActions)]
        [BackendActionContext(ActionCode.CustomActions)]
        public ActionResult _Index(
            string tabId,
            int parentId,
            int page,
            int pageSize,
            int gridParentId,
            string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _service.List(listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewCustomAction)]
        [BackendActionContext(ActionCode.AddNewCustomAction)]
        public ActionResult New(string tabId, int parentId)
        {
            var action = _service.New();
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewCustomAction)]
        [BackendActionContext(ActionCode.AddNewCustomAction)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, int id, FormCollection collection)
        {
            var action = _service.NewForSave();
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _service.Save(model.Data, model.SelectedActionsIds);
                PersistResultId(model.Data.Id);
                PersistActionId(model.Data.ActionId);
                PersistActionCode(model.Data.Action.Code);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveCustomAction });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CustomActionsProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.CustomAction, "id")]
        [BackendActionContext(ActionCode.CustomActionsProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var action = _service.Read(id);
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.CustomActionsProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateCustomAction)]
        [BackendActionContext(ActionCode.UpdateCustomAction)]
        [BackendActionLog]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var action = _service.ReadForUpdate(id);
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _service.Update(model.Data, model.SelectedActionsIds);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateCustomAction });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveCustomAction)]
        [BackendActionContext(ActionCode.RemoveCustomAction)]
        [BackendActionLog]
        public ActionResult Remove(int id) => JsonMessageResult(_service.Remove(id));


        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikeCustomAction)]
        [BackendActionContext(ActionCode.CreateLikeCustomAction)]
        [BackendActionLog]
        public ActionResult Copy(string tabId, int parentId, int id, FormCollection collection)
        {
            var action = _service.ReadForUpdate(id);
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            var result = _service.Copy(id, model.SelectedActionsIds);
            return JsonMessageResult(result.Message);
        }
    }
}

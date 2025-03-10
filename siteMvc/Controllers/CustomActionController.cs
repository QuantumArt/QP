using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NLog;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Web.Helpers;
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
using ILogger = NLog.ILogger;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class CustomActionController : AuthQpController
    {
        private readonly ICustomActionService _service;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBackendActionLogRepository _logRepository;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public CustomActionController(
            ICustomActionService service,
            IServiceProvider serviceProvider,
            IBackendActionLogRepository logRepository
        )
        {
            _service = service;
            _serviceProvider = serviceProvider;
            _logRepository = logRepository;
        }

        [HttpPost]
        public async Task<ActionResult> Execute(string tabId, int parentId, [FromBody] CustomActionQuery query)
        {
            CustomActionPrepareResult customActionToExecute = null;
            try
            {
                customActionToExecute = _service.PrepareForExecuting(tabId, parentId, query);
                Logger.ForDebugEvent()
                    .Message("Executing custom action url: {url}", customActionToExecute.CustomAction.FullUrl)
                    .Log();

                if (!customActionToExecute.IsActionAccessible)
                {
                    var message = customActionToExecute.SecurityErrorMessage;
                    var clientMessage = customActionToExecute.ClientSecurityErrorMessage;
                    throw new SecurityException(message) { Data = { { ExceptionHelpers.ClientMessageKey, clientMessage } } };
                }

                if (customActionToExecute.CustomAction.Action.IsInterface)
                {
                    var model = ExecuteCustomActionViewModel.Create(tabId, parentId, query.Ids, customActionToExecute.CustomAction);
                    return await JsonHtml("ExecuteAction", model);
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
                    return Json(new { success = false, message = ex.Message });
                }

                return Json(MessageResult.Error(ex.Message));
            }
        }

        [HttpPost]
        public ActionResult ExecutePreAction(string tabId, int parentId, [FromBody] CustomActionQuery query)
        {
            var msg = $"Action: {query.ActionCode}, ParentId: {parentId}, IDs: {string.Join(";", query.Ids)}";
            return Json(MessageResult.Confirm(msg));
        }

        [HttpPost]
        public ActionResult Proxy([FromBody] ProxyViewModel model)
        {
            var urlToProcess = UrlHelpers.ConvertToAbsoluteUrl(model.Url);

            Logger.ForDebugEvent()
                .Message("Proxying custom action url: {url}", urlToProcess)
                .Log();

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

                if (model.Level >= PermissionLevel.Modify)
                {
                    BackendActionContext.CreateLogs(model.ActionCode, model.Ids, model.ParentEntityId, _logRepository);
                }

                return Content(result);
            }
            catch (Exception ex)
            {
                return JsonMessageResult(MessageResult.Error(ex.Message));
            }
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CustomActions)]
        [BackendActionContext(ActionCode.CustomActions)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var initList = _service.InitList(parentId);
            var model = CustomActionListViewModel.Create(initList, tabId, parentId);
            return await JsonHtml("Index", model);
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
        public async Task<ActionResult> New(string tabId, int parentId)
        {
            var action = _service.New();
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewCustomAction)]
        [BackendActionContext(ActionCode.AddNewCustomAction)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, int id, IFormCollection collection)
        {
            var action = _service.NewForSave();
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _service.Save(model.Data, model.SelectedActionsIds);
                PersistResultId(model.Data.Id);
                PersistActionId(model.Data.ActionId);
                PersistActionCode(model.Data.Action.Code);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveCustomAction });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CustomActionsProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.CustomAction, "id")]
        [BackendActionContext(ActionCode.CustomActionsProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var action = _service.Read(id);
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.CustomActionsProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateCustomAction)]
        [BackendActionContext(ActionCode.UpdateCustomAction)]
        [BackendActionLog]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var action = _service.ReadForModify(id);
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _service.Update(model.Data, model.SelectedActionsIds);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateCustomAction });
            }

            return await JsonHtml("Properties", model);
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
        public ActionResult Copy(int id, string tabId, int parentId, int? forceId, int? forceActionId, string forceActionCode, IFormCollection collection)
        {
            var action = _service.ReadForModify(id);
            var model = CustomActionViewModel.Create(action, tabId, parentId, _service);
            var result = _service.Copy(id, model.SelectedActionsIds, forceId, forceActionId, forceActionCode);
            PersistResultId(result.Data.Id);
            PersistFromId(id);
            PersistActionId(result.Data.ActionId);
            PersistActionCode(result.Data.Action.Code);
            return JsonMessageResult(result.Message);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Web.Mvc;
using QP8.Infrastructure.Web.ActionResults;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;
using Telerik.Web.Mvc;

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
            CustomActionPrepareResult result = null;
            try
            {
                result = _service.PrepareForExecuting(actionCode, tabId, ids, parentId);
                if (!result.IsActionAccessable)
                {
                    throw new SecurityException(result.SecurityErrorMesage);
                }

                if (result.CustomAction.Action.IsInterface)
                {
                    var model = ExecuteCustomActionViewModel.Create(tabId, parentId, ids, result.CustomAction);
                    return JsonHtml("ExecuteAction", model);
                }

                return Json(new { Url = result.CustomAction.FullUrl, PreActionUrl = result.CustomAction.PreActionFullUrl });
            }
            catch (Exception ex)
            {
                if (result?.CustomAction?.Action == null)
                {
                    throw;
                }

                if (result.CustomAction.Action.IsInterface)
                {
                    return new JsonNetResult<object>(new { success = false, message = ex.Message });
                }

                return new JsonResult { Data = MessageResult.Error(ex.Message), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public ActionResult ExecutePreAction(string tabId, int parentId, int[] ids, string actionCode)
        {
            return Json(MessageResult.Confirm($"Action: {actionCode}, ParentId: {parentId}, IDs: {string.Join(";", ids)}"));
        }

        [HttpPost]
        public ActionResult Proxy(string url, string actionCode, int level, int[] ids, int? parentEntityId)
        {
            var parts = url.Split("?".ToCharArray(), 2);
            var req = WebRequest.Create(parts[0]);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            var ascii = new ASCIIEncoding();
            var postBytes = ascii.GetBytes(parts[1]);
            req.ContentLength = postBytes.Length;

            using (var postStream = req.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
            }

            try
            {
                var result = string.Empty;
                var resp = req.GetResponse().GetResponseStream();
                if (resp != null)
                {
                    result = new StreamReader(resp).ReadToEnd();
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
            var repo = DependencyResolver.Current.GetService<IBackendActionLogRepository>();
            BackendActionContext.SetCurrent(actionCode, ids.Select(n => n.ToString()), parentEntityId);
            var logs = BackendActionLog.CreateLogs(BackendActionContext.Current, repo);

            repo.Save(logs);
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
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.CustomActions)]
        [BackendActionContext(ActionCode.CustomActions)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = _service.List(command.GetListCommand());
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
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
        public ActionResult Remove(int id)
        {
            return JsonMessageResult(_service.Remove(id));
        }
    }
}

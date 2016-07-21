using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.CustomAction;
using System;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class CustomActionController : QPController
    {
        private readonly ICustomActionService service;

        #region Executing
        public CustomActionController(ICustomActionService service)
        {
            this.service = service;
        }

        [HttpPost]
        public ActionResult Execute(string tabId, int parentId, int[] IDs, string actionCode)
        {
            CustomActionPrepareResult result = null;
            try
            {
                result = service.PrepareForExecuting(actionCode, tabId, IDs, parentId);
                if (!result.IsActionAccessable)
                {
                    throw new SecurityException(result.SecurityErrorMesage);
                }

                if (result.CustomAction.Action.IsInterface)
                {
                    ExecuteCustomActionViewModel model = ExecuteCustomActionViewModel.Create(tabId, parentId, IDs, result.CustomAction);
                    return JsonHtml("ExecuteAction", model);
                }
                else
                {
                    return Json(new { Url = result.CustomAction.FullUrl, PreActionUrl = result.CustomAction.PreActionFullUrl });
                }
            }
            catch (Exception exp)
            {
                if (result == null || result.CustomAction == null || result.CustomAction.Action == null)
                {
                    throw;
                }
                else if (result.CustomAction.Action.IsInterface)
                {
                    return new JsonNetResult<object>(new { success = false, message = exp.Message });
                }
                else
                {
                    return new JsonResult { Data = MessageResult.Error(exp.Message), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
        }

        [HttpPost]
        public ActionResult ExecutePreAction(string tabId, int parentId, int[] IDs, string actionCode)
        {
            return Json(MessageResult.Confirm(String.Format("Action: {0}, ParentId: {1}, IDs: {2}", actionCode, parentId, String.Join(";", IDs))));
        }

        [HttpPost]
        public ActionResult Proxy(string url, string callback)
        {
            var parts = url.Split("?".ToCharArray(), 2);
            var req = HttpWebRequest.Create(parts[0]);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(parts[1]);
            req.ContentLength = postBytes.Length;
            using (Stream postStream = req.GetRequestStream())
            {
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
            }
            try
            {
                var resp = req.GetResponse();
                var strmReader = new StreamReader(resp.GetResponseStream());
                return Content(strmReader.ReadToEnd());
            }
            catch (Exception ex)
            {
                return this.JsonMessageResult(MessageResult.Error(ex.Message));
            }
        }

        #endregion

        #region List
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CustomActions)]
        [BackendActionContext(ActionCode.CustomActions)]
        public ActionResult Index(string tabId, int parentId)
        {
            CustomActionInitListResult initList = service.InitList(parentId);
            CustomActionListViewModel model = CustomActionListViewModel.Create(initList, tabId, parentId);
            return this.JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.CustomActions)]
        [BackendActionContext(ActionCode.CustomActions)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            ListResult<CustomActionListItem> serviceResult = service.List(command.GetListCommand());
            return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }
        #endregion

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewCustomAction)]
        [BackendActionContext(ActionCode.AddNewCustomAction)]
        public ActionResult New(string tabId, int parentId)
        {
            CustomAction action = service.New();
            CustomActionViewModel model = CustomActionViewModel.Create(action, tabId, parentId, service);
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.AddNewCustomAction)]
        [BackendActionContext(ActionCode.AddNewCustomAction)]
        [BackendActionLog]
        [Record]
        public ActionResult New(string tabId, int parentId, int id, FormCollection collection)
        {
            CustomAction action = service.NewForSave();
            CustomActionViewModel model = CustomActionViewModel.Create(action, tabId, parentId, service);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = service.Save(model.Data, model.SelectedActionsIds);
                this.PersistResultId(model.Data.Id);
                this.PersistActionId(model.Data.ActionId);
                this.PersistActionCode(model.Data.Action.Code);
                return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveCustomAction });
            }
            else
                return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.CustomActionsProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.CustomAction, "id")]
        [BackendActionContext(ActionCode.CustomActionsProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            CustomAction action = service.Read(id);
            CustomActionViewModel model = CustomActionViewModel.Create(action, tabId, parentId, service);
            model.SuccesfulActionCode = successfulActionCode;
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.UpdateCustomAction)]
        [BackendActionContext(ActionCode.UpdateCustomAction)]
        [BackendActionLog]
        [Record(ActionCode.CustomActionsProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            CustomAction action = service.ReadForUpdate(id);
            CustomActionViewModel model = CustomActionViewModel.Create(action, tabId, parentId, service);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = service.Update(model.Data, model.SelectedActionsIds);
                return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateCustomAction });
            }
            else
                return JsonHtml("Properties", model);
        }


        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.RemoveCustomAction)]
        [BackendActionContext(ActionCode.RemoveCustomAction)]
        [BackendActionLog]
        [Record]
        public ActionResult Remove(int id)
        {
            MessageResult result = service.Remove(id);
            return JsonMessageResult(result);
        }
    }
}

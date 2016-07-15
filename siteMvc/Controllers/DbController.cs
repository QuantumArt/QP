using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Hubs;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class DbController : QPController
    {
        private readonly ICommunicationService _communicationService;

        public DbController(ICommunicationService communicationService)
        {
            _communicationService = communicationService;
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.DbSettings)]
        [BackendActionContext(ActionCode.DbSettings)]
        public ActionResult Settings(string tabId, int parentId, string successfulActionCode)
        {
            var db = DbService.ReadSettings();
            var model = EntityViewModel.Create<DbViewModel>(db, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Settings", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateDbSettings)]
        [BackendActionContext(ActionCode.UpdateDbSettings)]
        [BackendActionLog]
        [ValidateInput(false)]
        public ActionResult Settings(string tabId, int parentId, FormCollection collection)
        {
            var db = DbService.ReadSettingsForUpdate();
            var model = EntityViewModel.Create<DbViewModel>(db, tabId, parentId);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                object message = null;
                var needSendMessage = false;
                if (model.Data.RecordActions)
                {
                    new RecordReplayHelper().Clear(HttpContext, model.OverrideRecordsFile);
                    if (model.OverrideRecordsUser || model.Data.SingleUserId == null)
                    {
                        model.Data.SingleUserId = QPContext.CurrentUserId;
                        needSendMessage = true;
                        message = new
                        {
                            userId = QPContext.CurrentUserId,
                            userName = QPContext.CurrentUserName
                        };
                    }
                }
                else
                {
                    needSendMessage = true;
                    model.Data.SingleUserId = null;
                }

                model.Data = DbService.UpdateSettings(model.Data);
                if (needSendMessage)
                {
                    _communicationService.Send("singleusermode", message);
                }

                return Redirect("Settings", new { successfulActionCode = ActionCode.UpdateDbSettings });
            }

            return JsonHtml("Settings", model);
        }

        public FileResult GetRecordedUserActions()
        {
            var fileBytes = System.IO.File.ReadAllBytes(@"c:\folder\myfile.ext");
            var fileName = "myfile.ext";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}

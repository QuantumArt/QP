using System.Net.Mime;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.XmlDbUpdate;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Hubs;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class DbController : QPController
    {
        private readonly ICommunicationService _communicationService;
        private readonly IXmlDbUpdateLogService _xmlDbUpdateLogService;
        private readonly IApplicationInfoRepository _appInfoRepository;
        private readonly IXmlDbUpdateHttpContextProcessor _httpContextProcessor;
        private readonly IXmlDbUpdateActionCorrecterService _actionsCorrecterService;

        public DbController(
            ICommunicationService communicationService,
            IXmlDbUpdateLogService xmlDbUpdateServce,
            IApplicationInfoRepository appInfoRepository,
            IXmlDbUpdateHttpContextProcessor httpContextProcessor,
            IXmlDbUpdateActionCorrecterService actionsCorrecterService)
        {
            _communicationService = communicationService;
            _xmlDbUpdateLogService = xmlDbUpdateServce;
            _appInfoRepository = appInfoRepository;
            _actionsCorrecterService = actionsCorrecterService;
            _httpContextProcessor = httpContextProcessor;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.DbSettings)]
        [BackendActionContext(ActionCode.DbSettings)]
        public ActionResult Settings(string tabId, int parentId, string successfulActionCode)
        {
            var db = DbService.ReadSettings();
            var model = EntityViewModel.Create<DbViewModel>(db, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;

            ViewBag.IsRecordAvailableForDownload = System.IO.File.Exists(QPContext.GetRecordXmlFilePath());
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
                    if (model.OverrideRecordsFile)
                    {
                        var currentDbVersion = _appInfoRepository.GetCurrentDbVersion();
                        XmlDbUpdateSerializerHelpers.ErasePreviouslyRecordedActions(CommonHelpers.GetBackendUrl(HttpContext), currentDbVersion);
                    }

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

        [ActionAuthorize(ActionCode.DbSettings)]
        [BackendActionContext(ActionCode.DbSettings)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public FileResult GetRecordedUserActions()
        {
            var fileName = $"{QPContext.CurrentCustomerCode}_{System.IO.File.GetLastWriteTime(QPContext.GetRecordXmlFilePath()):yyyy-MM-dd_HH-mm-ss}.xml";
            return File(QPContext.GetRecordXmlFilePath(), MediaTypeNames.Application.Octet, fileName);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.DbSettings)]
        [BackendActionContext(ActionCode.DbSettings)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public JsonCamelCaseResult<JSendResponse> ReplayRecordedUserActions(string xmlString, bool disableFieldIdentity, bool disableContentIdentity, bool useGuidSubstitution)
        {
            try
            {
                new XmlDbUpdateReplayService(
                    QPConfiguration.GetConnectionString(QPContext.CurrentCustomerCode),
                    CommonHelpers.GetDbIdentityInsertOptions(disableFieldIdentity, disableContentIdentity),
                    QPContext.CurrentUserId,
                    useGuidSubstitution,
                    _xmlDbUpdateLogService,
                    _appInfoRepository,
                    _actionsCorrecterService,
                    _httpContextProcessor
                ).Process(xmlString);
            }
            catch (XmlDbUpdateLoggingException ex)
            {
                return new JSendResponse
                {
                    Status = JSendStatus.Fail,
                    Message = ex.Message
                };
            }

            return new JSendResponse
            {
                Status = JSendStatus.Success,
                Message = "Xml data successfully processed"
            };
        }
    }
}

using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Hubs;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
//using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class DbController : AuthQpController
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
        public async Task<ActionResult> Settings(string tabId, int parentId, string successfulActionCode)
        {
            var db = DbService.ReadSettings();
            var model = EntityViewModel.Create<DbViewModel>(db, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;

            ViewBag.IsRecordAvailableForDownload = System.IO.File.Exists(QPContext.GetRecordXmlFilePath());
            return await JsonHtml("Settings", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.UpdateDbSettings)]
        [BackendActionContext(ActionCode.UpdateDbSettings)]
        [BackendActionLog]
        public async Task<ActionResult> Settings(string tabId, int parentId, IFormCollection collection)
        {
            var db = DbService.ReadSettingsForUpdate();
            var model = EntityViewModel.Create<DbViewModel>(db, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                object message = null;
                var needSendMessage = false;
                if (model.Data.RecordActions)
                {
                    if (model.OverrideRecordsFile)
                    {
                        var currentDbVersion = _appInfoRepository.GetCurrentDbVersion();
                        //XmlDbUpdateSerializerHelpers.ErasePreviouslyRecordedActions(CommonHelpers.GetBackendUrl(HttpContext), currentDbVersion);
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
                    await _communicationService.Send("singleusermode", message);
                }

                return Redirect("Settings", new { successfulActionCode = ActionCode.UpdateDbSettings });
            }

            return await JsonHtml("Settings", model);
        }

        [ActionAuthorize(ActionCode.DbSettings)]
        [BackendActionContext(ActionCode.DbSettings)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public FileResult GetRecordedUserActions()
        {
            var fileName = $"{QPContext.CurrentCustomerCode}.xml";
            var stream = System.IO.File.Open(QPContext.GetRecordXmlFilePath(), FileMode.Open);
            return File(stream, MediaTypeNames.Application.Octet, fileName);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.DbSettings)]
        [BackendActionContext(ActionCode.DbSettings)]
        [ExceptionResult(ExceptionResultMode.JSendResponse)]
        public ActionResult ReplayRecordedUserActions(string xmlString, bool generateNewFieldIds, bool generateNewContentIds, bool useGuidSubstitution)
        {
            var info = QPContext.CurrentDbConnectionInfo;
            new XmlDbUpdateReplayService(
                info.ConnectionString,
                info.DbType,
                CommonHelpers.GetDbIdentityInsertOptions(generateNewFieldIds, generateNewContentIds),
                QPContext.CurrentUserId,
                useGuidSubstitution,
                _xmlDbUpdateLogService,
                _appInfoRepository,
                _actionsCorrecterService,
                _httpContextProcessor
            ).Process(xmlString);

            return JsonCamelCase(new JSendResponse
            {
                Status = JSendStatus.Success,
                Message = "Xml data successfully processed"
            });
        }
    }
}

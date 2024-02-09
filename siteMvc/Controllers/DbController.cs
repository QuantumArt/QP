using System;
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
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;

//using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class DbController : AuthQpController
    {
        private readonly IXmlDbUpdateLogService _xmlDbUpdateLogService;
        private readonly IApplicationInfoRepository _appInfoRepository;
        private readonly IXmlDbUpdateHttpContextProcessor _httpContextProcessor;
        private readonly IXmlDbUpdateActionCorrecterService _actionsCorrecterService;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;

        public DbController(
            IXmlDbUpdateLogService xmlDbUpdateServce,
            IApplicationInfoRepository appInfoRepository,
            IXmlDbUpdateHttpContextProcessor httpContextProcessor,
            IXmlDbUpdateActionCorrecterService actionsCorrecterService,
            IUserService userService,
            IServiceProvider provider)
        {
            _xmlDbUpdateLogService = xmlDbUpdateServce;
            _appInfoRepository = appInfoRepository;
            _actionsCorrecterService = actionsCorrecterService;
            _httpContextProcessor = httpContextProcessor;
            _userService = userService;
            _serviceProvider = provider;
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
                    }
                }
                else
                {
                    model.Data.SingleUserId = null;
                }

                model.Data = DbService.UpdateSettings(model.Data);

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
        public ActionResult ReplayRecordedUserActions([FromBody] ReplayViewModel model)
        {
            var info = QPContext.CurrentDbConnectionInfo;
            new XmlDbUpdateReplayService(
                info.ConnectionString,
                info.DbType,
                CommonHelpers.GetDbIdentityInsertOptions(model.GenerateNewFieldIds, model.GenerateNewContentIds),
                QPContext.CurrentUserId,
                model.UseGuidSubstitution,
                _xmlDbUpdateLogService,
                _appInfoRepository,
                _actionsCorrecterService,
                _httpContextProcessor,
                _serviceProvider,
                throwActionReplayed: true
            ).Process(model.XmlString);

            return JsonCamelCase(new JSendResponse
            {
                Status = JSendStatus.Success,
                Message = "Xml data successfully processed"
            });
        }

        [HttpGet]
        public ActionResult CheckDbMode()
        {
            var settings = DbService.ReadSettings();

            object message = null;

            if (settings.SingleUserId.HasValue)
            {
                string userName = null;

                if (settings.SingleUserId.Value != QPContext.CurrentUserId)
                {
                    var user = _userService.ReadProfile(settings.SingleUserId.Value);
                    userName = user.Name;
                }

                message = new
                {
                    userId = settings.SingleUserId.Value,
                    userName
                };
            }

            return JsonCamelCase(new JSendResponse
            {
                Status = JSendStatus.Success,
                Data = message
            });
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.Validation.Xaml.Extensions.Rules;
using QP8.Infrastructure.Web.Enums;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.CommonScheduler;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.WebMvc.ViewModels.HomePage;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class HomeController : AuthQpController
    {
        private JsLanguageHelper _languageHelper;
        private JsConstantsHelper _constantsHelper;
        private QPublishingOptions _options;

        private QuartzService _quartzService;

        public HomeController(JsLanguageHelper languageHelper, JsConstantsHelper constantsHelper, QPublishingOptions options, QuartzService quartzService)
        {
            _languageHelper = languageHelper;
            _constantsHelper = constantsHelper;
            _options = options;
            _quartzService = quartzService;
        }

        [DisableBrowserCache]
        public ActionResult Index(DirectLinkOptions directLinkOptions)
        {
            DbService.ResetUserCache();
            return View(new IndexViewModel(directLinkOptions, DbService.ReadSettings(), DbService.GetDbHash(), _options));
        }

        public async Task<ActionResult> Home(string tabId, int parentId)
        {
            var model = HomeViewModel.Create(tabId, parentId, DbService.Home());
            return await JsonHtml("Home", model);
        }

        public async Task<ActionResult> About(string tabId, int parentId)
        {
            var envVersion = Environment.GetEnvironmentVariable("SERVICE_VERSION");
            var model = AboutViewModel.Create(tabId, parentId, envVersion ?? _options.BuildVersion);
            return await JsonHtml("About", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ScheduledTasks)]
        [BackendActionContext(ActionCode.ScheduledTasks)]
        public async Task<IActionResult> ScheduledTasks(string tabId, int parentId)
        {
            var tasks = await _quartzService.GetAllTasks();

            var model = ScheduledTasksViewModel.Create(tabId, parentId, tasks);
            return await JsonHtml("ScheduledTasks", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ScheduledTasks)]
        [BackendActionContext(ActionCode.ScheduledTasks)]
        public async Task<ActionResult> RunJob([FromBody] ScheduledTaskViewModel model)
        {
            var result = await _quartzService.RunJob(model.Name, QPContext.CurrentCustomerCode);

            return JsonCamelCase(new JSendResponse
            {
                Status = result.Status,
                Message = result.Message
            });
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ScheduledTasks)]
        [BackendActionContext(ActionCode.ScheduledTasks)]
        public async Task<ActionResult> StopJob([FromBody] ScheduledTaskViewModel model)
        {
            var result = await _quartzService.InterruptJob(model.Name);

            return JsonCamelCase(new JSendResponse
            {
                Status = result.Status,
                Message = result.Message
            });
        }

        public ActionResult Lang()
        {
            return Content(_languageHelper.GetResult(), "text/javascript");
        }

        public ActionResult Constants()
        {
            return Content(_constantsHelper.GetResult(), "text/javascript");
        }

        public ActionResult TestValidation()
        {
            var result = new ProcessRemoteValidationIf();
            return Content(result.ToString());
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.LockedArticles)]
        [BackendActionContext(ActionCode.LockedArticles)]
        public async Task<ActionResult> LockedArticles(string tabId, int parentId, int id)
        {
            var model = ArticleBaseListViewModel.Create(id, tabId, parentId);
            model.DataBindingActionName = "_LockedArticles";
            return await JsonHtml("LockedArticles", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.LockedArticles)]
        [BackendActionContext(ActionCode.LockedArticles)]
        public ActionResult _LockedArticles(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = BLL.Services.ArticleServices.ArticleService.ListLocked(listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticlesForApproval)]
        [BackendActionContext(ActionCode.ArticlesForApproval)]
        public async Task<ActionResult> ArticlesForApproval(string tabId, int parentId, int id)
        {
            var model = ArticleBaseListViewModel.Create(id, tabId, parentId);
            model.DataBindingActionName = "_ArticlesForApproval";
            model.AllowMultipleEntitySelection = false;
            return await JsonHtml("ArticlesForApproval", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.ArticlesForApproval)]
        [BackendActionContext(ActionCode.ArticlesForApproval)]
        public ActionResult _ArticlesForApproval(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = BLL.Services.ArticleServices.ArticleService.ArticlesForApproval(listCommand);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UnlockArticles)]
        [BackendActionContext(ActionCode.UnlockArticles)]
        [BackendActionLog]
        public ActionResult UnlockArticles([FromBody] SelectedItemsViewModel selModel)
        {
            BLL.Services.ArticleServices.ArticleService.UnlockArticles(selModel.Ids);
            return Json(null);
        }


    }
}

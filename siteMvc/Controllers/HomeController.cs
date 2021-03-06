using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.Validation.Xaml.Extensions.Rules;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.WebMvc.ViewModels.HomePage;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class HomeController : AuthQpController
    {
        private JsLanguageHelper _languageHelper;
        private JsConstantsHelper _constantsHelper;
        private QPublishingOptions _options;


        public HomeController(JsLanguageHelper languageHelper, JsConstantsHelper constantsHelper, QPublishingOptions options)
        {
            _languageHelper = languageHelper;
            _constantsHelper = constantsHelper;
            _options = options;
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
            var model = AboutViewModel.Create(tabId, parentId, envVersion ?? _options.Version);
            return await JsonHtml("About", model);
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
            var serviceResult = ArticleService.ListLocked(listCommand);
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
            var serviceResult = ArticleService.ArticlesForApproval(listCommand);
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
            ArticleService.UnlockArticles(selModel.Ids);
            return Json(null);
        }
    }
}

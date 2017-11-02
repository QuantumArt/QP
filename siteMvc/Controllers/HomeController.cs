using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.WebMvc.ViewModels.HomePage;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class HomeController : QPController
    {
        [HttpGet]
        [DisableBrowserCache]
        public ActionResult Index(DirectLinkOptions directLinkOptions) => View(new IndexViewModel(directLinkOptions, DbService.ReadSettings(), DbService.GetDbHash()));

        [HttpGet]
        public ActionResult Test() => View();

        [HttpGet]
        public ActionResult JQueryTest() => View();

        [HttpPost]
        public ActionResult JQueryTest(FormCollection formData) => View();

        [HttpGet]
        public ActionResult Home(string tabId, int parentId)
        {
            var model = HomeViewModel.Create(tabId, parentId, DbService.Home());
            return JsonHtml("Home", model);
        }

        [HttpGet]
        public ActionResult About(string tabId, int parentId)
        {
            var model = ViewModel.Create<AboutViewModel>(tabId, parentId);
            return JsonHtml("About", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.LockedArticles)]
        [BackendActionContext(ActionCode.LockedArticles)]
        public ActionResult LockedArticles(string tabId, int parentId, int id)
        {
            var model = ArticleBaseListViewModel.Create(id, tabId, parentId);
            model.DataBindingActionName = "_LockedArticles";
            return JsonHtml("LockedArticles", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.LockedArticles)]
        [BackendActionContext(ActionCode.LockedArticles)]
        public ActionResult _LockedArticles(string tabId, int parentId, int id, GridCommand command)
        {
            var result = ArticleService.ListLocked(command.GetListCommand());
            return View(new GridModel { Data = result.Data, Total = result.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticlesForApproval)]
        [BackendActionContext(ActionCode.ArticlesForApproval)]
        public ActionResult ArticlesForApproval(string tabId, int parentId, int id)
        {
            var model = ArticleBaseListViewModel.Create(id, tabId, parentId);
            model.DataBindingActionName = "_ArticlesForApproval";
            model.AllowMultipleEntitySelection = false;
            return JsonHtml("ArticlesForApproval", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ArticlesForApproval)]
        [BackendActionContext(ActionCode.ArticlesForApproval)]
        public ActionResult _ArticlesForApproval(string tabId, int parentId, int id, GridCommand command)
        {
            var result = ArticleService.ArticlesForApproval(command.GetListCommand());
            return View(new GridModel { Data = result.Data, Total = result.TotalRecords });
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UnlockArticles)]
        [BackendActionContext(ActionCode.UnlockArticles)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult UnlockArticles(int[] IDs)
        {
            ArticleService.UnlockArticles(IDs);
            return Json(null);
        }
    }
}

using System.Web.Mvc;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.WebMvc.ViewModels.HomePage;
using Telerik.Web.Mvc;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class HomeController : QPController
    {
		/// <summary>
		/// Выводит главную страницу системы
		/// </summary>
		[HttpGet]
		[DisableBrowserCache]
		public ActionResult Index(DirectLinkOptions directLinkOptions)
        {
			Db data = DbService.ReadSettings();
			var dhHash = DbService.GetDbHash();
            return View(new IndexViewModel(directLinkOptions, data, dhHash));
        }

		[HttpGet]
        public ActionResult Test()
        {
            return View();
        }

		[HttpGet]
		public ActionResult JQueryTest()
		{
			return View();
		}

		[HttpPost]
		public ActionResult JQueryTest(FormCollection formData)
		{
			return View();
		}

		[HttpGet]
		public ActionResult Home(string tabId, int parentId)
		{
			HomeViewModel model = HomeViewModel.Create(tabId, parentId, DbService.Home());
			return this.JsonHtml("Home", model);
		}

		[HttpGet]
		public ActionResult About(string tabId, int parentId)
		{
			AboutViewModel model = AboutViewModel.Create<AboutViewModel>(tabId, parentId);
			return this.JsonHtml("About", model);
		}

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.LockedArticles)]
        [BackendActionContext(ActionCode.LockedArticles)]
        public ActionResult LockedArticles(string tabId, int parentId, int id)
        {
            ArticleBaseListViewModel model = ArticleBaseListViewModel.Create(id, tabId,parentId);
            model.DataBindingActionName = "_LockedArticles";
            return this.JsonHtml("LockedArticles", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.LockedArticles)]
        [BackendActionContext(ActionCode.LockedArticles)]
        public ActionResult _LockedArticles(string tabId, int parentId, int id, GridCommand command)
        {
            ListResult<ArticleListItem> result = ArticleService.ListLocked(command.GetListCommand());
            return View(new GridModel() { Data = result.Data, Total = result.TotalRecords });
        }


        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.ArticlesForApproval)]
        [BackendActionContext(ActionCode.ArticlesForApproval)]
        public ActionResult ArticlesForApproval(string tabId, int parentId, int id)
        {
            ArticleBaseListViewModel model = ArticleBaseListViewModel.Create(id, tabId, parentId);
            model.DataBindingActionName = "_ArticlesForApproval";
            model.AllowMultipleEntitySelection = false;
            return this.JsonHtml("ArticlesForApproval", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.ArticlesForApproval)]
        [BackendActionContext(ActionCode.ArticlesForApproval)]
        public ActionResult _ArticlesForApproval(string tabId, int parentId, int id, GridCommand command)
        {
            ListResult<ArticleListItem> result = ArticleService.ArticlesForApproval(command.GetListCommand());
            return View(new GridModel() { Data = result.Data, Total = result.TotalRecords });
        }
        #region non-interface actions

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope()]
        [ActionAuthorize(ActionCode.UnlockArticles)]
        [BackendActionContext(ActionCode.UnlockArticles)]
        [BackendActionLog]
        public ActionResult UnlockArticles(int[] IDs)
        {
            ArticleService.UnlockArticles(IDs);
            return Json(null);
        }
        #endregion

    }
}

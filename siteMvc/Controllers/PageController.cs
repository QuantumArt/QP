using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
	public class PageController : QPController
    {
		IPageService _pageService;

		public PageController(IPageService pageService)
		{
            this._pageService = pageService;
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.Pages)]
		[BackendActionContext(ActionCode.Pages)]
		public ActionResult IndexPages(string tabId, int parentId)
		{
			PageInitListResult result = _pageService.InitPageList(parentId);
			PageListViewModel model = PageListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.Pages)]
		[BackendActionContext(ActionCode.Pages)]
		public ActionResult _IndexPages(string tabId, int parentId, GridCommand command)
		{
			ListResult<PageListItem> serviceResult = _pageService.GetPagesByTemplateId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.AddNewPage)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Page, "parentId")]
		[BackendActionContext(ActionCode.AddNewPage)]
		public ActionResult NewPage(string tabId, int parentId)
		{
			Page page = _pageService.NewPageProperties(parentId);
			PageViewModel model = PageViewModel.Create(page, tabId, parentId, _pageService);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.AddNewPage)]
		[BackendActionContext(ActionCode.AddNewPage)]
		[BackendActionLog]
		[Record]
		public ActionResult NewPage(string tabId, int parentId, FormCollection collection)
		{
			Page page = _pageService.NewPagePropertiesForUpdate(parentId);
			PageViewModel model = PageViewModel.Create(page, tabId, parentId, _pageService);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _pageService.SavePageProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				return Redirect("PageProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SavePage });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.PageProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Page, "id")]
		[BackendActionContext(ActionCode.PageProperties)]
		public ActionResult PageProperties(string tabId, int parentId, int id, string successfulActionCode)
		{
			Page page = _pageService.ReadPageProperties(id);
			ViewData[SpecialKeys.IsEntityReadOnly] = page.LockedByAnyoneElse;
			PageViewModel model = PageViewModel.Create(page, tabId, parentId, _pageService);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.UpdatePage)]
		[BackendActionContext(ActionCode.UpdatePage)]
		[BackendActionLog]
		[Record(ActionCode.PageProperties)]
		public ActionResult PageProperties(string tabId, int parentId, int id, FormCollection collection)
		{
			Page page = _pageService.ReadPagePropertiesForUpdate(id);
			PageViewModel model = PageViewModel.Create(page, tabId, parentId, _pageService);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _pageService.UpdatePageProperties(model.Data);
				return Redirect("PageProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdatePageTemplate });
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.MultipleRemovePage)]
		[BackendActionContext(ActionCode.MultipleRemovePage)]
		[BackendActionLog]
		[Record]
		public ActionResult MultipleRemovePage(int parentId, int[] IDs)
		{
			return this.JsonMessageResult(_pageService.MultipleRemovePage(IDs));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.RemovePage)]
		[BackendActionContext(ActionCode.RemovePage)]
		[BackendActionLog]
		[Record]
		public ActionResult RemovePage(int id)
		{
			MessageResult result = _pageService.RemovePage(id);
			return this.JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.CancelPage)]
		[BackendActionContext(ActionCode.CancelPage)]
		public ActionResult CancelPage(int id)
		{
			_pageService.CancelPage(id);
			return this.JsonMessageResult(null);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UiAction)]
		[ActionAuthorize(ActionCode.SelectPageForObjectForm)]
		[BackendActionContext(ActionCode.SelectPageForObjectForm)]
		public ActionResult SelectPages(string tabId, int parentId, int id)
		{
			var template = _pageService.ReadPageTemplateProperties(parentId);
			PageInitListResult result = _pageService.InitPageListForSite(template.SiteId);
			PageSelectableListViewModel model = new PageSelectableListViewModel(result, tabId, parentId, new[] { id });
			return this.JsonHtml("SelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.SelectPageForObjectForm)]
		[BackendActionContext(ActionCode.SelectPageForObjectForm)]
		public ActionResult _SelectPages(string tabId, int id, GridCommand command, int parentId)
		{
			var template = _pageService.ReadPageTemplateProperties(parentId);
			ListResult<PageListItem> serviceResult = _pageService.ListPagesForSite(command.GetListCommand(), template.SiteId, id);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpPost]
		public ActionResult AssemblePagePreAction(int id)
		{
			return this.JsonMessageResult(_pageService.AssemblePagePreAction(id));
		}

		[HttpPost]
		public ActionResult MultipleAssemblePagePreAction(int[] IDs)
		{
			return this.JsonMessageResult(_pageService.MultipleAssemblePagePreAction(IDs));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.AssemblePage)]
		[BackendActionContext(ActionCode.AssemblePage)]
		[BackendActionLog]
		public ActionResult AssemblePage(int id)
		{
			return this.JsonMessageResult(_pageService.AssemblePage(id));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[NoTransactionConnectionScopeAttribute]
		[ActionAuthorize(ActionCode.MultipleAssemblePage)]
		[BackendActionContext(ActionCode.MultipleAssemblePage)]
		[BackendActionLog]
		public ActionResult MultipleAssemblePage(int[] IDs)
		{
			return this.JsonMessageResult(_pageService.MultipleAssemblePage(IDs));
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.CaptureLockPage)]
		[BackendActionContext(ActionCode.CaptureLockPage)]
		[BackendActionLog]
		public ActionResult CaptureLockPage(int id)
		{
			_pageService.CaptureLockPage(id);
			return this.JsonMessageResult(null);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope()]
		[ActionAuthorize(ActionCode.CreateLikePage)]
		[BackendActionContext(ActionCode.CreateLikePage)]
		[BackendActionLog]
		[Record]
		public ActionResult Copy(int id)
		{
			CopyResult result = _pageService.Copy(id);
			this.PersistResultId(result.Id);
			this.PersistFromId(id);
			return this.JsonMessageResult(result.Message);
		}
    }
}

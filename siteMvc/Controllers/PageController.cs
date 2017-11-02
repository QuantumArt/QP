using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class PageController : QPController
    {
        private readonly IPageService _pageService;

        public PageController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Pages)]
        [BackendActionContext(ActionCode.Pages)]
        public ActionResult IndexPages(string tabId, int parentId)
        {
            var result = _pageService.InitPageList(parentId);
            var model = PageListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Pages)]
        [BackendActionContext(ActionCode.Pages)]
        public ActionResult _IndexPages(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = _pageService.GetPagesByTemplateId(command.GetListCommand(), parentId);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewPage)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Page, "parentId")]
        [BackendActionContext(ActionCode.AddNewPage)]
        public ActionResult NewPage(string tabId, int parentId)
        {
            var page = _pageService.NewPageProperties(parentId);
            var model = PageViewModel.Create(page, tabId, parentId, _pageService);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewPage)]
        [BackendActionContext(ActionCode.AddNewPage)]
        [BackendActionLog]
        public ActionResult NewPage(string tabId, int parentId, FormCollection collection)
        {
            var page = _pageService.NewPagePropertiesForUpdate(parentId);
            var model = PageViewModel.Create(page, tabId, parentId, _pageService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _pageService.SavePageProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("PageProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SavePage });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Page, "id")]
        [BackendActionContext(ActionCode.PageProperties)]
        public ActionResult PageProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var page = _pageService.ReadPageProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = page.LockedByAnyoneElse;
            var model = PageViewModel.Create(page, tabId, parentId, _pageService);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdatePage)]
        [BackendActionContext(ActionCode.UpdatePage)]
        [BackendActionLog]
        [Record(ActionCode.PageProperties)]
        public ActionResult PageProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            var page = _pageService.ReadPagePropertiesForUpdate(id);
            var model = PageViewModel.Create(page, tabId, parentId, _pageService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _pageService.UpdatePageProperties(model.Data);
                return Redirect("PageProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdatePageTemplate });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemovePage)]
        [BackendActionContext(ActionCode.MultipleRemovePage)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemovePage(int parentId, int[] IDs) => JsonMessageResult(_pageService.MultipleRemovePage(IDs));

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemovePage)]
        [BackendActionContext(ActionCode.RemovePage)]
        [BackendActionLog]
        public ActionResult RemovePage(int id)
        {
            var result = _pageService.RemovePage(id);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelPage)]
        [BackendActionContext(ActionCode.CancelPage)]
        public ActionResult CancelPage(int id)
        {
            _pageService.CancelPage(id);
            return JsonMessageResult(null);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SelectPageForObjectForm)]
        [BackendActionContext(ActionCode.SelectPageForObjectForm)]
        public ActionResult SelectPages(string tabId, int parentId, int id)
        {
            var template = _pageService.ReadPageTemplateProperties(parentId);
            var result = _pageService.InitPageListForSite(template.SiteId);
            var model = new PageSelectableListViewModel(result, tabId, parentId, new[] { id });
            return JsonHtml("SelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.SelectPageForObjectForm)]
        [BackendActionContext(ActionCode.SelectPageForObjectForm)]
        public ActionResult _SelectPages(string tabId, int id, GridCommand command, int parentId)
        {
            var template = _pageService.ReadPageTemplateProperties(parentId);
            var serviceResult = _pageService.ListPagesForSite(command.GetListCommand(), template.SiteId, id);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpPost]
        public ActionResult AssemblePagePreAction(int id) => JsonMessageResult(_pageService.AssemblePagePreAction(id));

        [HttpPost]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssemblePagePreAction(int[] IDs) => JsonMessageResult(_pageService.MultipleAssemblePagePreAction(IDs));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssemblePage)]
        [BackendActionContext(ActionCode.AssemblePage)]
        [BackendActionLog]
        public ActionResult AssemblePage(int id) => JsonMessageResult(_pageService.AssemblePage(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.MultipleAssemblePage)]
        [BackendActionContext(ActionCode.MultipleAssemblePage)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssemblePage(int[] IDs) => JsonMessageResult(_pageService.MultipleAssemblePage(IDs));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockPage)]
        [BackendActionContext(ActionCode.CaptureLockPage)]
        [BackendActionLog]
        public ActionResult CaptureLockPage(int id)
        {
            _pageService.CaptureLockPage(id);
            return JsonMessageResult(null);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CreateLikePage)]
        [BackendActionContext(ActionCode.CreateLikePage)]
        [BackendActionLog]
        public ActionResult Copy(int id)
        {
            var result = _pageService.Copy(id);
            PersistResultId(result.Id);
            PersistFromId(id);
            return JsonMessageResult(result.Message);
        }
    }
}

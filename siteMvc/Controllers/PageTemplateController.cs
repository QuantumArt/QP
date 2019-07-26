using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class PageTemplateController : QPController
    {
        private readonly IPageTemplateService _pageTemplateService;

        public PageTemplateController(IPageTemplateService pageTemplateService)
        {
            _pageTemplateService = pageTemplateService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Templates)]
        [BackendActionContext(ActionCode.Templates)]
        public ActionResult IndexTemplates(string tabId, int parentId)
        {
            var result = _pageTemplateService.InitTemplateList(parentId);
            var model = PageTemplateListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.Templates)]
        [BackendActionContext(ActionCode.Templates)]
        public ActionResult _IndexTemplates(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _pageTemplateService.GetPageTemplatesBySiteId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewPageTemplate)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.PageTemplate, "parentId")]
        [BackendActionContext(ActionCode.AddNewPageTemplate)]
        public ActionResult NewPageTemplate(string tabId, int parentId)
        {
            var template = _pageTemplateService.NewPageTemplateProperties(parentId);
            var model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewPageTemplate)]
        [BackendActionContext(ActionCode.AddNewPageTemplate)]
        [BackendActionLog]
        public ActionResult NewPageTemplate(string tabId, int parentId, FormCollection collection)
        {
            var template = _pageTemplateService.NewPageTemplatePropertiesForUpdate(parentId);
            var model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _pageTemplateService.SavePageTemplateProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("PageTemplateProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SavePageTemplate });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageTemplateProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageTemplate, "id")]
        [BackendActionContext(ActionCode.PageTemplateProperties)]
        public ActionResult PageTemplateProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var template = _pageTemplateService.ReadPageTemplateProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = template.LockedByAnyoneElse;
            var model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdatePageTemplate)]
        [BackendActionContext(ActionCode.UpdatePageTemplate)]
        [BackendActionLog]
        [Record(ActionCode.PageTemplateProperties)]
        public ActionResult PageTemplateProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            var template = _pageTemplateService.ReadPageTemplatePropertiesForUpdate(id);
            var model = PageTemplateViewModel.Create(template, tabId, parentId, _pageTemplateService);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _pageTemplateService.UpdatePageTemplateProperties(model.Data);
                return Redirect("PageTemplateProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdatePageTemplate });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemovePageTemplate)]
        [BackendActionContext(ActionCode.RemovePageTemplate)]
        [BackendActionLog]
        [Record]
        public ActionResult RemovePageTemplate(int id)
        {
            var result = _pageTemplateService.RemovePageTemplate(id);
            return JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelTemplate)]
        [BackendActionContext(ActionCode.CancelTemplate)]
        public ActionResult CancelTemplate(int id)
        {
            _pageTemplateService.CancelTemplate(id);
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetFieldsByContentId(int contentId)
        {
            var content = _pageTemplateService.GetContentById(contentId);
            var statuses = _pageTemplateService.GetStatusIdsByContentId(contentId, out var hasWorkflow);
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    fields = string.Join(",", ServiceField.CreateAll().Select(f => f.ColumnName).Concat(content.Fields.Select(x => x.Name))),
                    statuses,
                    hasWorkflow
                },
                JsonRequestBehavior = JsonRequestBehavior.DenyGet
            };
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssemblePageFromPageObject)]
        [BackendActionContext(ActionCode.AssemblePageFromPageObject)]
        [BackendActionLog]
        public ActionResult AssemblePageFromPageObject(string tabId, int parentId, int id) => Json(_pageTemplateService.AssemblePageFromPageObject(parentId));

        [HttpPost]
        public ActionResult AssemblePageFromPageObjectPreAction(string tabId, int parentId, int id) => Json(_pageTemplateService.AssemblePageFromPageObjectPreAction(parentId));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssemblePageFromPageObjectFormat)]
        [BackendActionContext(ActionCode.AssemblePageFromPageObjectFormat)]
        [BackendActionLog]
        public ActionResult AssemblePageFromPageObjectFormat(string tabId, int parentId, int id) => Json(_pageTemplateService.AssemblePageFromPageObjectFormat(parentId));

        [HttpPost]
        public ActionResult AssemblePageFromPageObjectFormatPreAction(string tabId, int parentId, int id) => Json(_pageTemplateService.AssemblePageFromPageObjectFormatPreAction(parentId));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssemblePageFromPageObjectList)]
        [BackendActionContext(ActionCode.AssemblePageFromPageObjectList)]
        [BackendActionLog]
        public ActionResult AssemblePageFromPageObjectList(string tabId, int parentId, int id) => Json(_pageTemplateService.AssemblePageFromPageObjectList(parentId));

        [HttpPost]
        public ActionResult AssemblePageFromPageObjectListPreAction(string tabId, int parentId, int id) => Json(_pageTemplateService.AssemblePageFromPageObjectListPreAction(parentId));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssembleObjectFromPageObjectFormat)]
        [BackendActionContext(ActionCode.AssembleObjectFromPageObjectFormat)]
        [BackendActionLog]
        public ActionResult AssembleObjectFromPageObjectFormat(string tabId, int parentId, int id) => Json(_pageTemplateService.AssembleObjectFromPageObjectFormat(parentId));

        [HttpPost]
        public ActionResult AssembleObjectFromPageObjectFormatPreAction(string tabId, int parentId, int id) => Json(_pageTemplateService.AssembleObjectFromPageObjectFormatPreAction(parentId));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssembleObjectFromPageObjectFormat)]
        [BackendActionContext(ActionCode.AssembleObjectFromPageObjectFormat)]
        [BackendActionLog]
        public ActionResult AssembleObjectFromTemplateObjectFormat(string tabId, int parentId, int id) => Json(_pageTemplateService.AssembleObjectFromTemplateObjectFormat(parentId));

        [HttpPost]
        public ActionResult AssembleObjectFromTemplateObjectFormatPreAction(string tabId, int parentId, int id) => Json(_pageTemplateService.AssembleObjectFromTemplateObjectFormatPreAction(parentId));

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SearchInCode)]
        [BackendActionContext(ActionCode.SearchInCode)]
        public ActionResult Formats(string tabId, int parentId, int id)
        {
            var model = SearchInFormatsViewModel.Create(tabId, parentId, id, _pageTemplateService);
            return JsonHtml("Formats", model);
        }

        [ActionAuthorize(ActionCode.SearchInCode)]
        [BackendActionContext(ActionCode.SearchInCode)]
        public ActionResult _Formats(
            string tabId, int parentId, int id, int? templateId, int? pageId, string filterVal,
            int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _pageTemplateService.FormatSearch(listCommand, id, templateId, pageId, filterVal);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SearchInTemplates)]
        [BackendActionContext(ActionCode.SearchInTemplates)]
        public ActionResult Templates(string tabId, int parentId)
        {
            var model = SearchInTemplatesViewModel.Create(tabId, parentId);
            return JsonHtml("Templates", model);
        }

        [ActionAuthorize(ActionCode.SearchInTemplates)]
        [BackendActionContext(ActionCode.SearchInTemplates)]
        public ActionResult _Templates(
            string tabId, int parentId, string filterVal,
            int page, int pageSize, string orderBy,
            // TODO: review where 'id' parameter should came from
            int id = 0)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _pageTemplateService.TemplateSearch(listCommand, id, filterVal);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.SearchInObjects)]
        [BackendActionContext(ActionCode.SearchInObjects)]
        public ActionResult Parameters(string tabId, int parentId, int id)
        {
            var model = SearchInObjectsViewModel.Create(tabId, parentId, id, _pageTemplateService);
            return JsonHtml("Objects", model);
        }

        [ActionAuthorize(ActionCode.SearchInObjects)]
        [BackendActionContext(ActionCode.SearchInObjects)]
        public ActionResult _Parameters(
            string tabId, int parentId, int id, int? templateId, int? pageId, string filterVal,
            int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _pageTemplateService.ObjectSearch(listCommand, id, templateId, pageId, filterVal);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockTemplate)]
        [BackendActionContext(ActionCode.CaptureLockTemplate)]
        [BackendActionLog]
        public ActionResult CaptureLockTemplate(int id)
        {
            _pageTemplateService.CaptureLockTemplate(id);
            return Json(null);
        }

        [HttpPost]
        public JsonResult GetDefaultCode(int formatId)
        {
            var defaultCode = _pageTemplateService.ReadDefaultCode(formatId);
            return new JsonResult
            {
                Data = new { success = true, code = defaultCode },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public JsonResult GetDefaultPresentation(int formatId)
        {
            var defaultCode = _pageTemplateService.ReadDefaultPresentation(formatId);
            return new JsonResult
            {
                Data = new { success = true, code = defaultCode },
                JsonRequestBehavior = JsonRequestBehavior.DenyGet
            };
        }

        [HttpPost]
        public JsonResult GetInsertPopUpMarkUp(int templateId, int? formatId, bool presentationOrCodeBehind)
        {
            int? languageId;
            string assemblingType;
            bool isForm, isContainer;
            int? pageId = null;
            int? contentId = null;

            if (formatId.HasValue)
            {
                var format = _pageTemplateService.ReadFormatProperties(formatId.Value, true, false);
                languageId = format.NetLanguageId;
                assemblingType = _pageTemplateService.ReadPageTemplateProperties(templateId).Site.AssemblingType;

                var obj = _pageTemplateService.ReadObjectProperties(format.ObjectId, false);
                isContainer = obj.IsObjectContainerType;
                pageId = obj.PageId;
                isForm = obj.IsObjectFormType;

                if (isForm)
                {
                    contentId = obj.ContentForm.ContentId;
                }
                else if (isContainer)
                {
                    contentId = obj.Container.ContentId;
                }
            }
            else
            {
                var template = _pageTemplateService.ReadPageTemplateProperties(templateId);
                languageId = template.NetLanguageId;
                assemblingType = template.Site.AssemblingType;
                isContainer = false;
                isForm = false;
            }

            return new JsonResult
            {
                Data = new
                {
                    html = RenderPartialView("InsertPopupWindow", new InsertPopupViewModel(templateId, languageId, assemblingType, presentationOrCodeBehind, isContainer, isForm, contentId, pageId, _pageTemplateService))
                },
                JsonRequestBehavior = JsonRequestBehavior.DenyGet
            };
        }

        [HttpPost]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public JsonResult GetHTAToolbarMarkUp(bool presentationOrCodeBehind, int? formatId, int? templateId) => new JsonResult
        {
            Data = new
            {
                html = RenderPartialView("HTAreaToolbar", new HtAreaToolbarViewModel(presentationOrCodeBehind, formatId, templateId))
            },
            JsonRequestBehavior = JsonRequestBehavior.DenyGet
        };
    }
}

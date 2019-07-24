using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.VisualEditor;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class VisualEditorStyleController : QPController
    {
        private readonly IVisualEditorService _visualEditorService;

        public VisualEditorStyleController(IVisualEditorService visualEditorService)
        {
            _visualEditorService = visualEditorService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VisualEditorStyles)]
        [BackendActionContext(ActionCode.VisualEditorStyles)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = _visualEditorService.InitVisualEditorStyleList(parentId);
            var model = VisualEditorStyleListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.VisualEditorStyles)]
        [BackendActionContext(ActionCode.VisualEditorStyles)]
        public ActionResult _Index(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _visualEditorService.GetVisualEditorStyles(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.VisualEditorStyleProperties)]
        [BackendActionContext(ActionCode.VisualEditorStyleProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var style = _visualEditorService.ReadVisualEditorStyleProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = style.IsSystem;
            var model = VisualEditorStyleViewModel.Create(style, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.VisualEditorStyleProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateVisualEditorStyle)]
        [BackendActionContext(ActionCode.UpdateVisualEditorStyle)]
        [BackendActionLog]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var style = _visualEditorService.ReadVisualEditorStylePropertiesForUpdate(id);
            var model = VisualEditorStyleViewModel.Create(style, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _visualEditorService.UpdateVisualEditorStyleProperties(model.Data);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateVisualEditorPlugin });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveVisualEditorStyle)]
        [BackendActionContext(ActionCode.RemoveVisualEditorStyle)]
        [BackendActionLog]
        public ActionResult Remove(int id) => JsonMessageResult(_visualEditorService.RemoveVisualEditorStyle(id));

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewVisualEditorStyle)]
        [BackendActionContext(ActionCode.AddNewVisualEditorStyle)]
        public ActionResult New(string tabId, int parentId)
        {
            var style = _visualEditorService.NewVisualEditorStyleProperties(parentId);
            var model = VisualEditorStyleViewModel.Create(style, tabId, parentId);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewVisualEditorStyle)]
        [BackendActionContext(ActionCode.AddNewVisualEditorStyle)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var style = _visualEditorService.NewVisualEditorStylePropertiesForUpdate(parentId);
            var model = VisualEditorStyleViewModel.Create(style, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _visualEditorService.SaveVisualEditorStyleProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveVisualEditorStyle });
            }

            return JsonHtml("Properties", model);
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.StatusType;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class StatusTypeController : QPController
    {
        private readonly IStatusTypeService _statusTypeService;

        public StatusTypeController(IStatusTypeService statusTypeService)
        {
            _statusTypeService = statusTypeService;
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.StatusTypes)]
        [BackendActionContext(ActionCode.StatusTypes)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = _statusTypeService.InitList(parentId);
            var model = StatusTypeListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.StatusTypes)]
        [BackendActionContext(ActionCode.StatusTypes)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = _statusTypeService.GetStatusesBySiteId(command.GetListCommand(), parentId);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.StatusTypeProperties)]
        [BackendActionContext(ActionCode.StatusTypeProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var status = _statusTypeService.ReadProperties(id);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateStatusType)]
        [BackendActionContext(ActionCode.UpdateStatusType)]
        [BackendActionLog]
        [Record(ActionCode.StatusTypeProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var status = _statusTypeService.ReadPropertiesForUpdate(id);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _statusTypeService.UpdateProperties(model.Data);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateVisualEditorPlugin });
            }

            return JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewStatusType)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewStatusType)]
        public ActionResult New(string tabId, int parentId)
        {
            var status = _statusTypeService.NewStatusTypeProperties(parentId);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewStatusType)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewStatusType)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var status = _statusTypeService.NewStatusTypePropertiesForUpdate(parentId);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _statusTypeService.SaveProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveStatusType });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveStatusType)]
        [BackendActionContext(ActionCode.RemoveStatusType)]
        [BackendActionLog]
        public ActionResult Remove(int id) => Json(_statusTypeService.Remove(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectStatusesForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectStatusesForWorkflow)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleSelectForWorkflow(string tabId, int parentId, int[] IDs)
        {
            _statusTypeService.InitList(parentId);
            var model = new StatusTypeSelectableListViewModel(tabId, parentId, IDs);
            return JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.MultipleSelectStatusesForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectStatusesForWorkflow)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult _MultipleSelectForWorkflow(string tabId, string IDs, GridCommand command, int parentId)
        {
            var serviceResult = _statusTypeService.ListForWorkflow(command.GetListCommand(), Converter.ToInt32Collection(IDs, ','), parentId);
            return View(new GridModel { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }
    }
}

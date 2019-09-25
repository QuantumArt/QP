using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.StatusType;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class StatusTypeController : AuthQpController
    {
        private readonly IStatusTypeService _statusTypeService;

        public StatusTypeController(IStatusTypeService statusTypeService)
        {
            _statusTypeService = statusTypeService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.StatusTypes)]
        [BackendActionContext(ActionCode.StatusTypes)]
        public async Task<ActionResult> Index(string tabId, int parentId)
        {
            var result = _statusTypeService.InitList(parentId);
            var model = StatusTypeListViewModel.Create(result, tabId, parentId);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.StatusTypes)]
        [BackendActionContext(ActionCode.StatusTypes)]
        public ActionResult _Index(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _statusTypeService.GetStatusesBySiteId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.StatusTypeProperties)]
        [BackendActionContext(ActionCode.StatusTypeProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var status = _statusTypeService.ReadProperties(id);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateStatusType)]
        [BackendActionContext(ActionCode.UpdateStatusType)]
        [BackendActionLog]
        [Record(ActionCode.StatusTypeProperties)]
        public async Task<ActionResult> Properties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var status = _statusTypeService.ReadPropertiesForUpdate(id);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _statusTypeService.UpdateProperties(model.Data);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateVisualEditorPlugin });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewStatusType)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewStatusType)]
        public async Task<ActionResult> New(string tabId, int parentId)
        {
            var status = _statusTypeService.NewStatusTypeProperties(parentId);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewStatusType)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewStatusType)]
        [BackendActionLog]
        public async Task<ActionResult> New(string tabId, int parentId, IFormCollection collection)
        {
            var status = _statusTypeService.NewStatusTypePropertiesForUpdate(parentId);
            var model = StatusTypeViewModel.Create(status, tabId, parentId);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data = _statusTypeService.SaveProperties(model.Data);
                PersistResultId(model.Data.Id);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveStatusType });
            }

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveStatusType)]
        [BackendActionContext(ActionCode.RemoveStatusType)]
        [BackendActionLog]
        public ActionResult Remove(int id)
        {
            return Json(_statusTypeService.Remove(id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.MultipleSelectStatusesForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectStatusesForWorkflow)]
        public async Task<ActionResult> MultipleSelectForWorkflow(
            string tabId, int parentId, [FromBody] SelectedItemsViewModel selModel
        )
        {
            _statusTypeService.InitList(parentId);
            var model = new StatusTypeSelectableListViewModel(tabId, parentId, selModel.Ids);
            return await JsonHtml("MultiSelectIndex", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.MultipleSelectStatusesForWorkflow)]
        [BackendActionContext(ActionCode.MultipleSelectStatusesForWorkflow)]
        public ActionResult _MultipleSelectForWorkflow(
            string tabId, int parentId, [FromForm(Name="IDs")]string ids, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _statusTypeService.ListForWorkflow(listCommand, Converter.ToInt32Collection(ids, ','), parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }
    }
}

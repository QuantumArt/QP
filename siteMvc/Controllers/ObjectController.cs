using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ObjectController : QPController
    {
        private readonly IObjectService _objectService;

        public ObjectController(IObjectService objectService)
        {
            _objectService = objectService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.TemplateObjects)]
        [BackendActionContext(ActionCode.TemplateObjects)]
        public async Task<ActionResult> IndexTemplateObjects(string tabId, int parentId)
        {
            var result = _objectService.InitObjectList(parentId, true);
            var model = ObjectListViewModel.Create(result, tabId, parentId, true);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.TemplateObjects)]
        [BackendActionContext(ActionCode.TemplateObjects)]
        public ActionResult _IndexTemplateObjects(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _objectService.GetTemplateObjectsByTemplateId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageObjects)]
        [BackendActionContext(ActionCode.PageObjects)]
        public async Task<ActionResult> IndexPageObjects(string tabId, int parentId)
        {
            var result = _objectService.InitObjectList(parentId, false);
            var model = ObjectListViewModel.Create(result, tabId, parentId, false);
            return await JsonHtml("Index", model);
        }

        [HttpPost]
        [ActionAuthorize(ActionCode.PageObjects)]
        [BackendActionContext(ActionCode.PageObjects)]
        public ActionResult _IndexPageObjects(string tabId, int parentId, int page, int pageSize, string orderBy)
        {
            var listCommand = GetListCommand(page, pageSize, orderBy);
            var serviceResult = _objectService.GetPageObjectsByPageId(listCommand, parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewPageObject)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.PageObject, "parentId")]
        [BackendActionContext(ActionCode.AddNewPageObject)]
        public async Task<ActionResult> NewPageObject(string tabId, int parentId)
        {
            var obj = _objectService.NewObjectProperties(parentId, true);
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewPageObject)]
        [BackendActionContext(ActionCode.AddNewPageObject)]
        [BackendActionLog]
        public async Task<ActionResult> NewPageObject(string tabId, int parentId, IFormCollection collection)
        {
            var obj = _objectService.NewObjectPropertiesForUpdate(parentId, true);
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);

            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = _objectService.SaveObjectProperties(model.Data, model.ActiveStatusTypeIds, HttpContext.IsXmlDbUpdateReplayAction());
                PersistResultId(model.Data.Id);
                PersistDefaultFormatId(model.Data.DefaultFormatId);
                return Redirect("PageObjectProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SavePageObject });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewTemplateObject)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.TemplateObject, "parentId")]
        [BackendActionContext(ActionCode.AddNewTemplateObject)]
        public async Task<ActionResult> NewTemplateObject(string tabId, int parentId)
        {
            var obj = _objectService.NewObjectProperties(parentId, false);
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewTemplateObject)]
        [BackendActionContext(ActionCode.AddNewTemplateObject)]
        [BackendActionLog]
        public async Task<ActionResult> NewTemplateObject(string tabId, int parentId, IFormCollection collection)
        {
            var obj = _objectService.NewObjectPropertiesForUpdate(parentId, false);
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);

            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data = _objectService.SaveObjectProperties(model.Data, model.ActiveStatusTypeIds, HttpContext.IsXmlDbUpdateReplayAction());
                PersistResultId(model.Data.Id);
                PersistDefaultFormatId(model.Data.DefaultFormatId);
                return Redirect("TemplateObjectProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveTemplateObject });
            }

            return await JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.PromotePageObject)]
        [BackendActionContext(ActionCode.PromotePageObject)]
        [BackendActionLog]
        [Record]
        public ActionResult PromotePageObject(string tabId, int parentId, int id)
        {
            return Json(_objectService.PromotePageObject(id));
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.PageObjectProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObject, "id")]
        [BackendActionContext(ActionCode.PageObjectProperties)]
        public async Task<ActionResult> PageObjectProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var obj = _objectService.ReadObjectProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = obj.LockedByAnyoneElse;
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdatePageObject)]
        [BackendActionContext(ActionCode.UpdatePageObject)]
        [Record(ActionCode.PageObjectProperties)]
        [BackendActionLog]
        public async Task<ActionResult> PageObjectProperties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var obj = _objectService.ReadObjectPropertiesForUpdate(id);
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);

            await TryUpdateModelAsync(model);
            if (ModelState.IsValid)
            {
                model.Data.DefaultValues = model.Data.UseDefaultValues
                    ? JsonConvert.DeserializeObject<List<DefaultValue>>(model.AggregationListItemsDataDefaultValues)
                    : Enumerable.Empty<DefaultValue>();

                model.Data = _objectService.UpdateObjectProperties(model.Data, model.ActiveStatusTypeIds);
                return Redirect("PageObjectProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdatePageTemplate });
            }

            return await JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.TemplateObjectProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObject, "id")]
        [BackendActionContext(ActionCode.TemplateObjectProperties)]
        public async Task<ActionResult> TemplateObjectProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var obj = _objectService.ReadObjectProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = obj.LockedByAnyoneElse;
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            model.SuccesfulActionCode = successfulActionCode;
            return await JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateTemplateObject)]
        [BackendActionContext(ActionCode.UpdateTemplateObject)]
        [Record(ActionCode.TemplateObjectProperties)]
        [BackendActionLog]
        public async Task<ActionResult> TemplateObjectProperties(string tabId, int parentId, int id, IFormCollection collection)
        {
            var obj = _objectService.ReadObjectPropertiesForUpdate(id);
            var model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);

            await TryUpdateModelAsync(model);

            if (ModelState.IsValid)
            {
                model.Data.DefaultValues = model.Data.UseDefaultValues
                    ? JsonConvert.DeserializeObject<List<DefaultValue>>(model.AggregationListItemsDataDefaultValues)
                    : Enumerable.Empty<DefaultValue>();

                model.Data = _objectService.UpdateObjectProperties(model.Data, model.ActiveStatusTypeIds);
                return Redirect("TemplateObjectProperties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateTemplateObject });
            }

            return await JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemovePageObject)]
        [BackendActionContext(ActionCode.RemovePageObject)]
        [BackendActionLog]
        public ActionResult RemovePageObject(int id)
        {
            var result = _objectService.RemoveObject(id);
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveTemplateObject)]
        [BackendActionContext(ActionCode.RemoveTemplateObject)]
        [BackendActionLog]
        public ActionResult RemoveTemplateObject(int id)
        {
            var result = _objectService.RemoveObject(id);
            return JsonMessageResult(result);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemovePageObject)]
        [BackendActionContext(ActionCode.MultipleRemovePageObject)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemovePageObject(int parentId, int[] IDs)
        {
            return JsonMessageResult(_objectService.MultipleRemovePageObject(IDs));
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.MultipleRemoveTemplateObject)]
        [BackendActionContext(ActionCode.MultipleRemoveTemplateObject)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleRemoveTemplateObject(int parentId, int[] IDs)
        {
            return JsonMessageResult(_objectService.MultipleRemoveTemplateObject(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelTemplateObject)]
        [BackendActionContext(ActionCode.CancelTemplateObject)]
        public ActionResult CancelTemplateObject(int id)
        {
            _objectService.CancelObject(id);
            return JsonMessageResult(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CancelPageObject)]
        [BackendActionContext(ActionCode.CancelPageObject)]
        public ActionResult CancelPageObject(int id)
        {
            _objectService.CancelObject(id);
            return JsonMessageResult(null);
        }

        [HttpPost]
        public ActionResult AssemblePageObjectPreAction(int id)
        {
            return Json(_objectService.AssembleObjectPreAction(id));
        }

        [HttpPost]
        public ActionResult AssembleTemplateObjectPreAction(int id)
        {
            return Json(_objectService.AssembleObjectPreAction(id));
        }

        [HttpPost]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssemblePageObjectPreAction(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObjectPreAction(IDs));
        }

        [HttpPost]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssembleTemplateObjectPreAction(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObjectPreAction(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssemblePageObject)]
        [BackendActionContext(ActionCode.AssemblePageObject)]
        [BackendActionLog]
        public ActionResult AssemblePageObject(int id)
        {
            return Json(_objectService.AssembleObject(id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.AssembleTemplateObject)]
        [BackendActionContext(ActionCode.AssembleTemplateObject)]
        [BackendActionLog]
        public ActionResult AssembleTemplateObject(int id)
        {
            return Json(_objectService.AssembleObject(id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.MultipleAssembleTemplateObject)]
        [BackendActionContext(ActionCode.MultipleAssembleTemplateObject)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssembleTemplateObject(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObject(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScope]
        [ActionAuthorize(ActionCode.MultipleAssemblePageObject)]
        [BackendActionContext(ActionCode.MultipleAssemblePageObject)]
        [BackendActionLog]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult MultipleAssemblePageObject(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObject(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockPageObject)]
        [BackendActionContext(ActionCode.CaptureLockPageObject)]
        [BackendActionLog]
        public ActionResult CaptureLockPageObject(int id)
        {
            _objectService.CaptureLockPageObject(id);
            return Json(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.CaptureLockTemplateObject)]
        [BackendActionContext(ActionCode.CaptureLockTemplateObject)]
        [BackendActionLog]
        public ActionResult CaptureLockTemplateObject(int id)
        {
            _objectService.CaptureLockTemplateObject(id);
            return Json(null);
        }
    }
}

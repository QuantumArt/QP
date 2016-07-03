using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.PageTemplate;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    [ValidateInput(false)]
    public class ObjectController : QPController
    {
        IObjectService _objectService;

        public ObjectController(IObjectService objectService)
        {
            this._objectService = objectService;
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.TemplateObjects)]
        [BackendActionContext(ActionCode.TemplateObjects)]
        public ActionResult IndexTemplateObjects(string tabId, int parentId)
        {
            ObjectInitListResult result = _objectService.InitObjectList(parentId, true);
            ObjectListViewModel model = ObjectListViewModel.Create(result, tabId, parentId, true);
            return this.JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.TemplateObjects)]
        [BackendActionContext(ActionCode.TemplateObjects)]
        public ActionResult _IndexTemplateObjects(string tabId, int parentId, GridCommand command)
        {
            ListResult<ObjectListItem> serviceResult = _objectService.GetTemplateObjectsByTemplateId(command.GetListCommand(), parentId);
            return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.PageObjects)]
        [BackendActionContext(ActionCode.PageObjects)]
        public ActionResult IndexPageObjects(string tabId, int parentId)
        {
            ObjectInitListResult result = _objectService.InitObjectList(parentId, false);
            ObjectListViewModel model = ObjectListViewModel.Create(result, tabId, parentId, false);
            return this.JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.PageObjects)]
        [BackendActionContext(ActionCode.PageObjects)]
        public ActionResult _IndexPageObjects(string tabId, int parentId, GridCommand command)
        {
            ListResult<ObjectListItem> serviceResult = _objectService.GetPageObjectsByPageId(command.GetListCommand(), parentId);
            return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.AddNewPageObject)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.PageObject, "parentId")]
        [BackendActionContext(ActionCode.AddNewPageObject)]
        public ActionResult NewPageObject(string tabId, int parentId)
        {
            BllObject obj = _objectService.NewObjectProperties(parentId, true);
            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.AddNewPageObject)]
        [BackendActionContext(ActionCode.AddNewPageObject)]
        [BackendActionLog]
        [Record]
        public ActionResult NewPageObject(string tabId, int parentId, FormCollection collection)
        {
            BllObject obj = _objectService.NewObjectPropertiesForUpdate(parentId, true);
            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _objectService.SaveObjectProperties(model.Data, model.ActiveStatusTypeIds, IsReplayAction());
                this.PersistResultId(model.Data.Id);
                this.PersistDefaultFormatId(model.Data.DefaultFormatId);
                return Redirect("PageObjectProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SavePageObject });
            }
            else
                return this.JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.AddNewTemplateObject)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.TemplateObject, "parentId")]
        [BackendActionContext(ActionCode.AddNewTemplateObject)]
        public ActionResult NewTemplateObject(string tabId, int parentId)
        {
            BllObject obj = _objectService.NewObjectProperties(parentId, false);
            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.AddNewTemplateObject)]
        [BackendActionContext(ActionCode.AddNewTemplateObject)]
        [BackendActionLog]
        [Record]
        public ActionResult NewTemplateObject(string tabId, int parentId, FormCollection collection)
        {
            BllObject obj = _objectService.NewObjectPropertiesForUpdate(parentId, false);
            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                model.Data = _objectService.SaveObjectProperties(model.Data, model.ActiveStatusTypeIds, IsReplayAction());
                this.PersistResultId(model.Data.Id);
                this.PersistDefaultFormatId(model.Data.DefaultFormatId);
                return Redirect("TemplateObjectProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveTemplateObject });
            }
            else
                return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.PromotePageObject)]
        [BackendActionContext(ActionCode.PromotePageObject)]
        [BackendActionLog]
        [Record]
        public ActionResult PromotePageObject(string tabId, int parentId, int id)
        {
            return Json(_objectService.PromotePageObject(id));
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.PageObjectProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.PageObject, "id")]
        [BackendActionContext(ActionCode.PageObjectProperties)]
        public ActionResult PageObjectProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            BllObject obj = _objectService.ReadObjectProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = obj.LockedByAnyoneElse;
            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            model.SuccesfulActionCode = successfulActionCode;
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.UpdatePageObject)]
        [BackendActionContext(ActionCode.UpdatePageObject)]
        [Record(ActionCode.PageObjectProperties)]
        [BackendActionLog]
        public ActionResult PageObjectProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            BllObject obj = _objectService.ReadObjectPropertiesForUpdate(id);

            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                if (model.Data.UseDefaultValues)
                {
                    model.Data.DefaultValues = new JavaScriptSerializer().Deserialize<List<DefaultValue>>(model.AggregationListItems_Data_DefaultValues);
                }
                else
                {
                    model.Data.DefaultValues = Enumerable.Empty<DefaultValue>();
                }
                model.Data = _objectService.UpdateObjectProperties(model.Data, model.ActiveStatusTypeIds);
                return Redirect("PageObjectProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdatePageTemplate });
            }
            else
                return this.JsonHtml("Properties", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ActionAuthorize(ActionCode.TemplateObjectProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.TemplateObject, "id")]
        [BackendActionContext(ActionCode.TemplateObjectProperties)]
        public ActionResult TemplateObjectProperties(string tabId, int parentId, int id, string successfulActionCode)
        {
            BllObject obj = _objectService.ReadObjectProperties(id);
            ViewData[SpecialKeys.IsEntityReadOnly] = obj.LockedByAnyoneElse;
            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            model.SuccesfulActionCode = successfulActionCode;
            return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UIAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.UpdateTemplateObject)]
        [BackendActionContext(ActionCode.UpdateTemplateObject)]
        [Record(ActionCode.TemplateObjectProperties)]
        [BackendActionLog]
        public ActionResult TemplateObjectProperties(string tabId, int parentId, int id, FormCollection collection)
        {
            BllObject obj = _objectService.ReadObjectPropertiesForUpdate(id);
            ObjectViewModel model = ObjectViewModel.Create(obj, tabId, parentId, _objectService);
            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                if (model.Data.UseDefaultValues)
                {
                    model.Data.DefaultValues = new JavaScriptSerializer().Deserialize<List<DefaultValue>>(model.AggregationListItems_Data_DefaultValues);
                }

                else
                {
                    model.Data.DefaultValues = Enumerable.Empty<DefaultValue>();
                }
                model.Data = _objectService.UpdateObjectProperties(model.Data, model.ActiveStatusTypeIds);
                return Redirect("TemplateObjectProperties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateTemplateObject });
            }
            else
                return this.JsonHtml("Properties", model);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.RemovePageObject)]
        [BackendActionContext(ActionCode.RemovePageObject)]
        [BackendActionLog]
        [Record]
        public ActionResult RemovePageObject(int id)
        {
            MessageResult result = _objectService.RemoveObject(id);
            return this.JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.RemoveTemplateObject)]
        [BackendActionContext(ActionCode.RemoveTemplateObject)]
        [BackendActionLog]
        [Record]
        public ActionResult RemoveTemplateObject(int id)
        {
            MessageResult result = _objectService.RemoveObject(id);
            return this.JsonMessageResult(result);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.MultipleRemovePageObject)]
        [BackendActionContext(ActionCode.MultipleRemovePageObject)]
        [BackendActionLog]
        [Record]
        public ActionResult MultipleRemovePageObject(int parentId, int[] IDs)
        {
            return this.JsonMessageResult(_objectService.MultipleRemovePageObject(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.MultipleRemoveTemplateObject)]
        [BackendActionContext(ActionCode.MultipleRemoveTemplateObject)]
        [BackendActionLog]
        [Record]
        public ActionResult MultipleRemoveTemplateObject(int parentId, int[] IDs)
        {
            return this.JsonMessageResult(_objectService.MultipleRemoveTemplateObject(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.CancelTemplateObject)]
        [BackendActionContext(ActionCode.CancelTemplateObject)]
        public ActionResult CancelTemplateObject(int id)
        {
            _objectService.CancelObject(id);
            return this.JsonMessageResult(null);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
        [ActionAuthorize(ActionCode.CancelPageObject)]
        [BackendActionContext(ActionCode.CancelPageObject)]
        public ActionResult CancelPageObject(int id)
        {
            _objectService.CancelObject(id);
            return this.JsonMessageResult(null);
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
        public ActionResult MultipleAssemblePageObjectPreAction(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObjectPreAction(IDs));
        }

        [HttpPost]
        public ActionResult MultipleAssembleTemplateObjectPreAction(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObjectPreAction(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScopeAttribute]
        [ActionAuthorize(ActionCode.AssemblePageObject)]
        [BackendActionContext(ActionCode.AssemblePageObject)]
        [BackendActionLog]
        public ActionResult AssemblePageObject(int id)
        {
            return Json(_objectService.AssembleObject(id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScopeAttribute]
        [ActionAuthorize(ActionCode.AssembleTemplateObject)]
        [BackendActionContext(ActionCode.AssembleTemplateObject)]
        [BackendActionLog]
        public ActionResult AssembleTemplateObject(int id)
        {
            return Json(_objectService.AssembleObject(id));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScopeAttribute]
        [ActionAuthorize(ActionCode.MultipleAssembleTemplateObject)]
        [BackendActionContext(ActionCode.MultipleAssembleTemplateObject)]
        [BackendActionLog]
        public ActionResult MultipleAssembleTemplateObject(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObject(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [NoTransactionConnectionScopeAttribute]
        [ActionAuthorize(ActionCode.MultipleAssemblePageObject)]
        [BackendActionContext(ActionCode.MultipleAssemblePageObject)]
        [BackendActionLog]
        public ActionResult MultipleAssemblePageObject(int[] IDs)
        {
            return Json(_objectService.MultipleAssembleObject(IDs));
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
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
        [ConnectionScope(ConnectionScopeMode.TransactionOn)]
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

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.ViewModels;
using Telerik.Web.Mvc;
using Quantumart.QP8.WebMvc.ViewModels.Field;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class FieldController : QPController
    {

		#region list actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.Fields)]
		public ActionResult Index(string tabId, int parentId)
		{
			FieldInitListResult result = FieldService.InitList(parentId);
			FieldListViewModel model = FieldListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.Fields)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<FieldListItem> serviceResult = FieldService.List(parentId, command.GetListCommand());
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		#endregion
		
		#region form actions

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewField)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Content, "parentId")]
		[BackendActionContext(ActionCode.AddNewField)]
		public ActionResult New(string tabId, int parentId, int? fieldId)
		{
			Field field = FieldService.New(parentId, fieldId);
			FieldViewModel model = FieldViewModel.Create(field, tabId, parentId);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewField)]
		[BackendActionContext(ActionCode.AddNewField)]
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, string backendActionCode, FormCollection collection)
		{
			Field content = FieldService.NewForSave(parentId);
			FieldViewModel model = FieldViewModel.Create(content, tabId, parentId);
			int? oldLinkId = model.Data.LinkId;
			Field oldBackward = model.Data.BackwardField;
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = FieldService.Save(model.Data);
					this.PersistResultId(model.Data.Id);
					this.PersistLinkId(oldLinkId, model.Data.LinkId);
					this.PersistBackwardId(oldBackward, model.Data.BackwardField);
					this.PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
					this.PersistChildFieldIds(model.Data.ResultChildFieldIds);
					this.PersistChildLinkIds(model.Data.ResultChildLinkIds);
				}
				catch (VirtualContentProcessingException vcpe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
					return this.JsonHtml("Properties", model);
				}
				return this.Redirect("Properties", new { 
					tabId = tabId, 
					parentId = parentId, 
					id = model.Data.Id, 
					successfulActionCode = backendActionCode,
					viewInListAffected = model.Data.ViewInList
				});
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.FieldProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Field, "id")]
		[BackendActionContext(ActionCode.FieldProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode, bool? orderChanged, bool? viewInListAffected)
		{
			Field content = FieldService.Read(id);
			FieldViewModel model = FieldViewModel.Create(content, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			model.OrderChanged = orderChanged ?? false;
			model.ViewInListAffected = viewInListAffected ?? false;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateField)]
		[BackendActionContext(ActionCode.UpdateField)]
		[BackendActionLog]
		[Record(ActionCode.FieldProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string backendActionCode, FormCollection collection)
		{
			Field field = FieldService.ReadForUpdate(id);
			FieldViewModel model = FieldViewModel.Create(field, tabId, parentId);
            int oldOrder = model.Data.Order;
			bool oldViewInList = model.Data.ViewInList;
			int? oldLinkId = model.Data.LinkId;
			Field oldBackward = model.Data.BackwardField;
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = FieldService.Update(model.Data);
					this.PersistLinkId(oldLinkId, model.Data.LinkId);
					this.PersistBackwardId(oldBackward, model.Data.BackwardField);
					this.PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
				}
				catch (UserQueryContentCreateViewException uqe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
					return this.JsonHtml("Properties", model);
				}
				catch (VirtualContentProcessingException vcpe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
					return this.JsonHtml("Properties", model);
				}
                int newOrder = model.Data.Order;
				bool newViewInList = model.Data.ViewInList;
				return this.Redirect("Properties", new { 
					tabId = tabId, 
					parentId = parentId, 
					id = model.Data.Id, 
					successfulActionCode = backendActionCode, 
					orderChanged = (newOrder != oldOrder),
					viewInListAffected = (newViewInList != oldViewInList)
				});
			}
			else
				return this.JsonHtml("Properties", model);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.VirtualFieldProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.VirtualField, "id")]
		[BackendActionContext(ActionCode.VirtualFieldProperties)]
		public ActionResult VirtualProperties(string tabId, int parentId, int id, string successfulActionCode, bool? orderChanged, bool? viewInListAffected)
		{
			Field content = FieldService.VirtualRead(id);
			FieldViewModel model = FieldViewModel.Create(content, tabId, parentId);
			model.SuccesfulActionCode = successfulActionCode;
			model.OrderChanged = orderChanged ?? false;
			model.ViewInListAffected = viewInListAffected ?? false;
			return this.JsonHtml("VirtualProperties", model);
		}

		[HttpPost]
		[ValidateInput(false)]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateVirtualField)]
		[BackendActionContext(ActionCode.UpdateVirtualField)]
		[BackendActionLog]
		[Record(ActionCode.VirtualFieldProperties)]
		public ActionResult VirtualProperties(string tabId, int parentId, int id, FormCollection collection)
		{
			Field field = FieldService.ReadForUpdate(id);
			FieldViewModel model = FieldViewModel.Create(field, tabId, parentId);
            int oldOrder = model.Data.Order;
			bool oldViewInList = model.Data.ViewInList;
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				try
				{
					model.Data = VirtualFieldService.Update(model.Data);
					this.PersistVirtualFieldIds(model.Data.NewVirtualFieldIds);
				}
				catch (UserQueryContentCreateViewException uqe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("UserQueryContentCreateViewException", uqe.Message);
					return JsonHtml("VirtualProperties", model);
				}
				catch (VirtualContentProcessingException vcpe)
				{
					if (IsReplayAction())
						throw;
					ModelState.AddModelError("VirtualContentProcessingException", vcpe.Message);
					return JsonHtml("Properties", model);
				}
				bool newViewInList = model.Data.ViewInList;
                return Redirect("VirtualProperties", new { 
					tabId = tabId, 
					parentId = parentId, 
					id = model.Data.Id, 
					successfulActionCode = Constants.ActionCode.UpdateField, 
					orderChanged = (oldOrder != model.Data.Order),
					viewInListAffected = (newViewInList != oldViewInList)
				});
			}
			else
				return JsonHtml("VirtualProperties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveField)]
		[BackendActionContext(ActionCode.RemoveField)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = FieldService.Remove(id);
			return this.JsonMessageResult(result);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.MultipleRemoveField)]
		[BackendActionContext(ActionCode.MultipleRemoveField)]
		[BackendActionLog]
		[Record]
		public ActionResult MultipleRemove(int[] IDs)
		{
			MessageResult result = FieldService.MultipleRemove(IDs);
			return this.JsonMessageResult(result);
		}

		#endregion

		#region select actions

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectFieldForExport)]
		[BackendActionContext(ActionCode.MultipleSelectFieldForExport)]
		public ActionResult MultipleSelectForExport(string tabId, int parentId, int[] IDs)
		{
			FieldInitListResult result = FieldService.InitList(parentId);


			FieldSelectableListViewModel model = new FieldSelectableListViewModel(result, tabId, parentId, IDs, ActionCode.MultipleSelectFieldForExport);
			
            model.IsMultiple = true;
			return this.JsonHtml("MultiSelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectFieldForExport)]
		[BackendActionContext(ActionCode.MultipleSelectFieldForExport)]
		public ActionResult _MultipleSelectForExport(string tabId, int parentId, string IDs, GridCommand command)
		{
			ListResult<FieldListItem> serviceResult = FieldService.ListForExport(command.GetListCommand(), parentId, Converter.ToInt32Collection(IDs, ','));
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.MultipleSelectFieldForExportExpanded)]
		[BackendActionContext(ActionCode.MultipleSelectFieldForExportExpanded)]
		public ActionResult MultipleSelectForExportExpanded(string tabId, int parentId, int[] IDs)
		{
			FieldInitListResult result = FieldService.InitList(parentId);
			FieldSelectableListViewModel model = new FieldSelectableListViewModel(result, tabId, parentId, IDs, ActionCode.MultipleSelectFieldForExportExpanded);
			model.IsMultiple = true;
			return this.JsonHtml("MultiSelectIndex", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.MultipleSelectFieldForExportExpanded)]
		[BackendActionContext(ActionCode.MultipleSelectFieldForExportExpanded)]
		public ActionResult _MultipleSelectForExportExpanded(string tabId, int parentId, string IDs, GridCommand command)
		{
            ListResult<FieldListItem> serviceResult = FieldService.ListForExportExpanded(command.GetListCommand(), parentId, Converter.ToInt32Collection(IDs, ','));
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}

		#endregion

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.CreateLikeField)]
		[BackendActionContext(ActionCode.CreateLikeField)]
		[BackendActionLog]
		[Record]
		public ActionResult Copy(int id, int? forceId, int? forceLinkId, string forceVirtualFieldIds, string forceChildFieldIds, string forceChildLinkIds)
		{
			var fieldToCopy = FieldService.Read(id);
			FieldCopyResult result = FieldService.Copy(id, forceId, forceLinkId,
				RecordReplayHelper.ToIntArray(forceVirtualFieldIds),
				RecordReplayHelper.ToIntArray(forceChildFieldIds),
				RecordReplayHelper.ToIntArray(forceChildLinkIds)
			);
			this.PersistResultId(result.Id);
			this.PersistFromId(id);
			this.PersistLinkId(null, result.LinkId);
			this.PersistVirtualFieldIds(result.VirtualFieldIds);
			this.PersistChildFieldIds(result.ChildFieldIds);
			this.PersistChildLinkIds(result.ChildLinkIds);
			return this.JsonMessageResult(result.Message);
		}
    }
}

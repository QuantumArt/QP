using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Linq.Expressions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Extensions.ModelBinders;
using Quantumart.QP8.WebMvc.ViewModels.VirtualContent;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;
using Telerik.Web.Mvc;
using System;
using Quantumart.QP8.Resources;
using System.Text;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class WorkflowController : QPController
    {
		IWorkflowService _workflowService;

		public WorkflowController(IWorkflowService workflowService)
		{
			this._workflowService = workflowService;
		}

		#region	list actions
		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.Workflows)]
		[BackendActionContext(ActionCode.Workflows)]
		public ActionResult Index(string tabId, int parentId)
		{
			WorkflowInitListResult result = _workflowService.InitList(parentId);
			WorkflowListViewModel model = WorkflowListViewModel.Create(result, tabId, parentId);
			return this.JsonHtml("Index", model);
		}

		[HttpPost]
		[GridAction(EnableCustomBinding = true)]
		[ActionAuthorize(ActionCode.Workflows)]
		[BackendActionContext(ActionCode.Workflows)]
		public ActionResult _Index(string tabId, int parentId, GridCommand command)
		{
			ListResult<WorkflowListItem> serviceResult = _workflowService.GetWorkflowsBySiteId(command.GetListCommand(), parentId);
			return View(new GridModel() { Data = serviceResult.Data, Total = serviceResult.TotalRecords });
		}
		#endregion

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.WorkflowProperties)]
		[EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Workflow, "id")]
		[BackendActionContext(ActionCode.WorkflowProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
		{
			Workflow workflow = _workflowService.ReadProperties(id);			
			WorkflowViewModel model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);
			model.SuccesfulActionCode = successfulActionCode;
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.UpdateWorkflow)]
		[BackendActionContext(ActionCode.UpdateWorkflow)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Workflow, "id")]
		[BackendActionLog]
		[Record(ActionCode.WorkflowProperties)]
		public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
		{
			Workflow workflow = _workflowService.ReadPropertiesForUpdate(id);
			WorkflowViewModel model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);			
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				int[] oldIds = model.Data.WorkflowRules.Select(n => n.Id).ToArray();
				model.Data = _workflowService.UpdateProperties(model.Data, model.ActiveContentsIds);
				int[] newIds = model.Data.WorkflowRules.Select(n => n.Id).ToArray();
				this.PersistRulesIds(oldIds, newIds);
				
				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.UpdateWorkflow });
			}
			else
			{				
				return JsonHtml("Properties", model);
			}
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ActionAuthorize(ActionCode.AddNewWorkflow)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewWorkflow)]
		public ActionResult New(string tabId, int parentId)
		{
			Workflow workflow = _workflowService.NewWorkflowProperties(parentId);
			WorkflowViewModel model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);
			return this.JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.AddNewWorkflow)]
		[EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
		[BackendActionContext(ActionCode.AddNewWorkflow)]		
		[BackendActionLog]
		[Record]
		public ActionResult New(string tabId, int parentId, FormCollection collection)
		{
			Workflow workflow = _workflowService.NewWorkflowPropertiesForUpdate(parentId);
			WorkflowViewModel model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);
			TryUpdateModel(model);
			model.Validate(ModelState);
			if (ModelState.IsValid)
			{
				model.Data = _workflowService.SaveWorkflowProperties(model.Data);
				this.PersistResultId(model.Data.Id);
				this.PersistRulesIds(null, model.Data.WorkflowRules.Select(n => n.Id).ToArray());

				return Redirect("Properties", new { tabId = tabId, parentId = parentId, id = model.Data.Id, successfulActionCode = Constants.ActionCode.SaveWorkflow });
			}
			else
				return JsonHtml("Properties", model);
		}

		[HttpPost]
		[ExceptionResult(ExceptionResultMode.OperationAction)]
		[ConnectionScope(ConnectionScopeMode.TransactionOn)]
		[ActionAuthorize(ActionCode.RemoveWorkflow)]
		[BackendActionContext(ActionCode.RemoveWorkflow)]
		[BackendActionLog]
		[Record]
		public ActionResult Remove(int id)
		{
			MessageResult result = _workflowService.Remove(id);
			return JsonMessageResult(result);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		public ActionResult CheckUserOrGroupAccessOnContents(string statusName, string userIdString, string groupIdString, string contentIdsString)
		{
			var contentIds = string.IsNullOrEmpty(contentIdsString) ? new List<int>() : contentIdsString.Split(new char[] { ',' }).Select(x => int.Parse(x)).ToList();
			int tempVal;
			int? userId = Int32.TryParse(userIdString, out tempVal) ? tempVal : (int?)null;
			int? groupId = Int32.TryParse(groupIdString, out tempVal) ? tempVal : (int?)null;


			var contentAccessSummary = new StringBuilder();
			
			foreach (var contentId in contentIds)
			{
				if (userId != null && !_workflowService.IsContentAccessibleForUser(contentId, userId.Value))
				{
					contentAccessSummary.AppendFormatLine(WorkflowStrings.InAccessibleForUser + "<br>", _workflowService.getContentNameById(contentId));
				}
				else if (groupId != null && !_workflowService.IsContentAccessibleForUserGroup(contentId, groupId.Value))
				{
					contentAccessSummary.AppendFormatLine(WorkflowStrings.InAccessibleForGroup + "<br>", _workflowService.getContentNameById(contentId));
				}
			}
			
			return this.Json(contentAccessSummary.ToString(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[ExceptionResult(ExceptionResultMode.UIAction)]
		public ActionResult CheckAllAccessOnContents(string modelString, string contentIdsString)
		{
			var contentAccessSummary = new List<object>();
			var contentIds = string.IsNullOrEmpty(contentIdsString) ? new List<int>() : contentIdsString.Split(new char[] { ',' }).Select(x => int.Parse(x)).ToList();
			var model = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<List<WorkflowRuleSimpleItem>>(modelString);

			foreach (var wfStage in model)
			{
				foreach (int contentId in contentIds)
				{
					if (wfStage.UserId.HasValue && !_workflowService.IsContentAccessibleForUser(contentId, wfStage.UserId.Value))
						contentAccessSummary.Add(new { StName = wfStage.StName, Message = String.Format(WorkflowStrings.InAccessibleForUser, _workflowService.getContentNameById(contentId)) });

					else if(wfStage.GroupId.HasValue && !_workflowService.IsContentAccessibleForUserGroup(contentId, wfStage.GroupId.Value))
						contentAccessSummary.Add(new { StName = wfStage.StName, Message = String.Format(WorkflowStrings.InAccessibleForGroup, _workflowService.getContentNameById(contentId)) });
				}
			}

			return this.Json(contentAccessSummary, JsonRequestBehavior.AllowGet);
		}
    }
}

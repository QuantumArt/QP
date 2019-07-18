using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.ActionResults;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.Workflow;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class WorkflowController : QPController
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.Workflows)]
        [BackendActionContext(ActionCode.Workflows)]
        public ActionResult Index(string tabId, int parentId)
        {
            var result = _workflowService.InitList(parentId);
            var model = WorkflowListViewModel.Create(result, tabId, parentId);
            return JsonHtml("Index", model);
        }

        [HttpPost]
        [GridAction(EnableCustomBinding = true)]
        [ActionAuthorize(ActionCode.Workflows)]
        [BackendActionContext(ActionCode.Workflows)]
        public ActionResult _Index(string tabId, int parentId, GridCommand command)
        {
            var serviceResult = _workflowService.GetWorkflowsBySiteId(command.GetListCommand(), parentId);
            return new TelerikResult(serviceResult.Data, serviceResult.TotalRecords);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.WorkflowProperties)]
        [EntityAuthorize(ActionTypeCode.Read, EntityTypeCode.Workflow, "id")]
        [BackendActionContext(ActionCode.WorkflowProperties)]
        public ActionResult Properties(string tabId, int parentId, int id, string successfulActionCode)
        {
            var workflow = _workflowService.ReadProperties(id);
            var model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);
            model.SuccesfulActionCode = successfulActionCode;
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record(ActionCode.WorkflowProperties)]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.UpdateWorkflow)]
        [BackendActionContext(ActionCode.UpdateWorkflow)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Workflow, "id")]
        [BackendActionLog]
        public ActionResult Properties(string tabId, int parentId, int id, FormCollection collection)
        {
            var workflow = _workflowService.ReadPropertiesForUpdate(id);
            var model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);

            TryUpdateModel(model);
            model.Validate(ModelState);
            if (ModelState.IsValid)
            {
                var oldIds = model.Data.WorkflowRules.Select(n => n.Id).ToArray();
                var activeStatusesIds = model.ActiveStatuses.Select(n => int.Parse(n.Value)).ToArray();
                model.Data = _workflowService.UpdateProperties(model.Data, model.ActiveContentsIds, activeStatusesIds);
                var newIds = model.Data.WorkflowRules.Select(n => n.Id).ToArray();
                PersistRulesIds(oldIds, newIds);
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.UpdateWorkflow });
            }

            return JsonHtml("Properties", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ActionAuthorize(ActionCode.AddNewWorkflow)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewWorkflow)]
        public ActionResult New(string tabId, int parentId)
        {
            var workflow = _workflowService.NewWorkflowProperties(parentId);
            var model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);
            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.AddNewWorkflow)]
        [EntityAuthorize(ActionTypeCode.Update, EntityTypeCode.Site, "parentId")]
        [BackendActionContext(ActionCode.AddNewWorkflow)]
        [BackendActionLog]
        public ActionResult New(string tabId, int parentId, FormCollection collection)
        {
            var workflow = _workflowService.NewWorkflowPropertiesForUpdate(parentId);
            var model = WorkflowViewModel.Create(workflow, tabId, parentId, _workflowService);

            TryUpdateModel(model);
            model.Validate(ModelState);

            if (ModelState.IsValid)
            {
                model.Data = _workflowService.SaveWorkflowProperties(model.Data);
                PersistResultId(model.Data.Id);
                PersistRulesIds(null, model.Data.WorkflowRules.Select(n => n.Id).ToArray());
                return Redirect("Properties", new { tabId, parentId, id = model.Data.Id, successfulActionCode = ActionCode.SaveWorkflow });
            }

            return JsonHtml("Properties", model);
        }

        [HttpPost, Record]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ConnectionScope]
        [ActionAuthorize(ActionCode.RemoveWorkflow)]
        [BackendActionContext(ActionCode.RemoveWorkflow)]
        [BackendActionLog]
        public ActionResult Remove(int id) => JsonMessageResult(_workflowService.Remove(id));

        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult CheckUserOrGroupAccessOnContents(string statusName, string userIdString, string groupIdString, string contentIdsString)
        {
            var contentIds = string.IsNullOrEmpty(contentIdsString) ? new List<int>() : contentIdsString.Split(',').Select(x => int.Parse(x)).ToList();
            var userId = int.TryParse(userIdString, out var tempVal) ? tempVal : (int?)null;
            var groupId = int.TryParse(groupIdString, out tempVal) ? tempVal : (int?)null;
            var contentAccessSummary = new StringBuilder();

            foreach (var contentId in contentIds)
            {
                if (userId != null && !_workflowService.IsContentAccessibleForUser(contentId, userId.Value))
                {
                    contentAccessSummary.AppendFormatLine(WorkflowStrings.InAccessibleForUser + "<br>", _workflowService.GetContentNameById(contentId));
                }
                else if (groupId != null && !_workflowService.IsContentAccessibleForUserGroup(contentId, groupId.Value))
                {
                    contentAccessSummary.AppendFormatLine(WorkflowStrings.InAccessibleForGroup + "<br>", _workflowService.GetContentNameById(contentId));
                }
            }

            return Json(contentAccessSummary.ToString(), JsonRequestBehavior.AllowGet);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult CheckAllAccessOnContents(string modelString, string contentIdsString)
        {
            var contentAccessSummary = new List<object>();
            var contentIds = string.IsNullOrEmpty(contentIdsString) ? new List<int>() : contentIdsString.Split(',').Select(int.Parse).ToList();
            var model = JsonConvert.DeserializeObject<List<WorkflowRuleSimpleItem>>(modelString);

            foreach (var stage in model)
            {
                foreach (var contentId in contentIds)
                {
                    if (stage.UserId.HasValue && !_workflowService.IsContentAccessibleForUser(contentId, stage.UserId.Value))
                    {
                        contentAccessSummary.Add(new { stage.StName, Message = string.Format(WorkflowStrings.InAccessibleForUser, _workflowService.GetContentNameById(contentId)) });
                    }
                    else if (stage.GroupId.HasValue && !_workflowService.IsContentAccessibleForUserGroup(contentId, stage.GroupId.Value))
                    {
                        contentAccessSummary.Add(new { stage.StName, Message = string.Format(WorkflowStrings.InAccessibleForGroup, _workflowService.GetContentNameById(contentId)) });
                    }
                }
            }

            return Json(contentAccessSummary, JsonRequestBehavior.AllowGet);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Workflow
{
    public class WorkflowViewModel : EntityViewModel
    {
        private IWorkflowService _service;

        public override string EntityTypeCode => Constants.EntityTypeCode.Workflow;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewWorkflow : Constants.ActionCode.WorkflowProperties;

        public new BLL.Workflow Data
        {
            get
            {
                return (BLL.Workflow)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        [LocalizedDisplayName("Contents", NameResourceType = typeof(WorkflowStrings))]
        public IEnumerable<int> ActiveContentsIds { get; set; }

        [LocalizedDisplayName("Contents", NameResourceType = typeof(WorkflowStrings))]
        public IEnumerable<ListItem> ActiveContentsListItems { get; set; }

        public static WorkflowViewModel Create(BLL.Workflow workflow, string tabId, int parentId, IWorkflowService service)
        {
            var model = Create<WorkflowViewModel>(workflow, tabId, parentId);
            model._service = service;
            return model.Init();
        }

        public EntityDataListArgs EntityDataListArgs { get; set; }

        public override void Validate(ModelStateDictionary modelState)
        {
            modelState.Clear();
            if (ActiveStatuses.Count == 0)
            {
                modelState.AddModelError("ActiveStatuses", WorkflowStrings.StatusNotSelected);
            }

            base.Validate(modelState);
        }

        private InitPropertyValue<IEnumerable<BLL.StatusType>> _statuses;

        private IEnumerable<BLL.StatusType> Statuses
        {
            get { return _statuses.Value.OrderBy(x => x.Weight); }
        }

        public string StatusWeightsDictionary
        {
            get { return new JavaScriptSerializer().Serialize(Statuses.ToDictionary(x => x.Id.ToString(), x => x.Weight)); }
        }

        public IEnumerable<ListItem> AllStatusListItems
        {
            get
            {
                return Statuses.Where(s => s.Weight > 0).Select(s => new ListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToArray();
            }
        }

        public string WorkflowsWorkflowRulesDisplay { get; set; }

        [LocalizedDisplayName("Statuses", NameResourceType = typeof(WorkflowStrings))]
        public IList<QPCheckedItem> ActiveStatuses { get; set; }

        private InitPropertyValue<IEnumerable<BLL.Content>> _contents;

        public IEnumerable<BLL.Content> Contents
        {
            get { return _contents.Value; }
            set { _contents.Value = value; }
        }

        public IEnumerable<object> WorkflowRulesDisplay
        {
            get
            {
                return Data.WorkflowRules.OrderBy(x => x.StatusType.Weight).Select(x => new
                {
                    x.Id,
                    StName = x.StatusType != null ? x.StatusType.Name : string.Empty,
                    StId = x.SuccessorStatusId,
                    RadioChecked = x.UserId.HasValue ? "User" : "Group",
                    x.UserId,
                    x.GroupId,
                    x.Description,
                    x.StatusType.Weight,
                    Invalid = x.IsInvalid
                });
            }
        }

        [LocalizedDisplayName("User", NameResourceType = typeof(WorkflowStrings))]
        public string O2MDefaultValue { get; set; }

        public QPSelectListItem DefaultValueListItem => O2MDefaultValue != null ? new QPSelectListItem { Value = "0", Text = @"Select User", Selected = true } : null;

        private WorkflowViewModel Init()
        {
            _statuses = new InitPropertyValue<IEnumerable<BLL.StatusType>>(() => _service.GetStatusTypesBySiteId(Data.SiteId));
            _contents = new InitPropertyValue<IEnumerable<BLL.Content>>(() => _service.GetContentsBySiteId(Data.SiteId));

            ActiveContentsIds = Data.Id == 0 ? new List<int>() : _service.GetBindedContetnsIds(Data.Id);
            ActiveContentsListItems = Contents.Where(x => ActiveContentsIds.Contains(x.Id)).Select(x => new ListItem { Value = x.Id.ToString(), Selected = true, Text = x.Name });
            ActiveStatuses = Data.WorkflowRules?.Select(x => new QPCheckedItem { Value = x.StatusType.Id.ToString() }).ToList() ?? new List<QPCheckedItem>();

            EntityDataListArgs = new EntityDataListArgs
            {
                EntityTypeCode = Constants.EntityTypeCode.Content,
                ParentEntityId = Data.SiteId,
                SelectActionCode = Constants.ActionCode.MultipleSelectContentForWorkflow,
                Filter = "cwb.workflow_id is null or cwb.workflow_id = " + Data.Id,
                ListId = -1 * DateTime.Now.Millisecond,
                MaxListHeight = 200,
                MaxListWidth = 350,
                ShowIds = true
            };

            return this;
        }

        private List<WorkflowRuleItem> _workflowRules;

        internal void DoCustomBinding()
        {
            _workflowRules = new JavaScriptSerializer().Deserialize<List<WorkflowRuleItem>>(WorkflowsWorkflowRulesDisplay);
            Data.DoCustomBinding(_workflowRules);
        }
    }
}

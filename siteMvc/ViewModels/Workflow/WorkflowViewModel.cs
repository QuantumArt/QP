using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Utils.Binders;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Workflow
{
    public class WorkflowViewModel : EntityViewModel
    {
        private IWorkflowService _service;

        public override string EntityTypeCode => Constants.EntityTypeCode.Workflow;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewWorkflow : Constants.ActionCode.WorkflowProperties;

        public BLL.Workflow Data
        {
            get => (BLL.Workflow)EntityData;
            set => EntityData = value;
        }

        [Display(Name = "Contents", ResourceType = typeof(WorkflowStrings))]
        [ModelBinder(BinderType = typeof(IdArrayBinder))]
        public IEnumerable<int> ActiveContentsIds { get; set; }

        [Display(Name = "Contents", ResourceType = typeof(WorkflowStrings))]
        public IEnumerable<ListItem> ActiveContentsListItems { get; set; }

        public static WorkflowViewModel Create(BLL.Workflow workflow, string tabId, int parentId, IWorkflowService service)
        {
            var model = Create<WorkflowViewModel>(workflow, tabId, parentId);
            model._service = service;
            return model.Init();
        }

        public EntityDataListArgs EntityDataListArgs { get; set; }

        public override IEnumerable<ValidationResult> ValidateViewModel()
        {
            if (ActiveStatuses.Count == 0)
            {
                yield return new ValidationResult(WorkflowStrings.StatusNotSelected, new[] {"ActiveStatuses"});
            }
        }

        private InitPropertyValue<IEnumerable<BLL.StatusType>> _statuses;

        private IEnumerable<BLL.StatusType> Statuses
        {
            get { return _statuses.Value.OrderBy(x => x.Weight); }
        }

        public string StatusWeightsDictionary
        {
            get { return JsonConvert.SerializeObject(Statuses.ToDictionary(x => x.Id.ToString(), x => x.Weight)); }
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

        [Display(Name = "Statuses", ResourceType = typeof(WorkflowStrings))]
        public IList<QPCheckedItem> ActiveStatuses { get; set; }

        private InitPropertyValue<IEnumerable<BLL.Content>> _contents;

        [ValidateNever]
        [BindNever]
        public IEnumerable<BLL.Content> Contents
        {
            get => _contents.Value;
            set => _contents.Value = value;
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

        [Display(Name = "User", ResourceType = typeof(WorkflowStrings))]
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

        public override void DoCustomBinding()
        {
            ActiveStatuses = ActiveStatuses?.Where(n => n != null).ToArray() ?? new QPCheckedItem[] { };

            _workflowRules = JsonConvert.DeserializeObject<List<WorkflowRuleItem>>(WorkflowsWorkflowRulesDisplay);
            Data.DoCustomBinding(_workflowRules);
        }
    }
}

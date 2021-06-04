using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class Workflow : EntityObject
    {
        public int SiteId { get; set; }

        [Display(Name = "ApplyRulesByDefault", ResourceType = typeof(WorkflowStrings))]
        public bool ApplyByDefault { get; set; }

        [Display(Name = "IsDefault", ResourceType = typeof(WorkflowStrings))]
        public bool IsDefault { get; set; }

        [Display(Name = "UseDirectionControls", ResourceType = typeof(WorkflowStrings))]
        public bool UseDirectionControls { get; set; }

        [Display(Name = "CreateAutoNotifications", ResourceType = typeof(WorkflowStrings))]
        public bool CreateDefaultNotification { get; set; }

        public List<WorkflowRule> WorkflowRules { get; set; }

        internal void BindWithContent(IEnumerable<int> activeContentsIds)
        {
            WorkflowRepository.CleanWorkflowContentIds(Id);
            foreach (var contentId in activeContentsIds)
            {
                WorkflowRepository.SetWorkflowBindedContentId(Id, contentId);
            }
        }

        public void DoCustomBinding(List<WorkflowRuleItem> workflowRuleItems)
        {
            WorkflowRules = workflowRuleItems.Select(x => new WorkflowRule
            {
                Id = x.Id ?? 0,
                Description = x.Description,
                GroupId = x.RadioChecked == "Group" ? x.GroupId : null,
                UserId = x.RadioChecked == "User" ? x.UserId : null,
                SuccessorStatusId = x.StId,
                StatusType = new StatusType { Weight = x.Weight, Name = x.StName }
            }).ToList();
        }

        public override void Validate()
        {
            var errors = new RulesException<Workflow>();
            if (IsDefault)
            {
                var another = WorkflowRepository.AnotherDefaultExists(Id);
                if (another != 0)
                {
                    errors.ErrorFor(w => w.IsDefault, string.Format(WorkflowStrings.WorkflowDefaultAlreadyExists, another));
                }
            }
            WorkflowRules.ForEach(x => x.IsInvalid = false);

            base.Validate(errors);

            var workflowRulesArray = WorkflowRules.ToArray();
            foreach (var rule in workflowRulesArray)
            {
                ValidateWorkflowRule(rule, errors);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private static void ValidateWorkflowRule(WorkflowRule rule, RulesException errors)
        {
            if (rule.UserId == null && rule.GroupId == null)
            {
                errors.ErrorForModel(string.Format(WorkflowStrings.UserNotSelected, rule.StatusType.Name));
                rule.IsInvalid = true;
            }

            if (rule.Description.Length > 2000)
            {
                errors.ErrorForModel(string.Format(WorkflowStrings.CommentToLong, rule.StatusType.Name));
                rule.IsInvalid = true;
            }
        }

        public static Workflow Create(int siteId) => new Workflow { SiteId = siteId, WorkflowRules = new List<WorkflowRule>() };

        public override string EntityTypeCode => Constants.EntityTypeCode.Workflow;

        public override int ParentEntityId => SiteId;

        public int[] ForceRulesIds { get; set; }
    }
}

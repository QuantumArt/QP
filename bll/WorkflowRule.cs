using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Quantumart.QP8.BLL
{
    public class WorkflowRule
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int? GroupId { get; set; }

        public int? UserId { get; set; }

        public int RuleOrder { get; set; }

        public int WorkflowId { get; set; }

        public int? PredecessorPermissionId { get; set; }

        public int? SuccessorPermissionId { get; set; }

        public int? SuccessorStatusId { get; set; }

        [ValidateNever]
        [BindNever]
        public StatusType StatusType { get; set; }

        public bool IsInvalid { get; set; }
    }
}

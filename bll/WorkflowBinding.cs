using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class WorkflowBind
    {
        #region private members

        private List<StatusType> _StatusTypes;

        private StatusType _MaxStatus;

        private int _CurrentUserMaxWeight;

        private List<StatusType> _AvailableStatuses;

        #endregion

        public static int UnassignedId = 0;

        public WorkflowBind()
        {
            WorkflowId = UnassignedId;
            IsAsync = true;
        }

        [Display(Name = "Workflow", ResourceType = typeof(ContentStrings))]
        public int WorkflowId { get; set; }

        [Display(Name = "SplitArticles", ResourceType = typeof(ContentStrings))]
        public bool IsAsync { get; set; }

        public bool IsAssigned => WorkflowId != UnassignedId;

        [ValidateNever]
        [BindNever]
        public bool CurrentUserCanUpdateArticles => !IsAssigned || QPContext.IsAdmin || CurrentUserMaxWeight > 0;

        [ValidateNever]
        [BindNever]
        public bool CurrentUserCanRemoveArticles => !IsAssigned || QPContext.IsAdmin || CurrentUserHasWorkflowMaxWeight;

        [ValidateNever]
        [BindNever]
        public bool CurrentUserCanPublishArticles => !IsAssigned || QPContext.IsAdmin || CurrentUserHasWorkflowMaxWeight;

        /// <summary>
        /// Список статусов Workflow
        /// </summary>
        [ValidateNever]
        [BindNever]
        public List<StatusType> StatusTypes
        {
            get
            {
                if (_StatusTypes == null)
                {
                    _StatusTypes = WorkflowRepository.GetStatuses(WorkflowId).ToList();
                }
                return _StatusTypes;
            }
        }


        /// <summary>
        /// Список статусов Workflow, доступных для текущего пользователя в виде элементов списка
        /// </summary>
        [ValidateNever]
        [BindNever]
        public List<StatusType> AvailableStatuses
        {
            get
            {
                if (_AvailableStatuses == null)
                {
                    _AvailableStatuses = StatusTypes
                        .Where(n => n.Weight <= CurrentUserMaxWeight)
                        .OrderBy(n => n.Weight)
                        .ToList();
                }
                return _AvailableStatuses;
            }
        }


        /// <summary>
        /// Максимальный статус в Workflow
        /// </summary>
        [ValidateNever]
        [BindNever]
        public StatusType MaxStatus
        {
            get
            {
                if (_MaxStatus == null)
                {
                    _MaxStatus = WorkflowRepository.GetMaxStatus(WorkflowId);
                }
                return _MaxStatus;
            }
        }

        /// <summary>
        /// Максимальный вес, доступный текущему пользователю
        /// </summary>
        [ValidateNever]
        [BindNever]
        public int CurrentUserMaxWeight
        {
            get
            {
                if (_CurrentUserMaxWeight == 0)
                {
                    if (QPContext.IsAdmin)
                    {
                        _CurrentUserMaxWeight = MaxStatus.Weight;
                    }
                    else
                    {
                        _CurrentUserMaxWeight = WorkflowRepository.GetCurrentUserMaxWeight(WorkflowId);
                    }
                }
                return _CurrentUserMaxWeight;
            }
        }

        /// <summary>
        /// Доступен ли пользователю максимальный вес данного Workflow
        /// </summary>
        [ValidateNever]
        [BindNever]
        public bool CurrentUserHasWorkflowMaxWeight => MaxStatus.Weight == CurrentUserMaxWeight;

        public bool UseStatus(int statusTypeId) => WorkflowRepository.DoesWorkflowUseStatus(WorkflowId, statusTypeId);

        public StatusType GetClosestStatus(int statusWeight) => WorkflowRepository.GetClosestStatus(WorkflowId, statusWeight);
    }
}

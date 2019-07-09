using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

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

        [LocalizedDisplayName("Workflow", NameResourceType = typeof(ContentStrings))]
        public int WorkflowId { get; set; }

        [LocalizedDisplayName("SplitArticles", NameResourceType = typeof(ContentStrings))]
        public bool IsAsync { get; set; }

        public bool IsAssigned => WorkflowId != UnassignedId;

        public bool CurrentUserCanUpdateArticles => !IsAssigned || QPContext.IsAdmin || CurrentUserMaxWeight > 0;

        public bool CurrentUserCanRemoveArticles => !IsAssigned || QPContext.IsAdmin || CurrentUserHasWorkflowMaxWeight;

        public bool CurrentUserCanPublishArticles => !IsAssigned || QPContext.IsAdmin || CurrentUserHasWorkflowMaxWeight;

        /// <summary>
        /// Список статусов Workflow
        /// </summary>
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
        public int CurrentUserMaxWeight
        {
            get
            {
                if (_CurrentUserMaxWeight == 0)
                {
                    #warning заглушка по security для Postgres
                    if (QPContext.IsAdmin || QPContext.DatabaseType == DatabaseType.Postgres)
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
        public bool CurrentUserHasWorkflowMaxWeight => MaxStatus.Weight == CurrentUserMaxWeight;

        public bool UseStatus(int statusTypeId) => WorkflowRepository.DoesWorkflowUseStatus(WorkflowId, statusTypeId);

        public StatusType GetClosestStatus(int statusWeight) => WorkflowRepository.GetClosestStatus(WorkflowId, statusWeight);
    }
}

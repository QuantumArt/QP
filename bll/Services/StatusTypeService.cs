using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    public interface IStatusTypeService
    {
        ListResult<StatusTypeListItem> GetStatusesBySiteId(ListCommand cmd, int siteId);

        InitListResult InitList(int parentId);

        StatusType ReadProperties(int id);

        StatusType ReadPropertiesForUpdate(int id);

        StatusType UpdateProperties(StatusType statusType);

        StatusType NewStatusTypeProperties(int parentId);

        StatusType NewStatusTypePropertiesForUpdate(int parentId);

        StatusType SaveProperties(StatusType statusType);

        MessageResult Remove(int id);

        ListResult<StatusTypeListItem> ListForWorkflow(ListCommand listCommand, int[] selectedIds, int workflowId);

        IEnumerable<StatusType> GetColouredStatuses();
    }

    public class StatusTypeService : IStatusTypeService
    {
        public ListResult<StatusTypeListItem> GetStatusesBySiteId(ListCommand cmd, int siteId)
        {
            var list = StatusTypeRepository.GetStatusTypePage(cmd, siteId, out var totalRecords);
            return new ListResult<StatusTypeListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public InitListResult InitList(int parentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewStatusType) && SecurityRepository.IsEntityAccessible(EntityTypeCode.StatusType, parentId, ActionTypeCode.Update)
        };

        public StatusType ReadProperties(int id)
        {
            var status = StatusTypeRepository.GetById(id);
            if (status == null)
            {
                throw new ApplicationException(string.Format(StatusTypeStrings.StatusTypeNotFound, id));
            }

            return status;
        }

        public StatusType ReadPropertiesForUpdate(int id) => ReadProperties(id);

        public StatusType UpdateProperties(StatusType statusType) => StatusTypeRepository.UpdateProperties(statusType);

        public StatusType NewStatusTypeProperties(int parentId) => StatusType.Create(parentId);

        public StatusType NewStatusTypePropertiesForUpdate(int parentId) => NewStatusTypeProperties(parentId);

        public StatusType SaveProperties(StatusType statusType)
        {
            var result = StatusTypeRepository.SaveProperties(statusType);
            return result;
        }

        public MessageResult Remove(int id)
        {
            var status = StatusTypeRepository.GetById(id);
            if (status == null)
            {
                throw new ApplicationException(string.Format(StatusTypeStrings.StatusTypeNotFound, id));
            }

            if (status.BuiltIn)
            {
                return MessageResult.Error(StatusTypeStrings.StatusBuiltIn);
            }

            if (StatusTypeRepository.IsInUseWithArticle(id))
            {
                return MessageResult.Error(StatusTypeStrings.StatusArticleUsage);
            }

            if (StatusTypeRepository.IsInUseWithWorkflow(id))
            {
                return MessageResult.Error(StatusTypeStrings.StatusWorkflowUsage);
            }

            StatusTypeRepository.SetNullAssociatedNotificationsStatusTypesIds(id);
            StatusTypeRepository.RemoveAssociatedContentItemsStatusHistoryRecords(id);
            StatusTypeRepository.RemoveAssociatedWaitingForApprovalRecords(id);
            StatusTypeRepository.Delete(id);

            return null;
        }

        public ListResult<StatusTypeListItem> ListForWorkflow(ListCommand listCommand, int[] selectedIds, int workflowId) => StatusTypeRepository.GetPageForWorkflow(listCommand, selectedIds, workflowId);

        public IEnumerable<StatusType> GetColouredStatuses() => StatusTypeRepository.GetColouredStatuses();
    }
}

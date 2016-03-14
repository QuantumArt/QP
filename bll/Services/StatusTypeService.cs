using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		StatusTypeInitListResult InitList(int parentId);

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
			int totalRecords;
			IEnumerable<StatusTypeListItem> list = StatusTypeRepository.GetStatusTypePage(cmd, siteId, out totalRecords);
			return new ListResult<StatusTypeListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public StatusTypeInitListResult InitList(int parentId)
		{
			return new StatusTypeInitListResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewStatusType) &&
				SecurityRepository.IsEntityAccessible(EntityTypeCode.StatusType, parentId, ActionTypeCode.Update)
			};
		}

		public StatusType ReadProperties(int id)
		{
			StatusType status = StatusTypeRepository.GetById(id);
			if (status == null)
				throw new ApplicationException(String.Format(StatusTypeStrings.StatusTypeNotFound, id));
			return status;
		}

		public StatusType ReadPropertiesForUpdate(int id)
		{
			return ReadProperties(id);
		}

		public StatusType UpdateProperties(StatusType statusType)
		{
			return StatusTypeRepository.UpdateProperties(statusType);
		}


		public StatusType NewStatusTypeProperties(int parentId)
		{
			return StatusType.Create(parentId);
		}

		public StatusType NewStatusTypePropertiesForUpdate(int parentId)
		{
			return NewStatusTypeProperties(parentId);
		}

		public StatusType SaveProperties(StatusType statusType)
		{
			StatusType result = StatusTypeRepository.SaveProperties(statusType);
			return result;
		}


		public MessageResult Remove(int id)
		{
			StatusType status = StatusTypeRepository.GetById(id);
			if (status == null)
				throw new ApplicationException(String.Format(StatusTypeStrings.StatusTypeNotFound, id));
			if (status.BuiltIn)
				return MessageResult.Error(StatusTypeStrings.StatusBuiltIn);
			if(StatusTypeRepository.IsInUseWithArticle(id))
				return MessageResult.Error(StatusTypeStrings.StatusArticleUsage);
			if(StatusTypeRepository.IsInUseWithWorkflow(id))
				return MessageResult.Error(StatusTypeStrings.StatusWorkflowUsage);
			StatusTypeRepository.SetNullAssociatedNotificationsStatusTypesIds(id);
			StatusTypeRepository.RemoveAssociatedContentItemsStatusHistoryRecords(id);
			StatusTypeRepository.RemoveAssociatedWaitingForApprovalRecords(id);			
			StatusTypeRepository.Delete(id);
			return null;
		}


		public ListResult<StatusTypeListItem> ListForWorkflow(ListCommand listCommand, int[] selectedIds, int workflowId)
		{
			return StatusTypeRepository.GetPageForWorkflow(listCommand, selectedIds, workflowId);
		}

		public IEnumerable<StatusType> GetColouredStatuses()
		{
			return StatusTypeRepository.GetColouredStatuses();
		}
	}
}

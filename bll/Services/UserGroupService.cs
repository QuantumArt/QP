using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;


namespace Quantumart.QP8.BLL.Services
{
	public interface IUserGroupService
	{
		UserGroupInitListResult InitList(int parentId);

		ListResult<UserGroupListItem> List(ListCommand cmd, IEnumerable<int> selectedIDs = null);

		IEnumerable<User> GetUsers(IEnumerable<int> userIDs);

		UserGroup ReadProperties(int id);

		UserGroup Read(int id);

		IEnumerable<UserGroup> GetAllGroups();

		UserGroup UpdateProperties(UserGroup userGroup);

		UserGroup NewProperties();

		UserGroup SaveProperties(UserGroup userGroup);

		MessageResult Remove(int id);

		MessageResult RemovePreAction(int id);

		CopyResult Copy(int id);

		UserGroupInitTreeResult InitTree(int parentId);
	}

	public class UserGroupService : IUserGroupService
	{
		public ListResult<UserGroupListItem> List(ListCommand cmd, IEnumerable<int> selectedIDs = null)
		{
			int totalRecords;
			IEnumerable<UserGroupListItem> list = UserGroupRepository.List(cmd, out totalRecords, selectedIDs);
			return new ListResult<UserGroupListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public IEnumerable<User> GetUsers(IEnumerable<int> userIDs)
		{
			return UserRepository.GetUsers(userIDs);
		}

		public IEnumerable<UserGroup> GetAllGroups()
		{
			return UserGroupRepository.GetAllGroups();
		}

		public UserGroup NewProperties()
		{
			return UserGroup.Create();
		}
		public UserGroup ReadProperties(int id)
		{
			UserGroup group = UserGroupRepository.GetPropertiesById(id);
			if (group == null)
				throw new ApplicationException(String.Format(UserGroupStrings.GroupNotFound, id));
			return group;
		}

		public UserGroup Read(int id)
		{
			UserGroup group = UserGroupRepository.GetById(id);
			if (group == null)
				throw new ApplicationException(String.Format(UserGroupStrings.GroupNotFound, id));
			return group;
		}


		public UserGroup UpdateProperties(UserGroup userGroup)
		{
			return UserGroupRepository.UpdateProperties(userGroup);
		}

		public UserGroup SaveProperties(UserGroup userGroup)
		{
			UserGroup result = UserGroupRepository.SaveProperties(userGroup);
			return result;
		}

		public CopyResult Copy(int id)
		{
			CopyResult result = new CopyResult();
			UserGroup group = UserGroupRepository.GetPropertiesById(id);
			if (group == null)
				throw new ApplicationException(String.Format(UserGroupStrings.GroupNotFound, id));

			group.MutateName();
			int newId = UserGroupRepository.CopyGroup(group, QPContext.CurrentUserId);
			if (newId == 0)
				result.Message = MessageResult.Error(UserGroupStrings.GroupHasNotBeenCreated);
			else
				result.Id = newId;
			return result;
		}

		public MessageResult RemovePreAction(int id)
		{
			UserGroup group = UserGroupRepository.GetPropertiesById(id);
			if (group == null)
				throw new ApplicationException(String.Format(UserGroupStrings.GroupNotFound, id));
			if (group.ChildGroups.Any())
				return MessageResult.Confirm(UserGroupStrings.ConfirmHasChildren);
			else
				return null;
		}

		public MessageResult Remove(int id)
		{
			UserGroup group = UserGroupRepository.GetById(id);
			if (group == null)
				throw new ApplicationException(String.Format(UserGroupStrings.GroupNotFound, id));
			if (group.BuiltIn)
				return MessageResult.Error(UserGroupStrings.CannotRemoveBuitInGroup);

			IEnumerable<Notification> notifications = new NotificationRepository().GetUserGroupNotifications(id);
			if (notifications.Any())
			{
				string message = String.Join(",", notifications.Select(w => String.Format("({0}) \"{1}\"", w.Id, w.Name)));
				return MessageResult.Error(String.Format(UserGroupStrings.NotificationsExist, message));
			}

			IEnumerable<Workflow> workflows = WorkflowRepository.GetUserGroupWorkflows(id);
			if (workflows.Any())
			{
				string message = String.Join(",", workflows.Select(w => String.Format("({0}) \"{1}\"", w.Id, w.Name)));
				return MessageResult.Error(String.Format(UserGroupStrings.WorkflowsExist, message));
			}

			UserGroupRepository.Delete(id);
			return null;
		}
	
		public UserGroupInitListResult InitList(int parentId)
		{
			return new UserGroupInitListResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewUserGroup)
			};
		}

		public UserGroupInitTreeResult InitTree(int parentId)
		{
			return new UserGroupInitTreeResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewUserGroup)
			};
		}		
	}
}

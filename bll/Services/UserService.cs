using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Security;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository.Articles;

namespace Quantumart.QP8.BLL.Services
{
	public interface IUserService
	{
		User ReadProperties(int id);
		User ReadProfile(int id);
		UserInitListResult InitList(int parentId);
		ListResult<UserListItem> List(ListCommand cmd, UserListFilter filter, IEnumerable<int> selectedIDs = null);
		User UpdateProfile(User user);
		User UpdateProperties(User user);
		User NewProperties();
		User SaveProperties(User user);
		IEnumerable<UserGroup> GetBindableUserGroups();
		IEnumerable<UserGroup> GetUserGroups(IEnumerable<int> ids);
		MessageResult Remove(int id);
		CopyResult Copy(int id);
		IEnumerable<UserDefaultFilter> GetContentDefaultFilters(int userId);
	}

	public class UserService : IUserService
    {
        public User ReadProfile(int id)
        {
            User user = UserRepository.GetById(id);
			if (user == null)
				throw new ApplicationException(String.Format(UserStrings.UserNotFound, id));
			return user;
        }

		public User ReadProperties(int id)
		{
			User user = UserRepository.GetPropertiesById(id);
			if (user == null)
				throw new ApplicationException(String.Format(UserStrings.UserNotFound, id));
			return user;
		}

		public UserInitListResult InitList(int parentId)
		{
			return new UserInitListResult
			{
				IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewUser)
			};
		}

		public ListResult<UserListItem> List(ListCommand cmd, UserListFilter filter, IEnumerable<int> selectedIDs = null)
		{
			int totalRecords;
			IEnumerable<UserListItem> list = UserRepository.List(cmd, filter, selectedIDs, out totalRecords);
			return new ListResult<UserListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		/// <summary>
		/// Возвращает список групп, кроме Readonly-групп
		/// </summary>
		/// <returns></returns>
		public IEnumerable<UserGroup> GetBindableUserGroups()
		{
			return UserGroupRepository.GetAllGroups().Where(g => !g.IsReadOnly).ToArray();
		}


		public IEnumerable<UserGroup> GetUserGroups(IEnumerable<int> ids)
		{
			return UserGroupRepository.GetAllGroups()
				.Where(g => ids.Contains(g.Id))
				.ToArray();
		}

		public User UpdateProfile(User user)
        {
            if (!String.IsNullOrEmpty(user.NewPassword))
                user.Password = user.NewPassword;
            user.LastModifiedBy = QPContext.CurrentUserId;
            User updatedUser = UserRepository.UpdateProfile(user);
            return updatedUser;
        }

		public User UpdateProperties(User user)
		{
			return UserRepository.UpdateProperties(user);
		}

		/// <summary>
		/// Возвращает пользователя для добавления
		/// </summary>
		public User NewProperties()
		{
			return User.Create();
		}

		public User SaveProperties(User user)
		{
			User result = UserRepository.SaveProperties(user);
			return result;
		}


		public MessageResult Remove(int id)
		{
			User user = UserRepository.GetById(id, true);
			if (user == null)
				throw new ApplicationException(String.Format(UserStrings.UserNotFound, id));
			if (user.BuiltIn)
				return MessageResult.Error(UserStrings.CannotRemoveBuitInUser);

			IEnumerable<Notification> notifications = new NotificationRepository().GetUserNotifications(id);
			if (notifications.Any())
			{
				string message = String.Join(",", notifications.Select(w => String.Format("({0}) \"{1}\"", w.Id, w.Name)));
				return MessageResult.Error(String.Format(UserStrings.NotificationsExist, message));
			}

			IEnumerable<Workflow> workflows = WorkflowRepository.GetUserWorkflows(id);
			if (workflows.Any())
			{
				string message = String.Join(",", workflows.Select(w => String.Format("({0}) \"{1}\"", w.Id, w.Name)));
				return MessageResult.Error(String.Format(UserStrings.WorkflowsExist, message));
			}

			UserRepository.Delete(id);

			return null;

		}

		public CopyResult Copy(int id)
		{
			CopyResult result = new CopyResult();
			User user = UserRepository.GetById(id, true);
			if (user == null)
				throw new ApplicationException(String.Format(UserStrings.UserNotFound, id));

			user.MutateLogin();
			int newId = UserRepository.CopyUser(user, QPContext.CurrentUserId);
			if (newId == 0)
				result.Message = MessageResult.Error(UserStrings.UserHasNotBeenCreated);
			else
				result.Id = newId;
			return result;
		}

		public IEnumerable<UserDefaultFilter> GetContentDefaultFilters(int userId)
		{
			if (userId > 0)
				return UserRepository.GetContentDefaultFilters(userId);
			else
				return Enumerable.Empty<UserDefaultFilter>();
		}
	}
}

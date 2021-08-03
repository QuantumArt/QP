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
    public interface IUserService
    {
        User ReadProperties(int id);

        User ReadProfile(int id);

        InitListResult InitList(int parentId);

        ListResult<UserListItem> List(ListCommand cmd, UserListFilter filter, IEnumerable<int> selectedIDs = null);

        User UpdateProfile(User user);

        User UpdateProperties(User user);

        User GetUserToAdd();

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
            var user = UserRepository.GetById(id);
            if (user == null)
            {
                throw new ApplicationException(string.Format(UserStrings.UserNotFound, id));
            }

            return user;
        }

        public User ReadProperties(int id)
        {
            var user = UserRepository.GetPropertiesById(id);
            if (user == null)
            {
                throw new ApplicationException(string.Format(UserStrings.UserNotFound, id));
            }

            return user;
        }

        public InitListResult InitList(int parentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewUser)
        };

        public ListResult<UserListItem> List(ListCommand cmd, UserListFilter filter, IEnumerable<int> selectedIDs = null)
        {
            var list = UserRepository.List(cmd, filter, selectedIDs, out var totalRecords);
            return new ListResult<UserListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public IEnumerable<UserGroup> GetBindableUserGroups()
        {
            return UserGroupRepository.GetAllGroups().Where(g => !g.IsReadOnly).ToArray();
        }

        public IEnumerable<UserGroup> GetUserGroups(IEnumerable<int> ids)
        {
            return UserGroupRepository.GetAllGroups().Where(g => ids.Contains(g.Id)).ToArray();
        }

        public User UpdateProfile(User user)
        {
            if (!string.IsNullOrEmpty(user.NewPassword))
            {
                user.Password = user.NewPassword;
            }
            user.LastModifiedBy = QPContext.CurrentUserId;
            var updatedUser = UserRepository.UpdateProfile(user);
            return updatedUser;
        }

        public User UpdateProperties(User user) => UserRepository.UpdateProperties(user);

        public User GetUserToAdd() => User.Create();

        public User SaveProperties(User user) => UserRepository.SaveProperties(user);

        public MessageResult Remove(int id)
        {
            var user = UserRepository.GetById(id, true);
            if (user == null)
            {
                throw new ApplicationException(string.Format(UserStrings.UserNotFound, id));
            }

            if (user.BuiltIn)
            {
                return MessageResult.Error(UserStrings.CannotRemoveBuitInUser);
            }

            var notifications = new NotificationRepository().GetUserNotifications(id).ToList();
            if (notifications.Any())
            {
                var message = string.Join(",", notifications.Select(w => $"({w.Id}) \"{w.Name}\""));
                return MessageResult.Error(string.Format(UserStrings.NotificationsExist, message));
            }

            var workflows = WorkflowRepository.GetUserWorkflows(id).ToList();
            if (workflows.Any())
            {
                var message = string.Join(",", workflows.Select(w => $"({w.Id}) \"{w.Name}\""));
                return MessageResult.Error(string.Format(UserStrings.WorkflowsExist, message));
            }

            UserRepository.Delete(id);
            return null;
        }

        public CopyResult Copy(int id)
        {
            var result = new CopyResult();
            var user = UserRepository.GetById(id, true);
            if (user == null)
            {
                throw new ApplicationException(string.Format(UserStrings.UserNotFound, id));
            }

            user.MutateLogin();
            var newId = UserRepository.CopyUser(user, QPContext.CurrentUserId);
            if (newId == 0)
            {
                result.Message = MessageResult.Error(UserStrings.UserHasNotBeenCreated);
            }
            else
            {
                result.Id = newId;
            }

            return result;
        }


        public static bool GetUserMustChangePassword(int userId) => userId > 0 ? UserRepository.GetUserMustChangePassword(userId) : false;

        public IEnumerable<UserDefaultFilter> GetContentDefaultFilters(int userId) => userId > 0 ? UserRepository.GetContentDefaultFilters(userId) : Enumerable.Empty<UserDefaultFilter>();
    }
}

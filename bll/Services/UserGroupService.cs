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
    public class UserGroupService : IUserGroupService
    {
        public ListResult<UserGroupListItem> List(ListCommand cmd, IEnumerable<int> selectedIds = null)
        {
            var list = UserGroupRepository.List(cmd, out var totalRecords, selectedIds?.ToList() ?? new List<int>());
            return new ListResult<UserGroupListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public IEnumerable<User> GetUsers(IEnumerable<int> userIDs) => UserRepository.GetUsers(userIDs);

        public IEnumerable<UserGroup> GetAllGroups() => UserGroupRepository.GetAllGroups();

        public UserGroup NewProperties() => UserGroup.Create();

        public UserGroup ReadProperties(int id)
        {
            var group = UserGroupRepository.GetPropertiesById(id);
            if (group == null)
            {
                throw new ApplicationException(string.Format(UserGroupStrings.GroupNotFound, id));
            }

            return group;
        }

        public UserGroup Read(int id)
        {
            var group = UserGroupRepository.GetById(id);
            if (group == null)
            {
                throw new ApplicationException(string.Format(UserGroupStrings.GroupNotFound, id));
            }

            return group;
        }

        public UserGroup UpdateProperties(UserGroup userGroup) => UserGroupRepository.UpdateProperties(userGroup);

        public UserGroup SaveProperties(UserGroup userGroup)
        {
            var result = UserGroupRepository.SaveProperties(userGroup);
            return result;
        }

        public CopyResult Copy(int id)
        {
            var result = new CopyResult();
            var group = UserGroupRepository.GetPropertiesById(id);
            if (group == null)
            {
                throw new ApplicationException(string.Format(UserGroupStrings.GroupNotFound, id));
            }

            group.MutateName();
            var newId = UserGroupRepository.CopyGroup(group, QPContext.CurrentUserId);
            if (newId == 0)
            {
                result.Message = MessageResult.Error(UserGroupStrings.GroupHasNotBeenCreated);
            }
            else
            {
                result.Id = newId;
            }
            return result;
        }

        public MessageResult RemovePreAction(int id)
        {
            var group = UserGroupRepository.GetPropertiesById(id);
            if (group == null)
            {
                throw new ApplicationException(string.Format(UserGroupStrings.GroupNotFound, id));
            }

            if (group.ChildGroups.Any())
            {
                return MessageResult.Confirm(UserGroupStrings.ConfirmHasChildren);
            }

            return null;
        }

        public MessageResult Remove(int id)
        {
            var group = UserGroupRepository.GetById(id);
            if (group == null)
            {
                throw new ApplicationException(string.Format(UserGroupStrings.GroupNotFound, id));
            }

            if (group.BuiltIn)
            {
                return MessageResult.Error(UserGroupStrings.CannotRemoveBuitInGroup);
            }

            var notifications = new NotificationRepository().GetUserGroupNotifications(id).ToList();
            if (notifications.Any())
            {
                var message = string.Join(",", notifications.Select(w => $"({w.Id}) \"{w.Name}\""));
                return MessageResult.Error(string.Format(UserGroupStrings.NotificationsExist, message));
            }

            var workflows = WorkflowRepository.GetUserGroupWorkflows(id).ToList();
            if (workflows.Any())
            {
                var message = string.Join(",", workflows.Select(w => $"({w.Id}) \"{w.Name}\""));
                return MessageResult.Error(string.Format(UserGroupStrings.WorkflowsExist, message));
            }

            UserGroupRepository.Delete(id);
            return null;
        }

        public InitListResult InitList(int parentId) => new InitListResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewUserGroup)
        };

        public InitTreeResult InitTree(int parentId) => new InitTreeResult
        {
            IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewUserGroup)
        };
    }
}

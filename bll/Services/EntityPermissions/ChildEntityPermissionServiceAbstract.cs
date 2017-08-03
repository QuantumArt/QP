using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
    public abstract class ChildEntityPermissionServiceAbstract : IChildEntityPermissionService
    {
        protected abstract IChildEntityPermissionRepository Repository { get; }

        #region IChildEntityPermissionService Members

        public abstract IPermissionListViewModelSettings ListViewModelSettings { get; }

        public abstract IPermissionViewModelSettings ViewModelSettings { get; }

        public IEnumerable<EntityPermissionLevel> GetPermissionLevels() => CommonPermissionRepository.GetPermissionLevels();

        public ChildPermissionInitListResult InitList(int parentId)
            => new ChildPermissionInitListResult { IsParentPermissionsListActionAccessable = SecurityRepository.IsActionAccessible(ListViewModelSettings.ParentPermissionsListAction) };

        public ListResult<ChildEntityPermissionListItem> List(int parentId, int? groupId, int? userId, ListCommand cmd)
        {
            var list = Enumerable.Empty<ChildEntityPermissionListItem>();
            var totalRecords = 0;
            if (groupId.HasValue || userId.HasValue)
            {
                list = Repository.List(parentId, groupId, userId, cmd, out totalRecords);
            }

            return new ListResult<ChildEntityPermissionListItem>
            {
                Data = list.ToList(),
                TotalRecords = totalRecords
            };
        }

        public virtual void MultipleChange(int parentId, List<int> entityIDs, ChildEntityPermission permissionSettings)
        {
            if (permissionSettings.CopyParentPermission)
            {
                var parentPermission = Repository.GetParentPermission(parentId, permissionSettings.UserId, permissionSettings.GroupId);
                if (parentPermission != null)
                {
                    var parentSettings = ChildEntityPermission.CreateFrom(parentPermission);
                    Repository.MultipleChange(parentId, entityIDs, parentSettings);
                }
                else
                {
                    Repository.MultipleRemove(parentId, entityIDs, permissionSettings.UserId, permissionSettings.GroupId);
                }
            }
            else
            {
                Repository.MultipleChange(parentId, entityIDs, permissionSettings);
            }
        }

        public virtual void Change(int parentId, int entityId, ChildEntityPermission permissionSettings)
        {
            var entityIdsList = new List<int> { entityId };
            if (permissionSettings.CopyParentPermission)
            {
                var parentPermission = Repository.GetParentPermission(parentId, permissionSettings.UserId, permissionSettings.GroupId);
                if (parentPermission != null)
                {
                    var parentSettings = ChildEntityPermission.CreateFrom(parentPermission);
                    Repository.MultipleChange(parentId, entityIdsList, parentSettings);
                }
                else
                {
                    Repository.MultipleRemove(parentId, new[] { entityId }, permissionSettings.UserId, permissionSettings.GroupId);
                }
            }
            else
            {
                Repository.MultipleChange(parentId, entityIdsList, permissionSettings);
            }
        }

        public virtual void ChangeAll(int parentId, ChildEntityPermission permissionSettings)
        {
            if (permissionSettings.CopyParentPermission)
            {
                var parentPermission = Repository.GetParentPermission(parentId, permissionSettings.UserId, permissionSettings.GroupId);
                if (parentPermission != null)
                {
                    var parentSettings = ChildEntityPermission.CreateFrom(parentPermission);
                    Repository.ChangeAll(parentId, parentSettings);
                }
                else
                {
                    Repository.RemoveAll(parentId, permissionSettings.UserId, permissionSettings.GroupId);
                }
            }
            else
            {
                Repository.ChangeAll(parentId, permissionSettings);
            }
        }

        public virtual MessageResult MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId)
        {
            Repository.MultipleRemove(parentId, entityIDs, userId, groupId);
            return null;
        }

        public virtual MessageResult Remove(int parentId, int entityId, int? userId, int? groupId)
        {
            Repository.MultipleRemove(parentId, new[] { entityId }, userId, groupId);
            return null;
        }

        public virtual MessageResult RemoveAll(int parentId, int? userId, int? groupId)
        {
            Repository.RemoveAll(parentId, userId, groupId);
            return null;
        }

        public virtual ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId) => Repository.Read(parentId, entityId, userId, groupId);

        #endregion
    }
}

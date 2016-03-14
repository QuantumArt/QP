using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.EntityPermissions
{
	public abstract class ChildEntityPermissionServiceAbstract : IChildEntityPermissionService
	{
		protected abstract IChildEntityPermissionRepository Repository { get; }

		#region IChildEntityPermissionService Members

		public abstract IPermissionListViewModelSettings ListViewModelSettings { get; }

		public abstract IPermissionViewModelSettings ViewModelSettings { get; }

		public IEnumerable<EntityPermissionLevel> GetPermissionLevels()
		{
			return CommonPermissionRepository.GetPermissionLevels();
		}
		
		public ChildPermissionInitListResult InitList(int parentId)
		{
			return new ChildPermissionInitListResult { IsParentPermissionsListActionAccessable = SecurityRepository.IsActionAccessible(ListViewModelSettings.ParentPermissionsListAction) };
		}

		public ListResult<ChildEntityPermissionListItem> List(int parentId, int? groupId, int? userId, ListCommand cmd)
		{
			int totalRecords;
			IEnumerable<ChildEntityPermissionListItem> list = Enumerable.Empty<ChildEntityPermissionListItem>();
			totalRecords = 0;
			if (groupId.HasValue || userId.HasValue)
				list = Repository.List(parentId, groupId, userId, cmd, out totalRecords);
			return new ListResult<ChildEntityPermissionListItem>
			{
				Data = list.ToList(),
				TotalRecords = totalRecords
			};
		}

		public virtual void MultipleChange(int parentId, IEnumerable<int> entityIDs, ChildEntityPermission permissionSettings)
		{
			ChildEntityPermission localPermissionSettings = permissionSettings;
			if (permissionSettings.CopyParentPermission)
			{
				EntityPermission parentPermission = Repository.GetParentPermission(parentId, permissionSettings.UserId, permissionSettings.GroupId);
				if (parentPermission != null)
				{
					ChildEntityPermission parentSettings = ChildEntityPermission.CreateFrom(parentPermission);
					Repository.MultipleChange(parentId, entityIDs, parentSettings);
				}
				else
					Repository.MultipleRemove(parentId, entityIDs, permissionSettings.UserId, permissionSettings.GroupId);
			}
			else
				Repository.MultipleChange(parentId, entityIDs, permissionSettings);
		}

		public virtual void Change(int parentId, int entityId, ChildEntityPermission permissionSettings)
		{
			ChildEntityPermission localPermissionSettings = permissionSettings;
			if (permissionSettings.CopyParentPermission)
			{
				EntityPermission parentPermission = Repository.GetParentPermission(parentId, permissionSettings.UserId, permissionSettings.GroupId);
				if (parentPermission != null)
				{
					ChildEntityPermission parentSettings = ChildEntityPermission.CreateFrom(parentPermission);
					Repository.MultipleChange(parentId, new[] { entityId }, parentSettings);
				}
				else
					Repository.MultipleRemove(parentId, new[] { entityId }, permissionSettings.UserId, permissionSettings.GroupId);
			}
			else
				Repository.MultipleChange(parentId, new[] { entityId }, permissionSettings);
		}

		public virtual void ChangeAll(int parentId, ChildEntityPermission permissionSettings)
		{
			ChildEntityPermission localPermissionSettings = permissionSettings;
			if (permissionSettings.CopyParentPermission)
			{
				EntityPermission parentPermission = Repository.GetParentPermission(parentId, permissionSettings.UserId, permissionSettings.GroupId);
				if (parentPermission != null)
				{
					ChildEntityPermission parentSettings = ChildEntityPermission.CreateFrom(parentPermission);
					Repository.ChangeAll(parentId, parentSettings);
				}
				else
					Repository.RemoveAll(parentId, permissionSettings.UserId, permissionSettings.GroupId);
			}
			else
				Repository.ChangeAll(parentId, permissionSettings);
		}

		public virtual MessageResult MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId)
		{
			Repository.MultipleRemove(parentId, entityIDs, userId, groupId);
			return null;
		}

		public virtual MessageResult Remove(int parentId, int entityId, int? userId, int? groupId)
		{
			Repository.MultipleRemove(parentId, new[]{entityId}, userId, groupId);
			return null;
		}

		public virtual MessageResult RemoveAll(int parentId, int? userId, int? groupId)
		{
			Repository.RemoveAll(parentId, userId, groupId);
			return null;
		}

        public virtual ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId)
		{
			return Repository.Read(parentId, entityId, userId, groupId);
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;
using System.Data;
using Quantumart.QP8.BLL.Mappers;
using System.Data.Objects;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal class ActionPermissionRepository :  IPermissionRepository
	{
		#region IPermissionRepository Members

		public IEnumerable<ListItems.EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
				IEnumerable<DataRow> rows = Common.GetActionPermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				return MappersRepository.PermissionListItemRowMapper.GetBizList(rows.ToList());
			}
		}

		public EntityPermission GetById(int id, bool include = true)
		{
			ObjectQuery<BackendActionPermissionDAL> set = QPContext.EFContext.BackendActionPermissionSet;
			if (include)
				set = set
					.Include("User")
					.Include("Group")
					.Include("LastModifiedByUser");
			return MappersRepository.BackendActionPermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
		}

		public EntityPermission Save(EntityPermission permission)
		{
			return DefaultRepository.Save<EntityPermission, BackendActionPermissionDAL>(permission);
		}

		public bool CheckUnique(EntityPermission permission)
		{
			return !QPContext.EFContext.BackendActionPermissionSet
				.Any(p =>
						p.ActionId == permission.ParentEntityId &&
						(permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
						(permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
					);
		}

		public EntityPermission Update(EntityPermission permission)
		{
			return DefaultRepository.Update<EntityPermission, BackendActionPermissionDAL>(permission);
		}

		public void MultipleRemove(IEnumerable<int> IDs)
		{
			DefaultRepository.Delete<BackendActionPermissionDAL>(IDs.ToArray());
		}

		public void Remove(int id)
		{
			DefaultRepository.Delete<BackendActionPermissionDAL>(id);
		}

		#endregion
	}
}

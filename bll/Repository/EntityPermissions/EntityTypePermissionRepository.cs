using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Mappers;
using System.Data;
using Quantumart.QP8.DAL;
using System.Data.Objects;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal class EntityTypePermissionRepository : IPermissionRepository
	{
		#region IPermissionRepository Members

		public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
				IEnumerable<DataRow> rows = Common.GetEntityTypePermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				return MappersRepository.PermissionListItemRowMapper.GetBizList(rows.ToList());
			}
		}

		public EntityPermission GetById(int id, bool include = true)
		{
			ObjectQuery<EntityTypePermissionDAL> set = QPContext.EFContext.EntityTypePermissionSet;
			if (include)
				set = set
					.Include("User")
					.Include("Group")
					.Include("LastModifiedByUser");
			return MappersRepository.EntityTypePermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
		}

		public EntityPermission Save(EntityPermission permission)
		{
			return DefaultRepository.Save<EntityPermission, EntityTypePermissionDAL>(permission);
		}

		public bool CheckUnique(EntityPermission permission)
		{
			return !QPContext.EFContext.EntityTypePermissionSet
				.Any(p =>
						p.EntityTypeId == permission.ParentEntityId &&
						(permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
						(permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
					);
		}

		public EntityPermission Update(EntityPermission permission)
		{
			return DefaultRepository.Update<EntityPermission, EntityTypePermissionDAL>(permission);
		}

		public void MultipleRemove(IEnumerable<int> IDs)
		{
			DefaultRepository.Delete<EntityTypePermissionDAL>(IDs.ToArray());
		}

		public void Remove(int id)
		{
			DefaultRepository.Delete<EntityTypePermissionDAL>(id);
		}

		#endregion
	}
}

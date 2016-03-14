using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using System.Data;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Helpers;
using System.Data.Objects;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal class WorkflowPermissionRepository : IPermissionRepository
	{
		#region IPermissionRepository Members
		public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
				IEnumerable<DataRow> rows = Common.GetWorkflowPermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				return MappersRepository.PermissionListItemRowMapper.GetBizList(rows.ToList());
			}
		}

		public EntityPermission GetById(int id, bool include = true)
		{
			ObjectQuery<WorkflowPermissionDAL> set = QPContext.EFContext.WorkflowPermissionSet;
			if (include)
				set = set
					.Include("User")
					.Include("Group")
					.Include("LastModifiedByUser");
			return MappersRepository.WorkflowPermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
		}

		public EntityPermission Save(EntityPermission permission)
		{
			return DefaultRepository.Save<EntityPermission, WorkflowPermissionDAL>(permission);
		}

		public EntityPermission Update(EntityPermission permission)
		{
			return DefaultRepository.Update<EntityPermission, WorkflowPermissionDAL>(permission);
		}


		public bool CheckUnique(EntityPermission permission)
		{
			return !QPContext.EFContext.WorkflowPermissionSet
				.Any(p => 
						p.WorkflowId == permission.ParentEntityId &&
						(permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
						(permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
					);
		}

		public void MultipleRemove(IEnumerable<int> IDs)
		{
			DefaultRepository.Delete<WorkflowPermissionDAL>(IDs.ToArray());
		}

		public void Remove(int id)
		{
			DefaultRepository.Delete<WorkflowPermissionDAL>(id);
		}

		#endregion
	}
}

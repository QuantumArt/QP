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
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal class ArticlePermissionRepository : IPermissionRepository
	{
		#region IPermissionRepository Members

		public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);				
				IEnumerable<DataRow> rows = Common.GetArticlePermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				return MapperFacade.PermissionListItemRowMapper.GetBizList(rows.ToList());
			}
		}

		public EntityPermission GetById(int id, bool include = true)
		{
			ObjectQuery<ArticlePermissionDAL> set = QPContext.EFContext.ArticlePermissionSet;
			if (include)
				set = set
					.Include("User")
					.Include("Group")
					.Include("LastModifiedByUser");
			return MapperFacade.ArticlePermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
		}

		public EntityPermission Save(EntityPermission permission)
		{
			return DefaultRepository.Save<EntityPermission, ArticlePermissionDAL>(permission);
		}

		public EntityPermission Update(EntityPermission permission)
		{
			return DefaultRepository.Update<EntityPermission, ArticlePermissionDAL>(permission);
		}


		public bool CheckUnique(EntityPermission permission)
		{
			return !QPContext.EFContext.ArticlePermissionSet
				.Any(p => 
						p.ArticleId == permission.ParentEntityId &&
						(permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
						(permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
					);
		}

		public void MultipleRemove(IEnumerable<int> IDs)
		{
			DefaultRepository.Delete<ArticlePermissionDAL>(IDs.ToArray());
		}

		public void Remove(int id)
		{
			DefaultRepository.Delete<ArticlePermissionDAL>(id);
		}

		#endregion
	}
}

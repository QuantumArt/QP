using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;
using System.Data;
using Quantumart.QP8.BLL.Mappers;
using System.Data.Objects;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Repository.Helpers;
using System.Diagnostics;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal interface IContentPermissionRepository : IPermissionRepository
	{
		/// <summary>
		/// Фильтрует id связанных контентов оставляя только те для которых нет permission уровня равного или больше чем чтение
		/// </summary>
		/// <param name="relatedContentID"></param>
		/// <param name="userId"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>
		IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentID, int? userId, int? groupId);
		/// <summary>
		/// Возвращает контент
		/// </summary>
		/// <param name="contentID"></param>
		/// <returns></returns>
		Content GetContentByID(int contentID);

		/// <summary>
		/// Создает permisions ордновременно для множества контентов 
		/// </summary>
		/// <param name="noPermissionRelatedContentID"></param>
		/// <param name="nullable1"></param>
		/// <param name="nullable2"></param>
		/// <param name="p"></param>
		void MultipleSetPermission(IEnumerable<int> contentIDs, int? userID, int? groupID, int permissionLevel);
	}

	internal class ContentPermissionRepository : IContentPermissionRepository
	{
		#region IPermissionRepository Members

		public IEnumerable<EntityPermissionListItem> List(ListCommand cmd, int parentId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
				IEnumerable<DataRow> rows = Common.GetContentPermissionPage(scope.DbConnection, parentId, cmd.SortExpression, cmd.FilterExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				return MapperFacade.PermissionListItemRowMapper.GetBizList(rows.ToList());
			}
		}

		public EntityPermission GetById(int id, bool include = true)
		{
			ObjectQuery<ContentPermissionDAL> set = QPContext.EFContext.ContentPermissionSet;
			if (include)
				set = set
					.Include("User")
					.Include("Group")
					.Include("LastModifiedByUser");
			return MapperFacade.ContentPermissionMapper.GetBizObject(set.SingleOrDefault(g => g.Id == id));
		}

		public EntityPermission Save(EntityPermission permission)
		{
			return DefaultRepository.Save<EntityPermission, ContentPermissionDAL>(permission);
		}

		public EntityPermission Update(EntityPermission permission)
		{
			return DefaultRepository.Update<EntityPermission, ContentPermissionDAL>(permission);
		}

		public bool CheckUnique(EntityPermission permission)
		{
			return !QPContext.EFContext.ContentPermissionSet
				.Any(p =>
						p.ContentId == permission.ParentEntityId &&
						(permission.GroupId.HasValue ? p.GroupId == permission.GroupId.Value : p.GroupId == null) &&
						(permission.UserId.HasValue ? p.UserId == permission.UserId.Value : p.UserId == null)
					);
		}


		public void MultipleRemove(IEnumerable<int> IDs)
		{
			DefaultRepository.Delete<ContentPermissionDAL>(IDs.ToArray());
		}

		public void Remove(int id)
		{
			DefaultRepository.Delete<ContentPermissionDAL>(id);
		}

		#endregion

		#region IContentPermissionRepository Members

		public IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentID, int? userId, int? groupId)
		{
			Debug.Assert(relatedContentID != null);
			return ContentPermissionHelper.FilterNoPermissionContent(relatedContentID, userId, groupId);
		}

		public Content GetContentByID(int contentID)
		{
			return ContentRepository.GetByIdWithFields(contentID);
		}

		public void MultipleSetPermission(IEnumerable<int> contentIDs, int? userID, int? groupID, int permissionLevel)
		{
			Debug.Assert(contentIDs != null);
			
			ContentPermissionHelper.MultipleSetPermission(contentIDs, userID, groupID, permissionLevel);
		}

		#endregion
	}
}

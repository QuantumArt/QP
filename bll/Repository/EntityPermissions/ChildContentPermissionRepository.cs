using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;
using System.Data;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Services.DTO;
using System.Diagnostics;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.Helpers;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
	internal interface IChildContentPermissionRepository : IChildEntityPermissionRepository
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
		/// Возвращает контенты
		/// </summary>
		/// <param name="contentID"></param>
		/// <returns></returns>
		IEnumerable<Content> GetContentList(IEnumerable<int> contentIDs);

		/// <summary>
		/// Создает permisions ордновременно для множества контентов 
		/// </summary>
		/// <param name="noPermissionRelatedContentID"></param>
		/// <param name="nullable1"></param>
		/// <param name="nullable2"></param>
		/// <param name="p"></param>
		void MultipleSetPermission(IEnumerable<int> contentIDs, int? userID, int? groupID, int permissionLevel);
	}

	internal class ChildContentPermissionRepository : IChildContentPermissionRepository
	{
		public IEnumerable<ChildEntityPermissionListItem> List(int siteId, int? groupId, int? userId, ListCommand cmd, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
				IEnumerable<DataRow> rows = null;
				totalRecords = 0;
				if (userId.HasValue)
					rows = Common.GetChildContentPermissionsForUser(scope.DbConnection, siteId, userId.Value, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				else if (groupId.HasValue)
					rows = Common.GetChildContentPermissionsForGroup(scope.DbConnection, siteId, groupId.Value, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
				else
					throw new ArgumentNullException("groupId, siteId");
				return MapperFacade.ChildEntityPermissionListItemRowMapper.GetBizList(rows.ToList());
			}
		}

		public ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId)
		{
            using (var scope = new QPConnectionScope())
            {
                DataRow row;
                if (userId.HasValue)
                    row = Common.GetChildContentPermissionForUser(scope.DbConnection, parentId, entityId, userId.Value);
                else if (groupId.HasValue)
                    row = Common.GetChildContentPermissionForGroup(scope.DbConnection, parentId, entityId, groupId.Value);
                else
                    throw new ArgumentNullException("groupId, siteId");

                if (row == null || !row.Field<decimal?>("LevelId").HasValue)
                    return null;

	            bool containsHide = row.Table.Columns.Contains("Hide");
				return ChildEntityPermission.Create(null, parentId, userId, groupId, Convert.ToInt32(row.Field<decimal>("LevelId")), row.Field<bool>("PropagateToItems"), containsHide && row.Field<bool>("Hide"));
            }
		}

		public void MultipleChange(int parentId, IEnumerable<int> entityIDs, ChildEntityPermission permissionSettings)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveChildContentPermissions(scope.DbConnection, entityIDs, permissionSettings.UserId, permissionSettings.GroupId);
				Common.InsertChildContentPermissions(scope.DbConnection, entityIDs,
					permissionSettings.UserId, permissionSettings.GroupId,
					permissionSettings.PermissionLevelId, permissionSettings.PropagateToItems, QPContext.CurrentUserId, permissionSettings.Hide);
			}
		}

		public void ChangeAll(int parentId, ChildEntityPermission permissionSettings)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveChildContentPermissions(scope.DbConnection, parentId, permissionSettings.UserId, permissionSettings.GroupId);
				Common.InsertChildContentPermissions(scope.DbConnection, parentId,
					permissionSettings.UserId, permissionSettings.GroupId,
					permissionSettings.PermissionLevelId, permissionSettings.PropagateToItems, QPContext.CurrentUserId, permissionSettings.Hide);
			}
		}

		public void MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveChildContentPermissions(scope.DbConnection, entityIDs, userId, groupId);
			}
		}

		public void RemoveAll(int parentId, int? userId, int? groupId)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveChildContentPermissions(scope.DbConnection, parentId, userId, groupId);
			}
		}

		public EntityPermission GetParentPermission(int siteId, int? userId, int? groupId)
		{
			return MapperFacade.SitePermissionMapper.GetBizObject(QPContext.EFContext.SitePermissionSet
				.FirstOrDefault(p => 
					p.SiteId == siteId &&
					(groupId.HasValue ? p.GroupId == groupId.Value : p.GroupId == null) &&
					(userId.HasValue ? p.UserId == userId.Value : p.UserId == null)
				)
			);
		}


		#region IChildContentPermissionRepository Members

		public IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentID, int? userId, int? groupId)
		{
			Debug.Assert(relatedContentID != null);
			return ContentPermissionHelper.FilterNoPermissionContent(relatedContentID, userId, groupId);
		}

		public IEnumerable<Content> GetContentList(IEnumerable<int> contentIDs)
		{
			return ContentRepository.GetList(contentIDs, loadFields:true);
		}

		public void MultipleSetPermission(IEnumerable<int> contentIDs, int? userID, int? groupID, int permissionLevel)
		{
			Debug.Assert(contentIDs != null);

			ContentPermissionHelper.MultipleSetPermission(contentIDs, userID, groupID, permissionLevel);
		}

		#endregion
	}
}

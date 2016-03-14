using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository.EntityPermissions;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
	internal static class ContentPermissionHelper
	{
		/// <summary>
		/// Фильтрует id связанных контентов оставляя только те для которых нет permission уровня равного или больше чем чтение
		/// </summary>
		/// <param name="relatedContentID"></param>
		/// <param name="userId"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>
		public static IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentID, int? userId, int? groupId)
		{
			
			IEnumerable<decimal> cIDs = Converter.ToDecimalCollection(relatedContentID.Distinct());
			
			// query возвращает подмножество связанных контентов для user/group c уровнем доступа >= Read
			var query = QPContext.EFContext.ContentPermissionSet
				.Where(p => cIDs.Contains(p.ContentId) && p.PermissionLevel.Level >= Quantumart.QP8.Constants.PermissionLevel.Read);

			if (userId.HasValue)
			{
				decimal uid = Converter.ToDecimal(userId.Value);
				query = query.Where(p => p.UserId == uid);
			}
			else if (groupId.HasValue)
			{
				decimal gid = Converter.ToDecimal(groupId.Value);
				query = query.Where(p => p.GroupId == gid);
			}

			IEnumerable<int> existedPermissionRelContents = Converter.ToInt32Collection(query
				.Select(p => p.ContentId)
				.Distinct()
				.ToArray()
			);

			// Возращает только те связанные контенты которые не входят в множество query
			return relatedContentID
				.Where(id => !existedPermissionRelContents.Contains(id))
				.ToArray();
		}

		/// <summary>
		/// Создает permisions ордновременно для множества контентов 
		/// </summary>
		/// <param name="contentIDs"></param>
		/// <param name="userID"></param>
		/// <param name="groupID"></param>
		/// <param name="permissionLevel"></param>
		internal static void MultipleSetPermission(IEnumerable<int> contentIDs, int? userID, int? groupID, int permissionLevel)
		{
			int permissionlevelID = CommonPermissionRepository.GetPermissionLevels().Single(l => l.Level == permissionLevel).Id;
			using (var scope = new QPConnectionScope())
			{
				Common.RemoveChildContentPermissions(scope.DbConnection, contentIDs, userID, groupID);
				Common.InsertChildContentPermissions(scope.DbConnection, contentIDs, userID, groupID, permissionlevelID, false, QPContext.CurrentUserId, false);
			}
		}
	}
}

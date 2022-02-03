using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal class ChildContentPermissionRepository : IChildContentPermissionRepository
    {
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

        public IEnumerable<int> FilterNoPermissionContent(IEnumerable<int> relatedContentId, int? userId, int? groupId) => ContentPermissionHelper.FilterNoPermissionContent(relatedContentId.ToList(), userId, groupId);

        public IEnumerable<Content> GetContentList(IEnumerable<int> contentIDs) => ContentRepository.GetList(contentIDs, loadFields: true);

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

        public IEnumerable<ChildEntityPermissionListItem> List(int siteId, int? groupId, int? userId, ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
                IEnumerable<DataRow> rows;
                totalRecords = 0;
                if (userId.HasValue)
                {
                    rows = Common.GetChildContentPermissionsForUser(scope.DbConnection, siteId, userId.Value, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                }
                else if (groupId.HasValue)
                {
                    rows = Common.GetChildContentPermissionsForGroup(scope.DbConnection, siteId, groupId.Value, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                }
                else
                {
                    throw new ArgumentNullException(nameof(groupId));
                }

                return MapperFacade.ChildEntityPermissionListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        public void MultipleChange(int parentId, List<int> entityIDs, ChildEntityPermission permissionSettings)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildContentPermissions(scope.DbConnection, entityIDs, permissionSettings.UserId, permissionSettings.GroupId);
                Common.InsertChildContentPermissions(scope.DbConnection, entityIDs,
                    permissionSettings.UserId, permissionSettings.GroupId,
                    permissionSettings.PermissionLevelId, permissionSettings.PropagateToItems, QPContext.CurrentUserId, permissionSettings.Hide);
            }
        }

        public void MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildContentPermissions(scope.DbConnection, entityIDs.ToList(), userId, groupId);
            }
        }

        public void MultipleSetPermission(IEnumerable<int> contentIds, int? userId, int? groupId, int permissionLevel)
        {
            Debug.Assert(contentIds != null);
            ContentPermissionHelper.MultipleSetPermission(contentIds.ToList(), userId, groupId, permissionLevel);
        }

        public ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                DataRow row;
                if (userId.HasValue)
                {
                    row = Common.GetChildContentPermissionForUser(scope.DbConnection, parentId, entityId, userId.Value);
                }
                else if (groupId.HasValue)
                {
                    row = Common.GetChildContentPermissionForGroup(scope.DbConnection, parentId, entityId, groupId.Value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(groupId));
                }

                if (row?.Field<decimal?>("LevelId") == null)
                {
                    return null;
                }

                var containsHide = row.Table.Columns.Contains("Hide");
                return ChildEntityPermission.Create(null, parentId, userId, groupId,
                    Convert.ToInt32(row.Field<decimal>("LevelId")),
                    Convert.ToBoolean(row.Field<decimal>("PropagateToItems")),
                    containsHide && row.Field<bool>("Hide"));
            }
        }

        public void RemoveAll(int parentId, int? userId, int? groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildContentPermissions(scope.DbConnection, parentId, userId, groupId);
            }
        }
    }
}

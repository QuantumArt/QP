using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.EntityPermissions
{
    internal class ChildArticlePermissionRepository : IChildEntityPermissionRepository
    {
        #region IChildEntityPermissionRepository Members

        public IEnumerable<ChildEntityPermissionListItem> List(int contentId, int? groupId, int? userId, ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var titleField = FieldRepository.GetTitleField(contentId);
                var titleFieldName = titleField == null ? FieldName.ContentItemId : titleField.Name;

                cmd.SortExpression = TranslateHelper.TranslateSortExpression(cmd.SortExpression);
                IEnumerable<DataRow> rows;
                totalRecords = 0;
                if (userId.HasValue)
                {
                    rows = Common.GetChildArticlePermissionsForUser(scope.DbConnection, contentId, userId.Value, titleFieldName, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                }
                else if (groupId.HasValue)
                {
                    rows = Common.GetChildArticlePermissionsForGroup(scope.DbConnection, contentId, groupId.Value, titleFieldName, cmd.SortExpression, cmd.StartRecord, cmd.PageSize, out totalRecords);
                }
                else
                {
                    throw new ArgumentNullException(nameof(groupId));
                }

                return MapperFacade.ChildEntityPermissionListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        public ChildEntityPermission Read(int parentId, int entityId, int? userId, int? groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                DataRow row;
                if (userId.HasValue)
                {
                    row = Common.GetChildArticlePermissionForUser(scope.DbConnection, parentId, entityId, userId.Value);
                }
                else if (groupId.HasValue)
                {
                    row = Common.GetChildArticlePermissionForGroup(scope.DbConnection, parentId, entityId, groupId.Value);
                }
                else
                {
                    throw new ArgumentNullException(nameof(groupId));
                }

                return row?.Field<decimal?>("LevelId") == null
                    ? null
                    : ChildEntityPermission.Create(null, parentId, userId, groupId, Convert.ToInt32(row.Field<decimal>("LevelId")));
            }
        }

        public void MultipleChange(int parentId, List<int> entityIDs, ChildEntityPermission permissionSettings)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildArticlePermissions(scope.DbConnection, entityIDs, permissionSettings.UserId, permissionSettings.GroupId);
                Common.InsertChildArticlePermissions(scope.DbConnection, entityIDs,
                    permissionSettings.UserId, permissionSettings.GroupId,
                    permissionSettings.PermissionLevelId, QPContext.CurrentUserId);
            }
        }

        public void ChangeAll(int parentId, ChildEntityPermission permissionSettings)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildArticlePermissions(scope.DbConnection, parentId, permissionSettings.UserId, permissionSettings.GroupId);
                Common.InsertChildArticlePermissions(scope.DbConnection, parentId,
                    permissionSettings.UserId, permissionSettings.GroupId,
                    permissionSettings.PermissionLevelId, QPContext.CurrentUserId);
            }
        }

        public void MultipleRemove(int parentId, IEnumerable<int> entityIDs, int? userId, int? groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildArticlePermissions(scope.DbConnection, entityIDs, userId, groupId);
            }
        }

        public void RemoveAll(int parentId, int? userId, int? groupId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveChildArticlePermissions(scope.DbConnection, parentId, userId, groupId);
            }
        }

        public EntityPermission GetParentPermission(int contentId, int? userId, int? groupId)
        {
            return MapperFacade.ContentPermissionMapper.GetBizObject(QPContext.EFContext.ContentPermissionSet
                .FirstOrDefault(p =>
                    p.ContentId == contentId &&
                    (groupId.HasValue ? p.GroupId == groupId.Value : p.GroupId == null) &&
                    (userId.HasValue ? p.UserId == userId.Value : p.UserId == null)
                )
            );
        }

        #endregion
    }
}

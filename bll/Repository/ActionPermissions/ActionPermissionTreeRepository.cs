using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.ActionPermissions
{
    internal class ActionPermissionTreeRepository
    {
        /// <summary>
        /// Возвращает ноды типов сущностей
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        internal static IEnumerable<ActionPermissionTreeNode> GetEntityTypeTreeNodes(int? userId, int? groupId, int? entityTypeId = null)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = null;
                if (userId.HasValue)
                {
                    rows = Common.GetEntityTypePermissionsForUser(scope.DbConnection, userId.Value, entityTypeId);
                }
                else if (groupId.HasValue)
                {
                    rows = Common.GetEntityTypePermissionsForGroup(scope.DbConnection, groupId.Value, entityTypeId);
                }
                else
                {
                    throw new ArgumentNullException("groupId, siteId");
                }

                var result = MapperFacade.ActionPermissionTreeNodeRowMapper.GetBizList(rows.ToList());
                result.ForEach(n =>
                {
                    n.NodeType = ActionPermissionTreeNode.ENTITY_TYPE_NODE;
                    n.HasChildren = true;
                    n.IconUrl = "entity_type.gif";
                });
                return result;
            }
        }

        /// <summary>
        /// Возвращает ноды действий
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        internal static IEnumerable<ActionPermissionTreeNode> GetActionTreeNodes(int entityTypeId, int? userId, int? groupId, int? actionId = null)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = null;
                if (userId.HasValue)
                {
                    rows = Common.GetActionPermissionsForUser(QPContext.EFContext, scope.DbConnection, userId.Value, entityTypeId, actionId);
                }
                else if (groupId.HasValue)
                {
                    rows = Common.GetActionPermissionsForGroup(QPContext.EFContext, scope.DbConnection, groupId.Value, entityTypeId, actionId);
                }
                else
                {
                    throw new ArgumentNullException("groupId, siteId");
                }

                var result = MapperFacade.ActionPermissionTreeNodeRowMapper.GetBizList(rows.ToList());
                result.ForEach(n =>
                {
                    n.NodeType = ActionPermissionTreeNode.ACTION_NODE;
                    n.HasChildren = false;
                    n.IconUrl = "action.gif";
                });
                return result;
            }
        }
    }
}

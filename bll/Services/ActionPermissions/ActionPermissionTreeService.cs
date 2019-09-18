using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ActionPermissions;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
    public class ActionPermissionTreeService : IActionPermissionTreeService
    {
        public IEnumerable<ActionPermissionTreeNode> GetTreeNodes(PermissionTreeQuery query)
        {
            if (!query.UserId.HasValue && !query.GroupId.HasValue)
            {
                return Enumerable.Empty<ActionPermissionTreeNode>();
            }

            if (query.EntityTypeId.HasValue)
            {
                return ActionPermissionTreeRepository.GetActionTreeNodes(
                    query.EntityTypeId.Value, query.UserId, query.GroupId
                ).OrderBy(n => n.Text);
            }

            return ActionPermissionTreeRepository.GetEntityTypeTreeNodes(
                query.UserId, query.GroupId
             ).OrderBy(n => n.Text);
        }

        public ActionPermissionTreeNode GetTreeNode(PermissionTreeQuery query)
        {
            if (!query.UserId.HasValue && !query.GroupId.HasValue)
            {
                return null;
            }

            ActionPermissionTreeNode result = null;
            if (query.EntityTypeId.HasValue && !query.ActionId.HasValue)
            {
                result = ActionPermissionTreeRepository.GetEntityTypeTreeNodes(query.UserId, query.GroupId, query.EntityTypeId).FirstOrDefault();
                if (result != null)
                {
                    result.Children = ActionPermissionTreeRepository.GetActionTreeNodes(query.EntityTypeId.Value, query.UserId, query.GroupId);
                }
            }
            else if (query.EntityTypeId.HasValue && query.ActionId.HasValue)
            {
                result = ActionPermissionTreeRepository.GetActionTreeNodes(query.EntityTypeId.Value, query.UserId, query.GroupId, query.ActionId).FirstOrDefault();
            }

            return result;
        }
    }
}

using System.Collections.Generic;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services.ActionPermissions
{
    public interface IActionPermissionTreeService
    {
        /// <summary>
        /// Возвращает список нодов для уровня
        /// </summary>
        /// <param name="entityTypeId">Если = 0 то список EntityType иначе список Action для EntityType</param>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        IEnumerable<ActionPermissionTreeNode> GetTreeNodes(PermissionTreeQuery query);

        /// <summary>
        /// Возвращает нод и его дочернии
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <param name="actionId"></param>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        ActionPermissionTreeNode GetTreeNode(PermissionTreeQuery query);
    }
}

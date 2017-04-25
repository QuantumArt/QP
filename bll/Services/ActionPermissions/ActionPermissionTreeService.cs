using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository.ActionPermissions;

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
		IEnumerable<ActionPermissionTreeNode> GetTreeNodes(int? entityTypeId, int? userId, int? groupId);
		/// <summary>
		/// Возвращает нод и его дочернии
		/// </summary>
		/// <param name="entityTypeId"></param>
		/// <param name="actionId"></param>
		/// <param name="userId"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>
		ActionPermissionTreeNode GetTreeNode(int? entityTypeId, int? actionId, int? userId, int? groupId);
	}

	public class ActionPermissionTreeService : IActionPermissionTreeService
	{
		public IEnumerable<ActionPermissionTreeNode> GetTreeNodes(int? entityTypeId, int? userId, int? groupId)
		{
			if(!userId.HasValue && !groupId.HasValue)
				return Enumerable.Empty<ActionPermissionTreeNode>();
			else
			{
				if (entityTypeId.HasValue)
					return ActionPermissionTreeRepository.GetActionTreeNodes(entityTypeId.Value, userId, groupId).OrderBy(n => n.Text);
				else
					return ActionPermissionTreeRepository.GetEntityTypeTreeNodes(userId, groupId).OrderBy(n => n.Text);
			}
		}

		public ActionPermissionTreeNode GetTreeNode(int? entityTypeId, int? actionId, int? userId, int? groupId)
		{
			
			if (!userId.HasValue && !groupId.HasValue)
				return null;
			else
			{
				ActionPermissionTreeNode result = null;
				if (entityTypeId.HasValue && !actionId.HasValue)
				{
					result = ActionPermissionTreeRepository.GetEntityTypeTreeNodes(userId, groupId, entityTypeId).FirstOrDefault();
					if(result != null)
						result.Children = ActionPermissionTreeRepository.GetActionTreeNodes(entityTypeId.Value, userId, groupId);
				}
				else if (entityTypeId.HasValue && actionId.HasValue)
				{
					result = ActionPermissionTreeRepository.GetActionTreeNodes(entityTypeId.Value, userId, groupId, actionId).FirstOrDefault();
				}

				return result;
			}
		}
	}
}

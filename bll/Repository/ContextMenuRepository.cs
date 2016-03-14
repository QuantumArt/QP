using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
	internal class ContextMenuRepository
	{
		/// <summary>
		/// Возвращает контекстное меню по его идентификатору
		/// </summary>
		/// <param name="menuId">идентификатор меню</param>
		/// <returns>контекстное меню</returns>
		internal static ContextMenu GetById(int menuId, bool loadItems = false)
		{
			using (var scope = new QPConnectionScope())
			{
				int userId = QPContext.CurrentUserId;
				ContextMenu menu = MappersRepository.ContextMenuRowMapper.GetBizObject(
					Common.GetContextMenuById(scope.DbConnection, userId, menuId, loadItems));

				return menu;
			}
		}

		/// <summary>
		/// Возвращает контекстное меню по его коду
		/// </summary>
		/// <param name="menuCode">код меню</param>
		/// <returns>контекстное меню</returns>
		internal static ContextMenu GetByCode(string menuCode, bool loadItems = false)
		{
			using (var scope = new QPConnectionScope())
			{
				int userId = QPContext.CurrentUserId;
				ContextMenu menu = MappersRepository.ContextMenuRowMapper.GetBizObject(
					Common.GetContextMenuByCode(scope.DbConnection, userId, menuCode, loadItems));

				return menu;
			}
		}

		/// <summary>
		/// Возвращает список контекстных меню
		/// </summary>
		/// <returns>список контекстных меню</returns>
		internal static List<ContextMenu> GetList()
		{
			using (var scope = new QPConnectionScope())
			{
				int userId = QPContext.CurrentUserId;
				List<ContextMenu> menusList = MappersRepository.ContextMenuRowMapper.GetBizList(
					Common.GetContextMenusList(scope.DbConnection, userId).ToList());

				return menusList;
			}
		}

		/// <summary>
		/// Возвращает список статусов действий
		/// </summary>
		/// <param name="actionCode">код контекстного</param>
		/// <param name="entityId">идентификатор сущности</param>
		/// <returns>список статусов действий</returns>
		internal static IEnumerable<BackendActionStatus> GetStatusesList(string menuCode, int entityId)
		{
			using (var scope = new QPConnectionScope())
			{
				int userId = QPContext.CurrentUserId;
				List<BackendActionStatus> statusesList = MappersRepository.BackendActionStatusMapper.GetBizList(
					Common.GetMenuStatusList(scope.DbConnection, userId, menuCode, entityId).ToList());

				return statusesList;
			}
		}

		internal static EntityType GetEntityType(int menuId)
		{						
			return MappersRepository.EntityTypeMapper.GetBizObject(QPContext.EFContext.EntityTypeSet.SingleOrDefault(t => t.ContextMenuId == menuId));			
		}
	}
}
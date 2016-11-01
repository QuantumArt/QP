using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ContextMenuRepository
    {
        /// <summary>
        /// Возвращает контекстное меню по его идентификатору
        /// </summary>
        internal static ContextMenu GetById(int menuId, bool loadItems = false)
        {
            using (var scope = new QPConnectionScope())
            {
                return MapperFacade.ContextMenuRowMapper.GetBizObject(Common.GetContextMenuById(scope.DbConnection, QPContext.CurrentUserId, menuId, loadItems));
            }
        }

        /// <summary>
        /// Возвращает контекстное меню по его коду
        /// </summary>
        internal static ContextMenu GetByCode(string menuCode, bool loadItems = false)
        {
            using (var scope = new QPConnectionScope())
            {
                return MapperFacade.ContextMenuRowMapper.GetBizObject(Common.GetContextMenuByCode(scope.DbConnection, QPContext.CurrentUserId, menuCode, loadItems));
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
                return MapperFacade.ContextMenuRowMapper.GetBizList(Common.GetContextMenusList(scope.DbConnection, QPContext.CurrentUserId).ToList());
            }
        }

        /// <summary>
        /// Возвращает список статусов действий
        /// </summary>
        internal static IEnumerable<BackendActionStatus> GetStatusesList(string menuCode, int entityId)
        {
            using (var scope = new QPConnectionScope())
            {
                return MapperFacade.BackendActionStatusMapper.GetBizList(Common.GetMenuStatusList(scope.DbConnection, QPContext.CurrentUserId, menuCode, entityId).ToList());
            }
        }

        internal static EntityType GetEntityType(int menuId)
        {
            return MapperFacade.EntityTypeMapper.GetBizObject(QPContext.EFContext.EntityTypeSet.SingleOrDefault(t => t.ContextMenuId == menuId));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.Helpers;
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
            var cmCode = QPContext.EFContext.ContextMenuSet.FirstOrDefault(x => x.Id == menuId)?.Code;
            return string.IsNullOrWhiteSpace(cmCode) ? null : GetByCode(cmCode, loadItems);

        }

        /// <summary>
        /// Возвращает контекстное меню по его коду
        /// </summary>
        internal static ContextMenu GetByCode(string menuCode, bool loadItems = false)
        {
            // using (var scope = new QPConnectionScope())
            // {
            var contextMenu = loadItems
                ? QPContext.EFContext.ContextMenuSet
                    .Include(x => x.Items)
                    .ThenInclude(cmi => cmi.Action)
                    .ThenInclude(act => act.ActionType)
                    .FirstOrDefault(x => x.Code == menuCode)
                : QPContext.EFContext.ContextMenuSet.FirstOrDefault(x => x.Code == menuCode);

            var contextMenuBiz = MapperFacade.ContextMenuMapper.GetBizObject(contextMenu);

            var customActions = BackendActionCache.CustomActions;
            contextMenuBiz.Items = contextMenuBiz.Items.OrderBy(x => x.Order);
            foreach (var menuItem in contextMenuBiz.Items)
            {
                menuItem.Icon = customActions.FirstOrDefault(x => x.Id == menuItem.ActionId)?.IconUrl ?? menuItem.Icon;

                // прогнать Name через translate
            }

            return contextMenuBiz;

            // }
        }

        /// <summary>
        /// Возвращает список контекстных меню
        /// </summary>
        /// <returns>список контекстных меню</returns>
        internal static List<ContextMenu> GetList()
        {
            var contextMenus = QPContext.EFContext.ContextMenuSet.OrderBy(x => x.Code).ToList();
            return MapperFacade.ContextMenuMapper.GetBizList(contextMenus);

            // using (var scope = new QPConnectionScope())
            // {
            //     return MapperFacade.ContextMenuRowMapper.GetBizList(Common.GetContextMenusList(scope.DbConnection, QPContext.CurrentUserId).ToList());
            // }
        }

        /// <summary>
        /// Возвращает список статусов действий
        /// </summary>
        internal static IEnumerable<BackendActionStatus> GetStatusesList(string menuCode, int entityId)
        {
            #warning прикрутить security (qp_get_menu_status_list)

            var contextMenuId = QPContext.EFContext.ContextMenuSet.FirstOrDefault(x => x.Code == menuCode)?.Id;

            return QPContext
                .EFContext
                .ContextMenuItemSet
                .Include(x => x.Action)
                .Where(x => x.ContextMenuId == contextMenuId)
                .Select(x => new BackendActionStatus
                {
                    Code = x.Action.Code,
                    Visible = true
                })
                .ToList();

            // using (var scope = new QPConnectionScope())
            // {
            //     return MapperFacade.BackendActionStatusMapper.GetBizList(Common.GetMenuStatusList(scope.DbConnection, QPContext.CurrentUserId, menuCode, entityId).ToList());
            // }
        }

        internal static EntityType GetEntityType(int menuId)
        {
            return MapperFacade.EntityTypeMapper.GetBizObject(QPContext.EFContext.EntityTypeSet.SingleOrDefault(t => t.ContextMenuId == menuId));
        }
    }
}

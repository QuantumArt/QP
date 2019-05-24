using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ToolbarRepository
    {
        /// <summary>
        /// Возвращает список кнопок панели инструментов по коду действия
        /// </summary>
        /// <param name="actionCode">код действия</param>
        /// <returns>список кнопок панели инструментов</returns>
        internal static IEnumerable<ToolbarButton> GetButtonListByActionCode(string actionCode, int entityId)
        {
            using (var scope = new QPConnectionScope())
            {
                var actionId = BackendActionCache.Actions.FirstOrDefault(x => x.Code.Equals(actionCode, StringComparison.InvariantCultureIgnoreCase))?.Id;
                var rows = Common.GetToolbarButtonsForAction(QPContext.EFContext, scope.DbConnection, QPContext.CurrentUserId, QPContext.IsAdmin, actionId, actionCode, entityId);
                return MapperFacade.ToolbarButtonRowMapper.GetBizList(rows.ToList());
                #warning Отфильтровать по qp_action_visible
            }
        }
    }
}

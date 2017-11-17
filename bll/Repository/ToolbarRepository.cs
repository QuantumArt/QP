using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
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
                var rows = Common.GetToolbarButtonsForAction(scope.DbConnection, QPContext.CurrentUserId, actionCode, entityId);
                return MapperFacade.ToolbarButtonRowMapper.GetBizList(rows.ToList());
            }
        }
    }
}

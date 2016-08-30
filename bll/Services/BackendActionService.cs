using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.SharedLogic;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services
{
    public class BackendActionService
    {
        /// <summary>
        /// Возвращает действие по его идентификатору
        /// </summary>
        /// <param name="actionId">идентификатор действия</param>
        /// <returns>действие</returns>
        public static BackendAction GetById(int actionId)
        {
            return BackendActionRepository.GetById(actionId);
        }

        /// <summary>
        /// Возвращает код действия по его Id
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public static string GetCodeById(int actionId)
        {
            return BackendActionRepository.GetById(actionId)?.Code;
        }

        /// <summary>
        /// Возвращает действие по его коду
        /// </summary>
        /// <param name="actionCode">код действия</param>
        /// <returns>действие</returns>
        public static BackendAction GetByCode(string actionCode)
        {
            return string.IsNullOrWhiteSpace(actionCode) ? null : BackendActionRepository.GetByCode(actionCode);
        }

        /// <summary>
        /// Возвращает список статусов действий
        /// </summary>
        /// <param name="actionCode">код родительского действия</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <param name="parentEntityId">родительский идентификатор сущности</param>
        /// <returns>список статусов действий</returns>
        public static IEnumerable<BackendActionStatus> GetStatusesList(string actionCode, int entityId, int parentEntityId)
        {
            return ResolveStatusForCustomActions(actionCode, entityId, parentEntityId, BackendActionRepository.GetStatusesList(actionCode, entityId));
        }

        /// <summary>
        /// Устанавливает соответствующий статус элементам меню связанным с Custom Action
        /// </summary>
        private static IEnumerable<BackendActionStatus> ResolveStatusForCustomActions(string actionCode, int entityId, int parentEntityId, IEnumerable<BackendActionStatus> statuses)
        {
            return CustomActionResolver.ResolveStatus(GetByCode(actionCode).EntityType.Code, entityId, parentEntityId, statuses.ToArray());
        }

        /// <summary>
        /// Возвращает словарь EntityTypeId -> Action ListItem Collection
        /// только статические интерфейсные деуствия
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EntityTypeIdToActionListItemPair> GetEntityTypeIdToActionListItemsDictionary()
        {
            return BackendActionRepository.GetInterfaceActionsForCustom().GroupBy(a => a.EntityTypeId).Select(g => new EntityTypeIdToActionListItemPair
            {
                EntityTypeId = g.Key,
                ActionItems = g.Select(a => new SimpleListItem
                {
                    Value = a.Id.ToString(),
                    Text = Translator.Translate(a.Name)
                }).OrderBy(n => n.Text).ToArray()
            }).ToArray();
        }
    }
}

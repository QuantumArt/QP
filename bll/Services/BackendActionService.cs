using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.SharedLogic;

namespace Quantumart.QP8.BLL.Services
{
    public class BackendActionService
    {
        public static BackendAction GetById(int actionId) => BackendActionRepository.GetById(actionId);

        public static string GetCodeById(int actionId) => BackendActionRepository.GetById(actionId)?.Code;

        public static BackendAction GetByCode(string actionCode) => string.IsNullOrWhiteSpace(actionCode) ? null : BackendActionRepository.GetByCode(actionCode);

        public static BackendAction GetByAlias(string alias) => string.IsNullOrWhiteSpace(alias) ? null : BackendActionRepository.GetByAlias(alias);

        public static IEnumerable<BackendActionStatus> GetStatusesList(string actionCode, int entityId, int parentEntityId) => ResolveStatusForCustomActions(actionCode, entityId, parentEntityId, BackendActionRepository.GetStatusesList(actionCode, entityId));

        /// <summary>
        /// Устанавливает соответствующий статус элементам меню связанным с Custom Action
        /// </summary>
        private static IEnumerable<BackendActionStatus> ResolveStatusForCustomActions(string actionCode, int entityId, int parentEntityId, IEnumerable<BackendActionStatus> statuses) => CustomActionResolver.ResolveStatus(GetByCode(actionCode).EntityType.Code, entityId, parentEntityId, statuses.ToArray());

        /// <summary>
        /// Возвращает словарь EntityTypeId -> Action ListItem Collection
        /// только статические интерфейсные деуствия
        /// </summary>
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

using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Repository
{
    internal static class BackendActionRepository
    {
        internal static BackendAction GetById(int actionId)
        {
            var action = BackendActionCache.Actions.SingleOrDefault(a => a.Id == actionId);
            if (action == null)
            {
                throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFound, actionId));
            }

            return action;
        }

        internal static BackendAction GetByCode(string actionCode)
        {
            var action = BackendActionCache.Actions.SingleOrDefault(a => a.Code == actionCode);
            if (action == null)
            {
                throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFoundByCode, actionCode));
            }

            return action;
        }

        internal static BackendAction GetByAlias(string alias)
        {
            var actionId = BackendActionCache.CustomActions.SingleOrDefault(a => String.Equals(a.Alias, alias, StringComparison.InvariantCultureIgnoreCase))?.ActionId;
            var action = BackendActionCache.Actions.SingleOrDefault(a => a.Id == actionId);
            if (action == null)
            {
                throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFoundByAlias, alias));
            }

            return action;
        }

        internal static IEnumerable<BackendActionCacheRecord> GetActionContextCacheData()
        {
            return BackendActionCache.Actions.Select(a => new BackendActionCacheRecord { ActionCode = a.Code, ActionTypeCode = a.ActionType.Code, EntityTypeCode = a.EntityType.Code }).ToArray();
        }

        internal static IEnumerable<BackendActionStatus> GetStatusesList(string actionCode, int entityId)
        {

            using (var scope = new QPConnectionScope())
            {
                var userId = QPContext.CurrentUserId;
                var action = BackendActionCache.Actions.FirstOrDefault(x => x.Code == actionCode);
                if (action == null)
                {
                    throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFoundByCode, actionCode));
                }
                var actionId = action.Id;
                var entityCode = EntityTypeRepository.GetById(action.EntityTypeId)?.Code;
                var statusesList = MapperFacade.BackendActionStatusMapper.GetBizList(
                    CommonSecurity.GetActionStatusList(
                        QPContext.EFContext,
                        scope.DbConnection,
                        userId, actionCode, actionId, entityId, entityCode,
                        QPContext.IsAdmin
                    ).ToList()
                );
                return statusesList;
            }
        }

        internal static IEnumerable<BackendAction> GetInterfaceActionsForCustom()
        {
            var refresh = BackendActionTypeRepository.GetByCode(ActionTypeCode.Refresh);
            return BackendActionCache.Actions.Where(a => a.IsInterface && a.TypeId != refresh.Id).ToArray();
        }

        internal static BackendAction SaveOrUpdate(string entityTypeCode, string actionTypeCode)
        {
            return BackendActionCache.Actions.FirstOrDefault(a => StringComparer.InvariantCultureIgnoreCase.Equals(a.EntityType.Code, entityTypeCode) && StringComparer.InvariantCultureIgnoreCase.Equals(a.ActionType.Code, actionTypeCode));
        }
    }
}

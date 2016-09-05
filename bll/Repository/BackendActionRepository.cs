﻿using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Repository
{
    internal class BackendActionRepository
    {

        /// <summary>
        /// Возвращает действие по его идентификатору
        /// </summary>
        /// <param name="actionId">идентификатор действия</param>
        /// <returns>действие</returns>
        internal static BackendAction GetById(int actionId)
        {
            var action = BackendActionCache.Actions.SingleOrDefault(a => a.Id == actionId);
            if (action == null)
            {
                throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFound, actionId));
            }

            return action;
        }

        /// <summary>
        /// Возвращает действие по его коду
        /// </summary>
        /// <param name="actionCode">код действия</param>
        /// <returns>действие</returns>
        internal static BackendAction GetByCode(string actionCode)
        {
            var action = BackendActionCache.Actions.SingleOrDefault(a => a.Code == actionCode);
            if (action == null)
            {
                throw new ApplicationException(string.Format(CustomActionStrings.ActionNotFoundByCode, actionCode));
            }

            return action;
        }

        internal static IEnumerable<BackendActionCacheRecord> GetActionContextCacheData()
        {
            return BackendActionCache.Actions.Select(a => new BackendActionCacheRecord { ActionCode = a.Code, ActionTypeCode = a.ActionType.Code, EntityTypeCode = a.EntityType.Code }).ToArray();
        }

        /// <summary>
        /// Возвращает список статусов действий
        /// </summary>
        /// <param name="actionCode">код родительского действия</param>
        /// <param name="entityId">идентификатор сущности</param>
        /// <returns>список статусов действий</returns>
        internal static IEnumerable<BackendActionStatus> GetStatusesList(string actionCode, int entityId)
        {
            using (var scope = new QPConnectionScope())
            {
                var userId = QPContext.CurrentUserId;
                var statusesList = MappersRepository.BackendActionStatusMapper.GetBizList(Common.GetActionStatusList(scope.DbConnection, userId, actionCode, entityId).ToList());
                return statusesList;
            }
        }

        internal static IEnumerable<BackendAction> GetInterfaceActionsForCustom()
        {
            var refresh = BackendActionTypeRepository.GetByCode(ActionTypeCode.Refresh);
            return BackendActionCache.Actions.Where(a => a.IsInterface && a.TypeId != refresh.Id).ToArray();
        }

        internal static BackendAction GetAction(string entityTypeCode, string actionTypeCode)
        {
            return BackendActionCache.Actions.FirstOrDefault(a => StringComparer.InvariantCultureIgnoreCase.Equals(a.EntityType.Code, entityTypeCode) && StringComparer.InvariantCultureIgnoreCase.Equals(a.ActionType.Code, actionTypeCode));
        }
    }
}

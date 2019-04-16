using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    public interface IBackendActionLogRepository
    {
        /// <summary>
        /// Сохранить запись в БД
        /// </summary>
        IEnumerable<BackendActionLog> Save(IEnumerable<BackendActionLog> logs);

        /// <summary>
        /// Получить список заголовков entity
        /// </summary>
        IEnumerable<ListItem> GetEntityTitles(string entityTypeCode, int? parentEntityId, IEnumerable<int> entitiesIDs);
    }

    public interface IBackendActionLogPagesRepository
    {
        /// <summary>
        /// Получить страницу лога
        /// </summary>
        IEnumerable<BackendActionLog> GetPage(ListCommand cmd, BackendActionLogFilter filter, out int totalRecords);

        /// <summary>
        /// Получить список Action Type
        /// </summary>
        /// <returns></returns>
        IEnumerable<BackendActionType> GetActionTypeList();

        /// <summary>
        /// Получить список Entity Type
        /// </summary>
        IEnumerable<EntityType> GetEntityTypeList();

        IEnumerable<BackendAction> GetActionList();
    }

    public interface IButtonTracePagesRepository
    {
        IEnumerable<ButtonTrace> GetPage(ListCommand cmd, out int totalRecords);
    }

    public interface IRemovedEntitiesPagesRepository
    {
        IEnumerable<RemovedEntity> GetPage(ListCommand cmd, out int totalRecords);
    }

    public interface ISessionLogRepository
    {
        IEnumerable<SessionsLog> GetSucessfullSessionPage(ListCommand cmd, out int totalRecords);

        IEnumerable<SessionsLog> GetFailedSessionPage(ListCommand cmd, out int totalRecords);

        SessionsLog Save(SessionsLog session);

        SessionsLog GetCurrent();

        SessionsLog Update(SessionsLog session);
    }

    public sealed class AuditRepository : IBackendActionLogRepository, IBackendActionLogPagesRepository, IButtonTracePagesRepository, IRemovedEntitiesPagesRepository, ISessionLogRepository
    {
        public IEnumerable<BackendActionLog> Save(IEnumerable<BackendActionLog> logs)
        {
            IEnumerable<BackendActionLogDAL> toSave = MapperFacade.BackendActionLogMapper.GetDalList(logs.ToList());
            var saved = DefaultRepository.SimpleSaveBulk(toSave);
            return MapperFacade.BackendActionLogMapper.GetBizList(saved.ToList());
        }

        public IEnumerable<ListItem> GetEntityTitles(string entityTypeCode, int? parentEntityId, IEnumerable<int> entitiesIDs)
        {
            if (entityTypeCode == EntityTypeCode.Article && parentEntityId.HasValue && parentEntityId != 0)
            {
                return ArticleRepository.GetSimpleList(parentEntityId.Value, null, null, ListSelectionMode.OnlySelectedItems, entitiesIDs.ToArray(), null, 0);
            }

            using (var scope = new QPConnectionScope())
            {
                return Common.GetEntitiesTitles(scope.DbConnection, entityTypeCode, entitiesIDs.ToList()).Select(r => new ListItem(Converter.ToInt32(r["ID"]).ToString(), r["TITLE"].ToString()));
            }
        }

        IEnumerable<BackendActionLog> IBackendActionLogPagesRepository.GetPage(ListCommand cmd, BackendActionLogFilter filter, out int totalRecords)
        {
            filter = filter ?? new BackendActionLogFilter();
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetActionLogPage(scope.DbConnection, cmd.SortExpression,
                    filter.actionCode,
                    filter.actionTypeCode,
                    filter.entityTypeCode,
                    filter.entityStringId,
                    filter.parentEntityId,
                    filter.entityTitle,
                    filter.from, filter.to,
                    (filter.userIDs ?? Enumerable.Empty<int>()).ToList(),
                    out totalRecords, cmd.StartRecord, cmd.PageSize);

                return MapperFacade.BackendActionLogRowMapper.GetBizList(rows.ToList());
            }
        }

        public IEnumerable<BackendActionType> GetActionTypeList()
        {
            return BackendActionTypeRepository.GetList().Where(r => r.RequiredPermissionLevel >= PermissionLevel.Modify).ToArray();
        }

        public IEnumerable<EntityType> GetEntityTypeList() => EntityTypeRepository.GetList();

        public IEnumerable<BackendAction> GetActionList()
        {
            return BackendActionCache.Actions.Where(r => r.ActionType.RequiredPermissionLevel >= PermissionLevel.Modify).ToArray();
        }

        IEnumerable<ButtonTrace> IButtonTracePagesRepository.GetPage(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetButtonTracePage(scope.DbConnection, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                IEnumerable<ButtonTrace> result = MapperFacade.ButtonTraceRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        IEnumerable<RemovedEntity> IRemovedEntitiesPagesRepository.GetPage(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetRemovedEntitiesPage(scope.DbConnection, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                IEnumerable<RemovedEntity> result = MapperFacade.RemovedEntitiesRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        public IEnumerable<SessionsLog> GetSucessfullSessionPage(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetSessionsPage(scope.DbConnection, false, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                IEnumerable<SessionsLog> result = MapperFacade.SessionsLogRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        public IEnumerable<SessionsLog> GetFailedSessionPage(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetSessionsPage(scope.DbConnection, true, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                IEnumerable<SessionsLog> result = MapperFacade.SessionsLogRowMapper.GetBizList(rows.ToList());
                return result;
            }
        }

        public SessionsLog Save(SessionsLog session)
        {
            var sessionsLogDal = MapperFacade.SessionsLogMapper.GetDalObject(session);
            sessionsLogDal = DefaultRepository.SimpleSave(sessionsLogDal);
            return MapperFacade.SessionsLogMapper.GetBizObject(sessionsLogDal);
        }

        public SessionsLog GetCurrent()
        {
            var uid = Converter.ToDecimal(QPContext.CurrentUserId);
            var sid = Converter.ToDecimal(QPContext.CurrentSessionId);

            var slDal =
                QPContext.EFContext.SessionsLogSet
                    .Where(s => !s.IsQP7 && s.UserId == uid && s.SessionId == sid)
                    .FirstOrDefault() ??
                QPContext.EFContext.SessionsLogSet
                    .Where(s => !s.IsQP7 && s.UserId == uid)
                    .OrderByDescending(s => s.StartTime)
                    .FirstOrDefault();

            return MapperFacade.SessionsLogMapper.GetBizObject(slDal);
        }

        public SessionsLog Update(SessionsLog session)
        {
            var sessionsLogDal = MapperFacade.SessionsLogMapper.GetDalObject(session);
            sessionsLogDal = DefaultRepository.SimpleUpdate(sessionsLogDal);
            return MapperFacade.SessionsLogMapper.GetBizObject(sessionsLogDal);
        }

        public void ClearUserToken(int userId, int sessionId)
        {
            using (var scope = new QPConnectionScope())
            {
                CommonSecurity.ClearUserToken(scope.DbConnection, userId, sessionId);
            }
        }

    }
}

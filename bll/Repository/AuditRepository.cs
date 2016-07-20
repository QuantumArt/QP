using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Mappers;
using System.Data;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.ListItems;

namespace Quantumart.QP8.BLL.Repository
{
	#region Interfaces

	#region Backend Action Log
	public interface IBackendActionLogRepository
	{
		/// <summary>
		/// Сохранить запись в БД
		/// </summary>
		/// <param name="log"></param>
		/// <returns></returns>
		IEnumerable<BackendActionLog> Save(IEnumerable<BackendActionLog> logs);
		/// <summary>
		/// Получить список заголовков entity
		/// </summary>
		/// <param name="entitiesIDs"></param>
		/// <returns></returns>
		IEnumerable<DataRow> GetEntityTitles(string entityTypeCode, IEnumerable<int> entitiesIDs);
	}

	public interface IBackendActionLogPagesRepository
	{
		/// <summary>
		/// Получить страницу лога
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="filter"></param>
		/// <param name="totalRecords"></param>
		/// <returns></returns>
		IEnumerable<BackendActionLog> GetPage(ListCommand cmd, BackendActionLogFilter filter, out int totalRecords);
		/// <summary>
		/// Получить список Action Type
		/// </summary>
		/// <returns></returns>
		IEnumerable<BackendActionType> GetActionTypeList();
		/// <summary>
		/// Получить список Entity Type
		/// </summary>
		/// <returns></returns>
		IEnumerable<EntityType> GetEntityTypeList();
	}
	#endregion

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


	#endregion


	public sealed class AuditRepository :	IBackendActionLogRepository, IBackendActionLogPagesRepository,
											IButtonTracePagesRepository,
											IRemovedEntitiesPagesRepository,
											ISessionLogRepository
	{
		#region Backend Action Log
		#region IBackendActionLogRepository
		public IEnumerable<BackendActionLog> Save(IEnumerable<BackendActionLog> logs)
		{
			IEnumerable<BackendActionLogDAL> toSave = MappersRepository.BackendActionLogMapper.GetDalList(logs.ToList());
			IEnumerable<BackendActionLogDAL> saved = DefaultRepository.SimpleSave(toSave);
			return MappersRepository.BackendActionLogMapper.GetBizList(saved.ToList());
		}

		public IEnumerable<DataRow> GetEntityTitles(string entityTypeCode, IEnumerable<int> entitiesIDs)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.GetEntitiesTitles(scope.DbConnection, entityTypeCode, entitiesIDs);
			}
		}
		#endregion

		#region IBackendActionLogPagesRepository
        IEnumerable<BackendActionLog> IBackendActionLogPagesRepository.GetPage(ListCommand cmd, BackendActionLogFilter filter, out int totalRecords)
		{
			filter = filter ?? new BackendActionLogFilter();
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetActionLogPage(scope.DbConnection, cmd.SortExpression,
					filter.actionTypeCode,
					filter.entityTypeCode,
					filter.entityStringId,
					filter.parentEntityId,
 					filter.entityTitle,
					filter.from, filter.to,
					filter.userIDs ?? Enumerable.Empty<int>(),
					out totalRecords, cmd.StartRecord, cmd.PageSize);
				var result = MappersRepository.BackendActionLogRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}

		public IEnumerable<BackendActionType> GetActionTypeList()
		{
			return BackendActionTypeRepository.GetList().Where(r => r.RequiredPermissionLevel >= 3).ToArray();
		}

		public IEnumerable<EntityType> GetEntityTypeList()
		{
			return EntityTypeRepository.GetList();
		}
		#endregion
		#endregion

		#region Button Trace

		IEnumerable<ButtonTrace> IButtonTracePagesRepository.GetPage(ListCommand cmd, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetButtonTracePage(scope.DbConnection, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
				IEnumerable<ButtonTrace> result = MappersRepository.ButtonTraceRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}

		#endregion

		#region Removed Entities

		IEnumerable<RemovedEntity> IRemovedEntitiesPagesRepository.GetPage(ListCommand cmd, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetRemovedEntitiesPage(scope.DbConnection, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
				IEnumerable<RemovedEntity> result = MappersRepository.RemovedEntitiesRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}

		#endregion

		#region Sessions

		public IEnumerable<SessionsLog> GetSucessfullSessionPage(ListCommand cmd, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetSessionsPage(scope.DbConnection, false, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
				IEnumerable<SessionsLog> result = MappersRepository.SessionsLogRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}

		public IEnumerable<SessionsLog> GetFailedSessionPage(ListCommand cmd, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetSessionsPage(scope.DbConnection, true, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
				IEnumerable<SessionsLog> result = MappersRepository.SessionsLogRowMapper.GetBizList(rows.ToList());
				return result;
			}
		}

		public SessionsLog Save(SessionsLog session)
		{
			SessionsLogDAL sessionsLogDAL = MappersRepository.SessionsLogMapper.GetDalObject(session);
			sessionsLogDAL = DefaultRepository.SimpleSave(sessionsLogDAL);
			return MappersRepository.SessionsLogMapper.GetBizObject(sessionsLogDAL);
		}

		public SessionsLog GetCurrent()
		{
			decimal uid = Converter.ToDecimal(QPContext.CurrentUserId);
			SessionsLogDAL slDal =
				QPContext.EFContext.SessionsLogSet
				.Where(s => !s.IsQP7 && s.UserId == uid)
				.OrderByDescending(s => s.StartTime)
				.FirstOrDefault();
			return MappersRepository.SessionsLogMapper.GetBizObject(slDal);
		}

		public SessionsLog Update(SessionsLog session)
		{
			SessionsLogDAL sessionsLogDAL = MappersRepository.SessionsLogMapper.GetDalObject(session);
			sessionsLogDAL = DefaultRepository.SimpleUpdate(sessionsLogDAL);
			return MappersRepository.SessionsLogMapper.GetBizObject(sessionsLogDAL);
		}

		#endregion
	}
}

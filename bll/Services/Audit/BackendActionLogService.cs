using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.Audit
{
    public interface IBackendActionLogService
    {
        /// <summary>
        /// Получить страницу лога
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="filter"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        ListResult<BackendActionLog> GetLogPage(ListCommand cmd, BackendActionLogFilter filter);

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

        IEnumerable<BackendAction> GetActionList();
    }

    public class BackendActionLogService : IBackendActionLogService
    {
        private readonly IBackendActionLogPagesRepository repository;

        public BackendActionLogService(IBackendActionLogPagesRepository repository)
        {
            this.repository = repository;
        }

        #region IBackendActionLogService Members

        public ListResult<BackendActionLog> GetLogPage(ListCommand cmd, BackendActionLogFilter filter)
        {
            var data = repository.GetPage(cmd, filter, out var totalRecords).ToList();
            return new ListResult<BackendActionLog>
            {
                Data = data,
                TotalRecords = totalRecords
            };
        }

        public IEnumerable<BackendActionType> GetActionTypeList() => repository.GetActionTypeList();

        public IEnumerable<EntityType> GetEntityTypeList() => repository.GetEntityTypeList();

        public IEnumerable<BackendAction> GetActionList() => repository.GetActionList();

        #endregion
    }
}

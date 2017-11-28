using System.Linq;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.Audit
{
    public interface ISessionLogService
    {
        /// <summary>
        /// Получить список успешных сессий
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        ListResult<SessionsLog> GetSucessfullSessionPage(ListCommand cmd);

        /// <summary>
        /// Получить список неудачных попыток логина
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        ListResult<SessionsLog> GetFailedSessionPage(ListCommand cmd);
    }

    public class SessionLogService : ISessionLogService
    {
        private readonly ISessionLogRepository repository;

        public SessionLogService(ISessionLogRepository repository)
        {
            this.repository = repository;
        }

        #region ISessionLogService Members

        public ListResult<SessionsLog> GetSucessfullSessionPage(ListCommand cmd)
        {
            var sessions = repository.GetSucessfullSessionPage(cmd, out var totalRecords).ToList();
            return new ListResult<SessionsLog>
            {
                Data = sessions,
                TotalRecords = totalRecords
            };
        }

        public ListResult<SessionsLog> GetFailedSessionPage(ListCommand cmd)
        {
            var sessions = repository.GetFailedSessionPage(cmd, out var totalRecords).ToList();
            return new ListResult<SessionsLog>
            {
                Data = sessions,
                TotalRecords = totalRecords
            };
        }

        #endregion
    }
}

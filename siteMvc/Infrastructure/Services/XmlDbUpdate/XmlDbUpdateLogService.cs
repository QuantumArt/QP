using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;

namespace Quantumart.QP8.BLL.Services.XmlDbUpdate
{
    public class XmlDbUpdateLogService : IXmlDbUpdateLogService
    {
        private readonly IXmlDbUpdateLogRepository _dbUpdateLogRepository;
        private readonly IXmlDbUpdateActionsLogRepository _dbUpdateActionsLogRepository;

        public XmlDbUpdateLogService(IXmlDbUpdateLogRepository dbUpdateLogRepository, IXmlDbUpdateActionsLogRepository dbUpdateActionsLogRepository)
        {
            _dbUpdateLogRepository = dbUpdateLogRepository;
            _dbUpdateActionsLogRepository = dbUpdateActionsLogRepository;
        }

        public bool IsFileAlreadyReplayed(string hash)
        {
            return _dbUpdateLogRepository.IsExist(hash);
        }

        public int InsertFileLogEntry(XmlDbUpdateLogModel dbLogEntry)
        {
            return _dbUpdateLogRepository.Insert(dbLogEntry);
        }

        public bool IsActionAlreadyReplayed(string logEntryHash)
        {
            return _dbUpdateActionsLogRepository.IsExist(logEntryHash);
        }

        public void InsertActionLogEntry(XmlDbUpdateActionsLogModel logEntry)
        {
            _dbUpdateActionsLogRepository.Insert(logEntry);
        }
    }
}

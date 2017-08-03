using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.BLL.Repository.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate.Interfaces;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate
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

        public bool IsFileAlreadyReplayed(string hash) => _dbUpdateLogRepository.IsExist(hash);

        public List<string> GetExistedHashes(List<string> hashes) => _dbUpdateLogRepository.GetExistedHashes(hashes);

        public int InsertFileLogEntry(XmlDbUpdateLogModel dbLogEntry) => _dbUpdateLogRepository.Insert(dbLogEntry);

        public bool IsActionAlreadyReplayed(string logEntryHash) => _dbUpdateActionsLogRepository.IsExist(logEntryHash);

        public void InsertActionLogEntry(XmlDbUpdateActionsLogModel logEntry)
        {
            _dbUpdateActionsLogRepository.Insert(logEntry);
        }
    }
}

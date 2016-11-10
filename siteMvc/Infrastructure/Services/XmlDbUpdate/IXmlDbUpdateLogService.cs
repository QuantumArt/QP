using Quantumart.QP8.BLL.Models.XmlDbUpdate;

namespace Quantumart.QP8.BLL.Services.XmlDbUpdate
{
    public interface IXmlDbUpdateLogService
    {
        void InsertActionLogEntry(XmlDbUpdateActionsLogModel logEntry);

        int InsertFileLogEntry(XmlDbUpdateLogModel dbLogEntry);

        bool IsActionAlreadyReplayed(string logEntryHash);

        bool IsFileAlreadyReplayed(string hash);
    }
}

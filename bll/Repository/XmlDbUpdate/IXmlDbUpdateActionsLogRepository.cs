using Quantumart.QP8.BLL.Models.XmlDbUpdate;

namespace Quantumart.QP8.BLL.Repository.XmlDbUpdate
{
    public interface IXmlDbUpdateActionsLogRepository
    {
        int Insert(XmlDbUpdateActionsLogModel entry);

        bool IsExist(string hash);
    }
}

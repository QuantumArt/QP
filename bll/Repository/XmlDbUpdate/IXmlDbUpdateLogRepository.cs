using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;

namespace Quantumart.QP8.BLL.Repository.XmlDbUpdate
{
    public interface IXmlDbUpdateLogRepository
    {
        int Insert(XmlDbUpdateLogModel entry);

        bool IsExist(string hash);

        List<string> GetExistedHashes(List<string> hashes);
    }
}

using System;

namespace Quantumart.QP8.BLL.Services.XmlDbUpdate
{
    public interface IXmlDbUpdateActionService
    {
        int GetArticleIdByGuid(string guid);

        int GetArticleIdByGuid(Guid guid);
    }
}

using System;
using Quantumart.QP8.BLL.Repository.Articles;

namespace Quantumart.QP8.BLL.Services.XmlDbUpdate
{
    public class XmlDbUpdateActionService : IXmlDbUpdateActionService
    {
        public int GetArticleIdByGuid(string guid)
        {
            return GetArticleIdByGuid(Guid.Parse(guid));
        }

        public int GetArticleIdByGuid(Guid guid)
        {
            return ArticleRepository.GetByGuid(guid).Id;
        }
    }
}

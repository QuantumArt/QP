using System;
using Quantumart.QP8.BLL.Repository.Articles;

namespace Quantumart.QP8.BLL.Services.XmlDbUpdate
{
    public class XmlDbUpdateActionService : IXmlDbUpdateActionService
    {
        public int GetArticleIdByGuid(string rawGuid)
        {
            Guid guid;
            if (!Guid.TryParse(rawGuid, out guid))
            {
                throw new Exception($"Неверный формат GUID: {rawGuid}");
            }

            return GetArticleIdByGuid(guid);
        }

        public int GetArticleIdByGuid(Guid guid)
        {
            var article = ArticleRepository.GetByGuid(guid);
            if (article == null)
            {
                throw new Exception($"Не найдена статья с заданным GUID: {guid}");
            }

            return article.Id;
        }
    }
}

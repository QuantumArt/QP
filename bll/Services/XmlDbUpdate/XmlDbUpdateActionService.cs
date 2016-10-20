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
            var articleId = GetArticleIdByGuidOrDefault(guid);
            if (!articleId.HasValue)
            {
                throw new Exception($"Не найдена статья с заданным GUID: {guid}");
            }

            return articleId.Value;
        }

        public int? GetArticleIdByGuidOrDefault(Guid guid)
        {
            return ArticleRepository.GetByGuid(guid)?.Id;
        }
    }
}

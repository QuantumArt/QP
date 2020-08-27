using System;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Repository.ArticleRepositories
{
    public interface IArticleRepository
    {
        Article GetById(int id);

        int GetIdByGuid(Guid guid);

        int[] GetIdsByGuids(Guid[] guids);

        Guid[] GetGuidsByIds(int[] ids);

        List<Article> GetByIds(int[] ids);

        bool IsExist(int id);

        int GetContentId(int id);
    }
}

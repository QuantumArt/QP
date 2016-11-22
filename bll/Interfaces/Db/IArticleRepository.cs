using System;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Interfaces.Db
{
    public interface IArticleRepository
    {
        Article GetById(int id);

        Article GetByGuid(Guid guid);

        List<Article> GetByIds(int[] ids);

        bool IsExist(int id);
    }
}

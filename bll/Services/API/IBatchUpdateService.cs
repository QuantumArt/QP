using System.Collections.Generic;
using Quantumart.QP8.BLL.Services.API.Models;

namespace Quantumart.QP8.BLL.Services.API
{
    public interface IBatchUpdateService
    {
        InsertData[] BatchUpdate(IEnumerable<Article> articles);

        InsertData[] BatchUpdate(IEnumerable<ArticleData> articles);
    }
}

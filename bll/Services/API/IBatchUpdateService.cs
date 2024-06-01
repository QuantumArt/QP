using System.Collections.Generic;
using Quantumart.QP8.BLL.Services.API.Models;

namespace Quantumart.QP8.BLL.Services.API
{
    public interface IBatchUpdateService
    {
        InsertData[] BatchUpdate(IEnumerable<Article> articles, bool createVersions = false);

        InsertData[] BatchUpdate(IEnumerable<ArticleData> articlesData, bool createVersions = false);

        InsertData[] BatchUpdate(BatchUpdateModel model);
    }
}

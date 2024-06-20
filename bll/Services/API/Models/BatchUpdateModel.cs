using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Services.API.Models;

public class BatchUpdateModel
{
    public ArticleData[] Articles { get; set; }

    public bool FormatArticleData { get; set; }

    public bool CreateVersions { get; set; }

    public bool CheckSecurity { get; set; } = true;

    public bool CreateTransactionScope { get; set; } = true;

    public PathHelper PathHelper { get; set; }
}

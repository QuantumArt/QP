using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ArticleInitListResult : ArticleResult
    {
        public string TitleFieldName { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<Field> DisplayFields { get; set; }
    }
}

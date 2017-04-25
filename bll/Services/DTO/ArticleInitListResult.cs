using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ArticleInitListResult : ArticleResultBase
    {
        public string TitleFieldName { get; set; }
		public int PageSize { get; set; }
		public IEnumerable<Field> DisplayFields { get; set; }		
    }
}

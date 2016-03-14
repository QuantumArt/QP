using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ArticleInitListResult : ArticleResultBase
    {
        public string TitleFieldName { get; set; }
		public int PageSize { get; set; }
		public IEnumerable<Field> DisplayFields { get; set; }		
    }
}

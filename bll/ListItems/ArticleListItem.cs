using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
    public class ArticleListItem
    {
        public decimal Id { get; set; }
		public decimal ParentId { get; set; }
        public string Title { get; set; }
        public bool IsPermanentLock { get; set; }
        public string SiteName { get; set; }
        public string ContentName { get; set; }
        public string StatusName { get; set; }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string LastModifiedByUser { get; set; }
    }
}

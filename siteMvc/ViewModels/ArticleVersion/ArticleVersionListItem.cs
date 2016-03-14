using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ArticleVersionListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

		public string Modified { get; set; }

		public string LastModifiedByUser { get; set; }
    }
}
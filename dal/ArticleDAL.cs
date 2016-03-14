using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.DAL
{
	public partial class ArticleDAL
	{
		public ArticleDAL()
		{
			Name = String.Empty;
			AliasForTree = String.Empty;
			ParentId = null;
			HasChildren = false;
		}
		
		public string Name { get; set; }

		public string AliasForTree { get; set; }

		public decimal? ParentId { get; set; }

		public bool HasChildren { get; set; }
	}
}

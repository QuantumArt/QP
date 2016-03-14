using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class ArticleInitTreeResult : ArticleResultBase
    {
		public string Filter { get; set; }

		public bool AutoCheckChildren { get; set; }

		public ArticleInitTreeResult(Content content, bool isMultipleSelection)
		{
			this.IsVirtual = content.IsVirtual;
			this.ContentName = content.Name;
			this.IsUpdatable = content.IsUpdatable;
			this.Filter = content.SelfRelationFieldFilter;
			this.AutoCheckChildren = (isMultipleSelection && content.TreeField != null) ? content.TreeField.AutoCheckChildren : false;

		}

    }
}

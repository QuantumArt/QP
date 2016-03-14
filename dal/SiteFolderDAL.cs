using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.DAL
{
	public partial class SiteFolderDAL
	{
		public SiteFolderDAL()
		{
			HasChildren = false;
		}

		public bool HasChildren { get; set; }

	}
}

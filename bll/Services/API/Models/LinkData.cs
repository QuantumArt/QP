using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.API.Models
{
	public class LinkData
	{
		public int LinkId { get; set; }
		public int ItemId { get; set; }
		public int? LinkedItemId { get; set; }

		public override string ToString()
		{
			return new
			{
				LinkId,
				ItemId,
				LinkedItemId								
			}.ToString();
		}
	}
}

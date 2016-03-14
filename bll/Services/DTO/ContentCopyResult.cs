using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class ContentCopyResult : CopyResult
	{
		public int[] FieldIds { get; set; }

		public int[] LinkIds { get; set; }
	}
}

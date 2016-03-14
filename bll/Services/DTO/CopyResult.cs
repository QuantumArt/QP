using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
	[Serializable]
	public class CopyResult
	{
		public MessageResult Message { get; set; }

		public int Id { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class FieldInitListResult : InitListResultBase
	{

		public FieldInitListResult()
		{
			IsVirtual = false;
		}

		public List<Field> Data { get; set; }

		public string ParentName { get; set; }

		public bool IsVirtual { get; set; }		
	}
}

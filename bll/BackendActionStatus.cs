using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Quantumart.QP8;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL
{
	public class BackendActionStatus
	{

		public string Code
		{
			get;
			set;
		}


		public bool Visible
		{
			get;
			set;
		}
	}
}

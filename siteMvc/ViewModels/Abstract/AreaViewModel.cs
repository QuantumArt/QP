using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public abstract class AreaViewModel : ViewModel	
	{
		public override MainComponentType MainComponentType
		{
			get { return MainComponentType.Area; }
		}

		public override string MainComponentId
		{
			get { return UniqueId("Area"); }
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class CustomCssViewModel
	{
		private IEnumerable<B.StatusType> statuses;

		public CustomCssViewModel(IEnumerable<B.StatusType> statuses)
		{
			this.statuses = statuses;
		}

		public IEnumerable<B.StatusType> Statuses { get { return statuses; } }
	}	
}
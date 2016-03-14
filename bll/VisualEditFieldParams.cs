using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	internal class VisualEditFieldParams
	{
		public bool PEnterMode { get; set; }
		public bool UseEnglishQuotes { get; set; }
		public string ExternalCss { get; set; }
		public string RootElementClass { get; set; }

		public VisualEditFieldParams()
		{

		}

		public VisualEditFieldParams(Site site)
		{
			PEnterMode = site.PEnterMode;
			UseEnglishQuotes = site.UseEnglishQuotes;
			ExternalCss = site.ExternalCss;
			RootElementClass = site.RootElementClass;
		}
	}
}

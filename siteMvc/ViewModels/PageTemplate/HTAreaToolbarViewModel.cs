using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public class HTAreaToolbarViewModel
	{
		public HTAreaToolbarViewModel(bool presentationOrCodeBehind, int? formatId, int? templateId)
		{
			PresentationOrCodeBehind = presentationOrCodeBehind;
			FormatId = formatId;
			TemplateId = templateId;
		}

		public bool PresentationOrCodeBehind { get; set; }

		public int? FormatId { get; set; }

		public int? TemplateId { get; set; }
	}
}
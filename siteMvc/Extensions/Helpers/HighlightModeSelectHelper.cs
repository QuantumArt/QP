using Quantumart.QP8.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public static class HighlightModeSelectHelper
	{
		private const string _CsharpMode = "hta-cSharpTextArea";
		private const string _VbsMode = "hta-VBSTextArea";
		private const string _VbMode = "hta-VBTextArea";

		public static string SelectMode(int? languageId)
		{
			if (NetLanguage.GetcSharp().Id == languageId)
				return _CsharpMode;
			if (NetLanguage.GetVbNet().Id == languageId)
				return _VbMode;
			return _VbsMode;
		}
	}
}
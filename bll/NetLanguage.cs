using Quantumart.QP8.BLL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
    public class NetLanguage : EntityObject
    {
		private const string cSharp = "C#";
		private const string vbNet = "VB.NET";
		public static NetLanguage GetcSharp()
		{
			return PageTemplateRepository.GetNetLanguageByName(cSharp);
		}

		public static NetLanguage GetVbNet()
		{
			return PageTemplateRepository.GetNetLanguageByName(vbNet);
		}
    }
}

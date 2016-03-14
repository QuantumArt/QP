using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.ViewModels.DirectLink
{
	/// <summary>
	/// Параметры внешней ссылки
	/// </summary>
	public class DirectLinkOptions
	{
		public string customerCode { get; set; }
		public string actionCode { get; set; }
		public string entityTypeCode { get; set; }
		public int? entityId { get; set; }
		public int? parentEntityId { get; set; }

		public string ToUrlParams()
		{ 
			if (IsDefined())
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("actionCode={0}&entityTypeCode={1}", actionCode, entityTypeCode);
				if (!String.IsNullOrEmpty(customerCode))
					sb.AppendFormat("&customerCode={0}", customerCode);
				if (entityId.HasValue)
					sb.AppendFormat("&entityId={0}", entityId);
				if (parentEntityId.HasValue)
					sb.AppendFormat("&parentEntityId={0}", parentEntityId);
				return sb.ToString();
			}
			else
				return String.Empty;
		}

		public string AddToUrl(string url)
		{
			if (IsDefined())
			{
				if (Url.IsQueryEmpty(url))
					return url + "?" + ToUrlParams();
				else
					return url + "&" + ToUrlParams();
			}
			else
				return url;
		}

		public bool IsDefined()
		{
			return !String.IsNullOrEmpty(actionCode) 
				&& !String.IsNullOrEmpty(entityTypeCode)
				&& (entityId.HasValue || parentEntityId.HasValue);
		}


	}
}
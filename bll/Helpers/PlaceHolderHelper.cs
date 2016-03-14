using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Helpers
{
	public class PlaceHolderHelper
	{
		private static string UploadUrlPlaceHolder = "<%=upload_url%>";
		private static string SiteUrlPlaceHolder = "<%=site_url%>";

		public static string ReplacePlaceHoldersToUrls(Site site, string value)
		{
			if (!String.IsNullOrEmpty(value))
				return value.Replace(UploadUrlPlaceHolder, site.ImagesLongUploadUrl).Replace(SiteUrlPlaceHolder, site.CurrentUrl);
			else
				return value;
		}

		public static string ReplaceUrlsToPlaceHolders(Site site, string value)
		{
			if (!String.IsNullOrEmpty(value))
				return value
					.Replace(site.ImagesLongUploadUrl, UploadUrlPlaceHolder)
					.Replace(site.StageUrl, SiteUrlPlaceHolder)
					.Replace(site.LiveUrl, SiteUrlPlaceHolder);
			else
				return value;
		}

	}
}

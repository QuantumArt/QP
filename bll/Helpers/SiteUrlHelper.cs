using Quantumart.QP8.BLL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Helpers
{
	public static class SiteUrlHelper
	{
		public static Dictionary<string, string> GetSiteUrlsByObjectId(int objectId)
		{			
			var obj = ObjectRepository.GetObjectPropertiesById(objectId);
			var site = obj.PageTemplate.Site;
			return GetSiteUrls(site);
		}

		public static Dictionary<string, string> GetSiteUrlsBySiteId(int siteId)
		{
			var site = SiteRepository.GetById(siteId);
			return GetSiteUrls(site);
		}

		private static Dictionary<string, string> GetSiteUrls(Site site)
		{
			return new Dictionary<string, string>()
			{
				{"ImagesLongUploadUrl", site.ImagesLongUploadUrl},
				{"StageUrl", site.StageUrl},
				{"LiveUrl", site.LiveUrl},
				{"CurrentUrl", site.CurrentUrl}
			};
		}
	}
}

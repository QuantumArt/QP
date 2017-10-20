using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository;

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

		private static Dictionary<string, string> GetSiteUrls(Site site) => new Dictionary<string, string>
		{
		    {"ImagesLongUploadUrl", site.ImagesLongUploadUrl},
		    {"StageUrl", site.StageUrl},
		    {"LiveUrl", site.LiveUrl},
		    {"CurrentUrl", site.CurrentUrl}
		};
	}
}

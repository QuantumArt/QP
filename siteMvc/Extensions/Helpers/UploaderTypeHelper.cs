using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using System.Configuration;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public static class UploaderTypeHelper
	{
		public static UploaderType UploaderType
		{
			get
			{
                if (QPConfiguration.WebConfigSection.UploaderTypeName.Equals("plupload", StringComparison.InvariantCultureIgnoreCase))
                    return UploaderType.PlUpload;
                else
                {

                    if (!QPContext.CurrentUserIdentity.IsSilverlightInstalled || HttpContext.Current.Request.Browser.Browser.Equals("Chrome", StringComparison.InvariantCultureIgnoreCase))
                        return ViewModels.UploaderType.Html;
                    else if (QPConfiguration.WebConfigSection.UploaderTypeName.Equals("silverlight", StringComparison.InvariantCultureIgnoreCase) ||
                        QPConfiguration.WebConfigSection.UploaderTypeName.Equals("html", StringComparison.InvariantCultureIgnoreCase))
                        return (UploaderType)Enum.Parse(typeof(UploaderType), QPConfiguration.WebConfigSection.UploaderTypeName, true);
                    else
                        return UploaderType.Silverlight;
                }
			}
		}
	}
}
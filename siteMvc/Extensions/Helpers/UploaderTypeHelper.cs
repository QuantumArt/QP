using System;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.WebMvc.ViewModels;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class UploaderTypeHelper
    {
        public static UploaderType UploaderType
        {
            get
            {
                if (QPConfiguration.WebConfigSection.UploaderTypeName.Equals("plupload", StringComparison.InvariantCultureIgnoreCase))
                {
                    return UploaderType.PlUpload;
                }

                if (!QPContext.CurrentUserIdentity.IsSilverlightInstalled || HttpContext.Current.Request.Browser.Browser.Equals("Chrome", StringComparison.InvariantCultureIgnoreCase))
                {
                    return UploaderType.Html;
                }

                if (QPConfiguration.WebConfigSection.UploaderTypeName.Equals("silverlight", StringComparison.InvariantCultureIgnoreCase) || QPConfiguration.WebConfigSection.UploaderTypeName.Equals("html", StringComparison.InvariantCultureIgnoreCase))
                {
                    return (UploaderType)Enum.Parse(typeof(UploaderType), QPConfiguration.WebConfigSection.UploaderTypeName, true);
                }

                return UploaderType.Silverlight;
            }
        }
    }
}

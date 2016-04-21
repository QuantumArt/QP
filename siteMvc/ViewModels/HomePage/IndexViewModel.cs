using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using System.Dynamic;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public class IndexViewModel
    {
        private readonly DirectLinkOptions _directLinkOptions;

        public Db Data { get; }

        public string DbHash { get; }

        public string Title
        {
            get
            {
                var configTitle = QPConfiguration.ApplicationTitle.Replace("{release}", Default.ReleaseNumber);
                var instanceName = QPConfiguration.WebConfigSection.InstanceName;
                if (!string.IsNullOrEmpty(configTitle) && !string.IsNullOrEmpty(instanceName))
                {
                    return instanceName + " " + configTitle;
                }

                return !string.IsNullOrEmpty(configTitle) ? configTitle : "QP8 Backend";
            }
        }

        public MvcHtmlString BackendComponentOptions
        {
            get
            {
                dynamic result = new ExpandoObject();
                result.currentCustomerCode = QPContext.CurrentCustomerCode;
                result.currentUserId = QPContext.CurrentUserId;
                result.autoLoadHome = Data.AutoOpenHome;
                if (_directLinkOptions != null && _directLinkOptions.IsDefined())
                {
                    result.directLinkOptions = _directLinkOptions;
                }

                return MvcHtmlString.Create(((ExpandoObject)result).ToJson());
            }
        }

        public IndexViewModel(DirectLinkOptions directLinkOptions, Db data, string dbHash)
        {
            _directLinkOptions = directLinkOptions;
            Data = data;
            DbHash = dbHash;
        }
    }
}

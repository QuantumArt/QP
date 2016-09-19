using System.Dynamic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public class IndexViewModel
    {
        private readonly DirectLinkOptions _directLinkOptions;
        private const string BackendTitle = "QP8 Backend";

        public IndexViewModel(DirectLinkOptions directLinkOptions, Db data, string dbHash)
        {
            _directLinkOptions = directLinkOptions;
            Data = data;
            DbHash = dbHash;
        }

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

                return !string.IsNullOrEmpty(configTitle) ? configTitle : BackendTitle;
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
    }
}

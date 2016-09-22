using System.Dynamic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public class IndexViewModel
    {
        private const string BackendTitle = "QP8 Backend";
        private readonly DirectLinkOptions _directLinkOptions;

        public IndexViewModel(DirectLinkOptions directLinkOptions, Db data, string dbHash)
        {
            Data = data;
            DbHash = dbHash;
            _directLinkOptions = directLinkOptions;
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
                result.CurrentCustomerCode = QPContext.CurrentCustomerCode;
                result.CurrentUserId = QPContext.CurrentUserId;
                result.AutoLoadHome = Data.AutoOpenHome;

                if (_directLinkOptions != null && _directLinkOptions.IsDefined())
                {
                    result.DirectLinkOptions = _directLinkOptions;
                }

                return MvcHtmlString.Create(((object)result).ToJsonLog());
            }
        }
    }
}

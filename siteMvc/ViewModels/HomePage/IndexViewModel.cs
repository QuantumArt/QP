using System.Dynamic;
using Microsoft.AspNetCore.Html;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.DirectLink;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public class IndexViewModel
    {
        private readonly DirectLinkOptions _directLinkOptions;
        private readonly QPublishingOptions _options;

        public IndexViewModel(DirectLinkOptions directLinkOptions, Db data, string dbHash, QPublishingOptions options)
        {
            Data = data;
            DbHash = dbHash;
            _directLinkOptions = directLinkOptions;
            _options = options;
        }

        public Db Data { get; }

        public string DbHash { get; }

        public string Title
        {
            get
            {
                var configTitle = QPConfiguration.ApplicationTitle.Replace("{release}", Default.ReleaseNumber);
                var instanceName = QPConfiguration.Options.InstanceName;
                if (!string.IsNullOrEmpty(configTitle) && !string.IsNullOrEmpty(instanceName))
                {
                    return instanceName + " " + configTitle;
                }

                return !string.IsNullOrEmpty(configTitle) ? configTitle : "QP8 Backend";
            }
        }

        public IHtmlContent BackendComponentOptions
        {
            get
            {
                dynamic result = new ExpandoObject();
                result.CurrentCustomerCode = QPContext.CurrentCustomerCode;
                result.CurrentUserId = QPContext.CurrentUserId;
                result.AutoLoadHome = Data.AutoOpenHome;
                result.MustChangePassword = UserService.GetUserMustChangePassword(QPContext.CurrentUserId);

                if (_directLinkOptions != null && _directLinkOptions.IsDefined())
                {
                    result.DirectLinkOptions = _directLinkOptions;
                }

                return new HtmlString(((object)result).ToJsonLog());
            }
        }
    }
}

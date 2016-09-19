using System;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public sealed class AboutViewModel : AreaViewModel
    {
        public static AboutViewModel Create(string tabId, int parentId)
        {
            return Create<AboutViewModel>(tabId, parentId);
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.About;

        [LocalizedDisplayName("ProductName", NameResourceType = typeof(HomeStrings))]
        public string Product => string.Format(HomeStrings.ProductValue, Default.ReleaseNumber, DateTime.Now.Year);

        [LocalizedDisplayName("DBVersion", NameResourceType = typeof(HomeStrings))]
        public string DbVersion => new ApplicationInfoHelper().GetCurrentDbVersion();

        [LocalizedDisplayName("BackendVersion", NameResourceType = typeof(HomeStrings))]
        public string BackendVersion => new ApplicationInfoHelper().GetCurrentBackendVersion();

        [LocalizedDisplayName("DBName", NameResourceType = typeof(HomeStrings))]
        public string DbName => DbRepository.GetDbName();

        [LocalizedDisplayName("DBServerName", NameResourceType = typeof(HomeStrings))]
        public string DbServerName => DbRepository.GetDbServerName();
    }
}

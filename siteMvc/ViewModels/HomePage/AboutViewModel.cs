using System;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public sealed class AboutViewModel : AreaViewModel
    {
        public static AboutViewModel Create(string tabId, int parentId, string version)
        {
            var model = Create<AboutViewModel>(tabId, parentId);
            model.Version = version;
            return model;
        }

        public string Version { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.About;

        [Display(Name = "ProductName", ResourceType = typeof(HomeStrings))]
        public string Product => string.Format(HomeStrings.ProductValue, Version, DateTime.Now.Year);

        [Display(Name = "DBName", ResourceType = typeof(HomeStrings))]
        public string DbName => DbRepository.GetDbName();

        [Display(Name = "DBServerName", ResourceType = typeof(HomeStrings))]
        public string DbServerName => DbRepository.GetDbServerName();
    }
}

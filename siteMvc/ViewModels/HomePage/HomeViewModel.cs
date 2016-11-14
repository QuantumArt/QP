using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public sealed class HomeViewModel : AreaViewModel
    {
        public static HomeViewModel Create(string tabId, int parentId, HomeResult result)
        {
            var model = Create<HomeViewModel>(tabId, parentId);
            model.Sites = result.Sites;
            model.CurrentUser = result.CurrentUser;
            model.LockedCount = result.LockedCount;
            model.ApprovalCount = result.ApprovalCount;
            return model;
        }

        [LocalizedDisplayName("LoggedAs", NameResourceType = typeof(HomeStrings))]
        public string LoggedAs => CurrentUser.FullName;

        [LocalizedDisplayName("Search", NameResourceType = typeof(HomeStrings))]
        public string Search { get; set; }

        [LocalizedDisplayName("Site", NameResourceType = typeof(HomeStrings))]
        public int SiteId { get; set; }

        [LocalizedDisplayName("LockedCount", NameResourceType = typeof(HomeStrings))]
        public int LockedCount { get; set; }

        [LocalizedDisplayName("ApprovalCount", NameResourceType = typeof(HomeStrings))]
        public int ApprovalCount { get; set; }

        public IEnumerable<ListItem> Sites { get; set; }

        public BLL.User CurrentUser { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.Home;

        public string SiteElementId => UniqueId("siteElement");

        public string LockedElementId => UniqueId("lockedElement");

        public string SearchElementId => UniqueId("searchElement");

        public string ApprovalElementId => UniqueId("approvalElement");

        public string LoggedAsElementId => UniqueId("loggedAsElement");

        public string CustomerCode => QPContext.CurrentCustomerCode;
    }

}

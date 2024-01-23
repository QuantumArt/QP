using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.HomePage
{
    public sealed class HomeViewModel : AreaViewModel
    {
        public static HomeViewModel Create(string tabId, int parentId, HomeResult result, int taskCount)
        {
            var model = Create<HomeViewModel>(tabId, parentId);
            model.Sites = result.Sites;
            model.CurrentUser = result.CurrentUser;
            model.LockedCount = result.LockedCount;
            model.ApprovalCount = result.ApprovalCount;
            model.ExternalUserTaskCount = taskCount;
            return model;
        }

        [Display(Name = "LoggedAs", ResourceType = typeof(HomeStrings))]
        public string LoggedAs => CurrentUser.FullName;

        [Display(Name = "Search", ResourceType = typeof(HomeStrings))]
        public string Search { get; set; }

        [Display(Name = "Site", ResourceType = typeof(HomeStrings))]
        public int SiteId { get; set; }

        [Display(Name = "LockedCount", ResourceType = typeof(HomeStrings))]
        public int LockedCount { get; set; }

        [Display(Name = "ApprovalCount", ResourceType = typeof(HomeStrings))]
        public int ApprovalCount { get; set; }

        [Display(Name = "ExternalUserTaskCount", ResourceType = typeof(HomeStrings))]
        public int ExternalUserTaskCount { get; set; }

        public IEnumerable<ListItem> Sites { get; set; }

        [BindNever]
        [ValidateNever]
        public BLL.User CurrentUser { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.Home;

        public string SiteElementId => UniqueId("siteElement");

        public string LockedElementId => UniqueId("lockedElement");

        public string SearchElementId => UniqueId("searchElement");

        public string ApprovalElementId => UniqueId("approvalElement");

        public string LoggedAsElementId => UniqueId("loggedAsElement");

        public string ExternalUserTaskElementId => UniqueId("externalUserTaskElement");

        public string CustomerCode => QPContext.CurrentCustomerCode;
    }
}

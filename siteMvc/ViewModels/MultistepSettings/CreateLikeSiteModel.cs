using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.MultistepSettings
{
    public class CreateLikeSiteModel : EntityViewModel
    {
        public readonly string BinBlock = "binBlock";
        public readonly string StageDnsBlock = "stageDnsBlock";
        public readonly string UploadUrlPrefixBlock = "uploadUrlPrefixBlock";
        public readonly string TestDirectoryPathBlock = "testDirectoryPathBlock";
        public readonly string TestDirectoryBlock = "testDirectoryBlock";
        public readonly string LivePagesLocationBlock = "LivePagesLocationBlock";
        public readonly string StagePagesLocationBlock = "StagePagesLocationBlock";
        public readonly string BeginNotCopyTemplatesBlock = "BeginNotCopyTemplates";

        public override string EntityTypeCode => Constants.EntityTypeCode.Site;

        public override string ActionCode => Constants.ActionCode.CreateLikeSite;

        [Display(Name = "DoNotCopyFiles", ResourceType = typeof(SiteStrings))]
        public bool DoNotCopyFiles { get; set; }

        [Display(Name = "DoNotCopyArticles", ResourceType = typeof(SiteStrings))]
        public int? DoNotCopyArticles { get; set; }

        [Display(Name = "DoNotCopyTemplates", ResourceType = typeof(SiteStrings))]
        public bool DoNotCopyTemplates { get; set; }

        [BindNever]
        [ValidateNever]
        public BLL.Site Data
        {
            get => (BLL.Site)EntityData;
            set => EntityData = value;
        }

        public static CreateLikeSiteModel Create(BLL.Site site, string tabId, int parentId)
        {
            var model = Create<CreateLikeSiteModel>(site, tabId, parentId);
            return model;
        }
    }
}

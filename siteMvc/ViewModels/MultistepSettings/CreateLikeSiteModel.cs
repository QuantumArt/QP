using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class CreateLikeSiteModel : EntityViewModel
    {
        public readonly string BinBlock = "binBlock";
        public readonly string StageDnsBlock = "stageDnsBlock";
        public readonly string UploadUrlPrefixBlock = "uploadUrlPrefixBlock";
        public readonly string TestDirectoryPathBlock = "testDirectoryPathBlock";
        public readonly string TestDirectoryBlock = "testDirectoryBlock";

        public override string EntityTypeCode => Constants.EntityTypeCode.Site;

        public override string ActionCode => Constants.ActionCode.CreateLikeSite;

        [LocalizedDisplayName("DoNotCopyFiles", NameResourceType = typeof(SiteStrings))]
        public bool DoNotCopyFiles { get; set; }

        [LocalizedDisplayName("DoNotCopyArticles", NameResourceType = typeof(SiteStrings))]
        public int? DoNotCopyArticles { get; set; }

        [LocalizedDisplayName("DoNotCopyTemplates", NameResourceType = typeof(SiteStrings))]
        public bool DoNotCopyTemplates { get; set; }

        public new Site Data
        {
            get
            {
                return (Site)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public override void Validate(ModelStateDictionary modelState)
        {
            modelState.Clear();
            base.Validate(modelState);
        }

        public static CreateLikeSiteModel Create(Site site, string tabId, int parentId)
        {
            var model = Create<CreateLikeSiteModel>(site, tabId, parentId);
            return model;
        }
    }
}

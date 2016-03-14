using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Con = Quantumart.QP8.Constants;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.BLL;
using System.Web.Mvc;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class CreateLikeSiteModel : EntityViewModel
    {
        public readonly string BinBlock = "binBlock";
        public readonly string StageDnsBlock = "stageDnsBlock";
        public readonly string UploadUrlPrefixBlock = "uploadUrlPrefixBlock";
        public readonly string TestDirectoryPathBlock = "testDirectoryPathBlock";
        public readonly string TestDirectoryBlock = "testDirectoryBlock";

        public override string EntityTypeCode
        {
            get { return Con.EntityTypeCode.Site; }
        }

        public override string ActionCode
        {
            get { return Con.ActionCode.CreateLikeSite; }
        }

        [LocalizedDisplayName("DoNotCopyFiles", NameResourceType = typeof(SiteStrings))]
        public bool DoNotCopyFiles
        {
            get;
            set;
        }

        [LocalizedDisplayName("DoNotCopyArticles", NameResourceType = typeof(SiteStrings))]
        public int? DoNotCopyArticles
        {
            get;
            set;
        }

        [LocalizedDisplayName("DoNotCopyTemplates", NameResourceType = typeof(SiteStrings))]
        public bool DoNotCopyTemplates
        {
            get;
            set;
        }

        public new B.Site Data
        {
            get
            {
                return (B.Site)EntityData;
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
            var model = EntityViewModel.Create<CreateLikeSiteModel>(site, tabId, parentId);
            return model;
        }
    }
}
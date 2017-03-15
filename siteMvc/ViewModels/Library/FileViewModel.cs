using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public class FileViewModel : EntityViewModel
    {
        public FileViewModel()
        {
            IsSite = true;
        }

        public static FileViewModel Create(FolderFile file, string tabId, int parentId, bool isSite)
        {
            var model = new FileViewModel
            {
                File = file,
                TabId = tabId,
                ParentEntityId = parentId,
                IsSite = isSite
            };

            return model;
        }

        public FolderFile File { get; set; }

        public bool IsSite { get; set; }

        public override string EntityTypeCode => IsSite ? Constants.EntityTypeCode.SiteFile : Constants.EntityTypeCode.ContentFile;

        public override string ActionCode => IsSite ? Constants.ActionCode.SiteFileProperties : Constants.ActionCode.ContentFileProperties;

        public override void Validate(ModelStateDictionary modelState)
        {
            try
            {
                File.Validate();
            }
            catch (RulesException ex)
            {
                ex.Extend(modelState, "Data");
                IsValid = false;
            }
        }

        public override string Id => File.Name;

        public override string Name => File.Name;
    }
}

using Quantumart.QP8.BLL;
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

        public override void Validate()
        {
            File.Validate();
        }

        public override void DoCustomBinding()
        {
        }

        public override string Id => File.Name;

        public override string Name => File.Name;
    }
}

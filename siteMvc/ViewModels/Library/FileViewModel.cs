using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using SixLabors.ImageSharp;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public class FileViewModel : EntityViewModel
    {
        public FileViewModel()
        {
            IsSite = true;
        }

        public static FileViewModel Create(FolderFile file, string tabId, int parentId, bool isSite, PathHelper pathHelper)
        {
            var model = new FileViewModel
            {
                File = file,
                TabId = tabId,
                ParentEntityId = parentId,
                IsSite = isSite,
                PathHelper = pathHelper
            };

            return model;
        }

        public FolderFile File { get; set; }

        public PathHelper PathHelper { get; set; }

        public bool IsSite { get; set; }

        public override string EntityTypeCode => IsSite ? Constants.EntityTypeCode.SiteFile : Constants.EntityTypeCode.ContentFile;

        public override string ActionCode => IsSite ? Constants.ActionCode.SiteFileProperties : Constants.ActionCode.ContentFileProperties;

        public override void Validate()
        {
            File.Validate(PathHelper);
        }

        public override void DoCustomBinding()
        {
        }

        public override string Id => File.Name;

        public override string Name => File.Name;


        private string _dimensions;
        /// <summary>
        /// Размеры изображения (только для картинок)
        /// </summary>
        [Display(Name = "Dimensions", ResourceType = typeof(LibraryStrings))]
        [BindNever]
        [ValidateNever]
        public string Dimensions
        {
            get
            {
                if (_dimensions == null)
                {
                    _dimensions = string.Empty;
                    if (File.FileType == FolderFileType.Image)
                    {
                        try
                        {
                            var image = PathHelper.IdentifyImage(File.FullName);
                            _dimensions = string.Format("{0}x{1}", image.Width, image.Height);
                        }
                        catch (UnknownImageFormatException)
                        {
                            _dimensions = string.Empty;
                        }
                    }
                }
                return _dimensions;
            }
        }

    }
}

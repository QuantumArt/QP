using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public abstract class FolderViewModel : EntityViewModel
    {
        [Required]
        public Folder Data
        {
            get => (Folder)EntityData;
            set => EntityData = value;
        }
    }
}

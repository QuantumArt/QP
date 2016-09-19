using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public abstract class FolderViewModel : EntityViewModel
    {
        public new Folder Data
        {
            get
            {
                return (Folder)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }
    }
}

using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class LockableEntityViewModel : EntityViewModel
    {
        public new LockableEntityObject Data
        {
            get { return (LockableEntityObject)EntityData; }
            set { EntityData = value; }
        }

        public string UnlockId => UniqueId("unlock");

        public string UnlockText => GlobalStrings.Unlock;

        public virtual string CaptureLockActionCode => string.Empty;
    }
}

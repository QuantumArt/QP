namespace Quantumart.QP8.WebMvc.ViewModels.StatusType
{
    public class StatusTypeViewModel : EntityViewModel
    {
        public override string EntityTypeCode => Constants.EntityTypeCode.StatusType;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewStatusType : Constants.ActionCode.StatusTypeProperties;

        public new BLL.StatusType Data
        {
            get
            {
                return (BLL.StatusType)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public static StatusTypeViewModel Create(BLL.StatusType status, string tabId, int parentId)
        {
            var model = Create<StatusTypeViewModel>(status, tabId, parentId);
            return model;
        }
    }
}

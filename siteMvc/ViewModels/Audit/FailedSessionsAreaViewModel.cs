using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
    public sealed class FailedSessionsAreaViewModel : AreaViewModel
    {
        public static FailedSessionsAreaViewModel Create(string tabId, int parentId)
        {
            var model = Create<FailedSessionsAreaViewModel>(tabId, parentId);
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.FailedSession;

        public string GridElementId => UniqueId("Grid");
    }
}

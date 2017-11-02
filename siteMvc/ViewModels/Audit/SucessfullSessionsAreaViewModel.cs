using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
    public sealed class SucessfullSessionsAreaViewModel : AreaViewModel
    {
        public static SucessfullSessionsAreaViewModel Create(string tabId, int parentId)
        {
            var model = Create<SucessfullSessionsAreaViewModel>(tabId, parentId);
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.SuccessfulSession;

        public string GridElementId => UniqueId("Grid");
    }
}

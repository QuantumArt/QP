namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
    public sealed class ButtonTraceAreaViewModel : AreaViewModel
    {
        public static ButtonTraceAreaViewModel Create(string tabId, int parentId) => Create<ButtonTraceAreaViewModel>(tabId, parentId);

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.ButtonTrace;

        public string GridElementId => UniqueId("Grid");
    }
}

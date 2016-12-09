using System.Dynamic;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.CustomAction
{
    public class ExecuteCustomActionViewModel : ViewModel
    {
        public static ExecuteCustomActionViewModel Create(string tabId, int parentId, int[] IDs, BLL.CustomAction customAction)
        {
            var model = Create<ExecuteCustomActionViewModel>(tabId, parentId);
            model.CustomAction = customAction;
            return model;
        }

        public BLL.CustomAction CustomAction { get; private set; }

        public string FrameElementId => UniqueId("caframe");

        public override ExpandoObject MainComponentParameters
        {
            get
            {
                dynamic result = base.MainComponentParameters;
                result.actionBaseUrl = CustomAction.FullUrl;
                result.iframeElementId = FrameElementId;
                return result;
            }
        }

        public override MainComponentType MainComponentType => MainComponentType.CustomActionHost;

        public override string MainComponentId => UniqueId("CustomActionHost");

        public override string EntityTypeCode => CustomAction.Action.EntityType.Code;

        public override string ActionCode => CustomAction.Action.Code;
    }
}

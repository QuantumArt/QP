using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.WebMvc.ViewModels.Content
{
    public class ContentViewModel : ContentViewModelBase
    {
        public readonly string SplitBlock = "split";
        public readonly string VersionsBlock = "versions";
        public readonly string XamlValidationPanel = "xamlvalidation";
        public readonly string ContextBlock = "context";

        public static ContentViewModel Create(BLL.Content content, string tabId, int parentId) => Create<ContentViewModel>(content, tabId, parentId);

        public override string EntityTypeCode => Constants.EntityTypeCode.Content;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewContent : Constants.ActionCode.ContentProperties;

        public string SiteName => Data.Site.Name;

        public List<ListItem> Workflows
        {
            get
            {
                var result = Data.Site.Workflows.Select(n => new ListItem(n.Id.ToString(), n.Name, SplitBlock)).ToList();
                result.Add(new ListItem(WorkflowBind.UnassignedId.ToString(), ContentStrings.NoWorkflow));
                return result;
            }
        }

        public override ExpandoObject MainComponentOptions
        {
            get
            {
                dynamic result = base.MainComponentOptions;
                if (Data.HasAggregatedFields) // запретить агрегированным контентам менять некоторые поля
                {
                    result.disabledFields = new[] { "Data.AutoArchive", "Data.WorkflowBinding.WorkflowId", "Data.WorkflowBinding.IsAsync" };
                }

                return result;
            }
        }
    }
}
